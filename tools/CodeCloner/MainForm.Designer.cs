namespace CodeCloner
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.fromText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.toText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.filesToSearch = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.filesToRename = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.sourceDirectory = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.logTab = new System.Windows.Forms.TabPage();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pluralText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.SettingsTab.SuspendLayout();
            this.logTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 89);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "From";
            // 
            // fromText
            // 
            this.fromText.Location = new System.Drawing.Point(202, 89);
            this.fromText.Margin = new System.Windows.Forms.Padding(4);
            this.fromText.Name = "fromText";
            this.fromText.Size = new System.Drawing.Size(738, 31);
            this.fromText.TabIndex = 2;
            this.fromText.Text = "Apple";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 137);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 25);
            this.label2.TabIndex = 5;
            this.label2.Text = "To";
            // 
            // toText
            // 
            this.toText.Location = new System.Drawing.Point(202, 131);
            this.toText.Margin = new System.Windows.Forms.Padding(4);
            this.toText.Name = "toText";
            this.toText.Size = new System.Drawing.Size(738, 31);
            this.toText.TabIndex = 4;
            this.toText.Text = "Orange";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 27);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(153, 25);
            this.label3.TabIndex = 7;
            this.label3.Text = "Files to search";
            // 
            // filesToSearch
            // 
            this.filesToSearch.Location = new System.Drawing.Point(194, 27);
            this.filesToSearch.Margin = new System.Windows.Forms.Padding(4);
            this.filesToSearch.Multiline = true;
            this.filesToSearch.Name = "filesToSearch";
            this.filesToSearch.Size = new System.Drawing.Size(322, 156);
            this.filesToSearch.TabIndex = 6;
            this.filesToSearch.Text = "*.cs\r\n*.cshtml";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 212);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(160, 25);
            this.label4.TabIndex = 9;
            this.label4.Text = "Files to rename";
            // 
            // filesToRename
            // 
            this.filesToRename.Location = new System.Drawing.Point(194, 212);
            this.filesToRename.Margin = new System.Windows.Forms.Padding(4);
            this.filesToRename.Multiline = true;
            this.filesToRename.Name = "filesToRename";
            this.filesToRename.Size = new System.Drawing.Size(322, 156);
            this.filesToRename.TabIndex = 8;
            this.filesToRename.Text = "*.cs\r\n*.cshtml";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(618, 279);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(322, 63);
            this.button1.TabIndex = 12;
            this.button1.Text = "Rename";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 42);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(169, 25);
            this.label5.TabIndex = 14;
            this.label5.Text = "Source directory";
            // 
            // sourceDirectory
            // 
            this.sourceDirectory.Location = new System.Drawing.Point(202, 42);
            this.sourceDirectory.Margin = new System.Windows.Forms.Padding(4);
            this.sourceDirectory.Name = "sourceDirectory";
            this.sourceDirectory.Size = new System.Drawing.Size(738, 31);
            this.sourceDirectory.TabIndex = 13;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.SettingsTab);
            this.tabControl1.Controls.Add(this.logTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1046, 485);
            this.tabControl1.TabIndex = 15;
            // 
            // SettingsTab
            // 
            this.SettingsTab.BackColor = System.Drawing.SystemColors.Control;
            this.SettingsTab.Controls.Add(this.label3);
            this.SettingsTab.Controls.Add(this.filesToSearch);
            this.SettingsTab.Controls.Add(this.filesToRename);
            this.SettingsTab.Controls.Add(this.label4);
            this.SettingsTab.Location = new System.Drawing.Point(8, 39);
            this.SettingsTab.Margin = new System.Windows.Forms.Padding(6);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Padding = new System.Windows.Forms.Padding(6);
            this.SettingsTab.Size = new System.Drawing.Size(1030, 438);
            this.SettingsTab.TabIndex = 0;
            this.SettingsTab.Text = "Settings";
            // 
            // logTab
            // 
            this.logTab.BackColor = System.Drawing.SystemColors.Control;
            this.logTab.Controls.Add(this.LogTextBox);
            this.logTab.Location = new System.Drawing.Point(8, 39);
            this.logTab.Margin = new System.Windows.Forms.Padding(6);
            this.logTab.Name = "logTab";
            this.logTab.Padding = new System.Windows.Forms.Padding(6);
            this.logTab.Size = new System.Drawing.Size(1016, 733);
            this.logTab.TabIndex = 1;
            this.logTab.Text = "Log";
            // 
            // LogTextBox
            // 
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.Location = new System.Drawing.Point(6, 6);
            this.LogTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.Size = new System.Drawing.Size(1004, 721);
            this.LogTextBox.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(6);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pluralText);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.fromText);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.sourceDirectory);
            this.splitContainer1.Panel1.Controls.Add(this.toText);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1MinSize = 120;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1046, 976);
            this.splitContainer1.SplitterDistance = 483;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 16;
            // 
            // pluralText
            // 
            this.pluralText.Location = new System.Drawing.Point(202, 182);
            this.pluralText.Margin = new System.Windows.Forms.Padding(4);
            this.pluralText.Name = "pluralText";
            this.pluralText.Size = new System.Drawing.Size(738, 31);
            this.pluralText.TabIndex = 15;
            this.pluralText.Text = "Oranges";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(31, 188);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 25);
            this.label6.TabIndex = 16;
            this.label6.Text = "Plural";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1046, 976);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Code cloner";
            this.tabControl1.ResumeLayout(false);
            this.SettingsTab.ResumeLayout(false);
            this.SettingsTab.PerformLayout();
            this.logTab.ResumeLayout(false);
            this.logTab.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox fromText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox toText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox filesToSearch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox filesToRename;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox sourceDirectory;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage SettingsTab;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabPage logTab;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.TextBox pluralText;
        private System.Windows.Forms.Label label6;
    }
}

