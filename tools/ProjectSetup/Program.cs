using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectSetup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
            var directoryArg = "-directory=";

            var directory = args.Where(s => s.StartsWith(directoryArg, StringComparison.OrdinalIgnoreCase))
                                .Select(s => s.Substring(directoryArg.Length).Trim()).FirstOrDefault();

            directory = String.IsNullOrEmpty(directory) ? Environment.CurrentDirectory : Path.GetFullPath(directory);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm() { BaseDirectory = directory });
        }
    }
}
