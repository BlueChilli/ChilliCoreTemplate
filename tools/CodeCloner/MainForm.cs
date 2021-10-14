using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeCloner
{
    public partial class MainForm : Form
    {
        BlockingCollection<MethodInvoker> _dispatchQueue;

        public string BaseDirectory
        {
            set
            {
                this.sourceDirectory.Text = value;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            _dispatchQueue = new BlockingCollection<MethodInvoker>();
            StartDispatchReader();
        }

        private void StartDispatchReader()
        {
            Task.Factory.StartNew(() =>
            {
                MethodInvoker item = null;
                while (GetNextDispatchedItem(out item))
                {
                    try
                    {
                        //Item delegate will run in UI thread.
                        this.Invoke(item);
                    }
                    catch { /* noop */ }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private bool GetNextDispatchedItem(out MethodInvoker item)
        {
            try
            {
                //blocks till item is available
                item = _dispatchQueue.Take();
            }
            catch
            {
                item = null;
            }

            return item != null;
        }

        private RenamerOptions GetRenamerOptions()
        {
            return new RenamerOptions()
            {
                SourceDirectory = sourceDirectory.Text,
                FromName = fromText.Text,
                ToName = toText.Text,
                PluralName = pluralText.Text,
                FilesToRename = filesToRename.Lines,
                FilesToSearch = filesToSearch.Lines
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.LogTextBox.Text = "";
            this.tabControl1.SelectedTab = this.logTab;
            this.button1.Enabled = false;

            Task.Run(() =>
            {
                var logger = new ExecutionLogger(WriteLogUIThread);
                logger.Log($"--- Started at {DateTime.Now.ToString("dd MMM yyyy hh:mm:ss tt")} ---");

                try
                {
                    var renamerOptions = GetRenamerOptions();
                    if (String.IsNullOrEmpty(renamerOptions.ToName))
                    {
                        logger.Log("Solution name is empty.");
                        return;
                    }

                    var renamer = new Renamer(renamerOptions, logger);
                    renamer.Run();

                    logger.Log("----------------------");
                }
                catch (Exception ex)
                {
                    logger.Log("****** Runtime Exception ******");
                    logger.Log(ex.Message);
                    logger.Log(ex.StackTrace);
                }
            }).ContinueWith((t) =>
            {
                this.RunUIThread(() =>
                {
                    this.button1.Enabled = true;
                });
            });
        }

        private void RunUIThread(Action action)
        {
            _dispatchQueue.Add(delegate
            {
                action();
            });
        }

        public void WriteLogUIThread(string text)
        {
            RunUIThread(() =>
            {
                this.LogTextBox.Text += $"{text}{Environment.NewLine}";
            });
        }
    }
}
