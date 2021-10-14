namespace ProjectSetup
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
            this.renameDirectories = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.sourceDirectory = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.logTab = new System.Windows.Forms.TabPage();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
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
            this.label1.Location = new System.Drawing.Point(15, 67);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "From";
            // 
            // fromText
            // 
            this.fromText.Location = new System.Drawing.Point(101, 64);
            this.fromText.Margin = new System.Windows.Forms.Padding(2);
            this.fromText.Name = "fromText";
            this.fromText.Size = new System.Drawing.Size(371, 20);
            this.fromText.TabIndex = 2;
            this.fromText.Text = "ChilliCoreTemplate";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 89);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "To";
            // 
            // toText
            // 
            this.toText.Location = new System.Drawing.Point(101, 86);
            this.toText.Margin = new System.Windows.Forms.Padding(2);
            this.toText.Name = "toText";
            this.toText.Size = new System.Drawing.Size(371, 20);
            this.toText.TabIndex = 4;
            this.toText.Text = "MySolution";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 14);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Files to search";
            // 
            // filesToSearch
            // 
            this.filesToSearch.Location = new System.Drawing.Point(97, 14);
            this.filesToSearch.Margin = new System.Windows.Forms.Padding(2);
            this.filesToSearch.Multiline = true;
            this.filesToSearch.Name = "filesToSearch";
            this.filesToSearch.Size = new System.Drawing.Size(163, 116);
            this.filesToSearch.TabIndex = 6;
            this.filesToSearch.Text = "*.sln\r\n*.csproj\r\n*.cs\r\n*.cshtml\r\n*.json\r\n*.pubxml\r\n*.asax\r\n*.config";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 146);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Files to rename";
            // 
            // filesToRename
            // 
            this.filesToRename.Location = new System.Drawing.Point(97, 146);
            this.filesToRename.Margin = new System.Windows.Forms.Padding(2);
            this.filesToRename.Multiline = true;
            this.filesToRename.Name = "filesToRename";
            this.filesToRename.Size = new System.Drawing.Size(163, 83);
            this.filesToRename.TabIndex = 8;
            this.filesToRename.Text = "*.sln\r\n*.csproj";
            // 
            // renameDirectories
            // 
            this.renameDirectories.AutoSize = true;
            this.renameDirectories.Checked = true;
            this.renameDirectories.CheckState = System.Windows.Forms.CheckState.Checked;
            this.renameDirectories.Location = new System.Drawing.Point(97, 247);
            this.renameDirectories.Margin = new System.Windows.Forms.Padding(2);
            this.renameDirectories.Name = "renameDirectories";
            this.renameDirectories.Size = new System.Drawing.Size(117, 17);
            this.renameDirectories.TabIndex = 11;
            this.renameDirectories.Text = "Rename directories";
            this.renameDirectories.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(309, 121);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(161, 33);
            this.button1.TabIndex = 12;
            this.button1.Text = "Setup";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 25);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Source directory";
            // 
            // sourceDirectory
            // 
            this.sourceDirectory.Location = new System.Drawing.Point(101, 22);
            this.sourceDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.sourceDirectory.Name = "sourceDirectory";
            this.sourceDirectory.Size = new System.Drawing.Size(371, 20);
            this.sourceDirectory.TabIndex = 13;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.SettingsTab);
            this.tabControl1.Controls.Add(this.logTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(516, 365);
            this.tabControl1.TabIndex = 15;
            // 
            // SettingsTab
            // 
            this.SettingsTab.BackColor = System.Drawing.SystemColors.Control;
            this.SettingsTab.Controls.Add(this.label3);
            this.SettingsTab.Controls.Add(this.filesToSearch);
            this.SettingsTab.Controls.Add(this.filesToRename);
            this.SettingsTab.Controls.Add(this.label4);
            this.SettingsTab.Controls.Add(this.renameDirectories);
            this.SettingsTab.Location = new System.Drawing.Point(4, 22);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsTab.Size = new System.Drawing.Size(508, 339);
            this.SettingsTab.TabIndex = 0;
            this.SettingsTab.Text = "Settings";
            // 
            // logTab
            // 
            this.logTab.BackColor = System.Drawing.SystemColors.Control;
            this.logTab.Controls.Add(this.LogTextBox);
            this.logTab.Location = new System.Drawing.Point(4, 22);
            this.logTab.Name = "logTab";
            this.logTab.Padding = new System.Windows.Forms.Padding(3);
            this.logTab.Size = new System.Drawing.Size(508, 339);
            this.logTab.TabIndex = 1;
            this.logTab.Text = "Log";
            // 
            // LogTextBox
            // 
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.Location = new System.Drawing.Point(3, 3);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.Size = new System.Drawing.Size(502, 333);
            this.LogTextBox.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
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
            this.splitContainer1.Size = new System.Drawing.Size(516, 546);
            this.splitContainer1.SplitterDistance = 177;
            this.splitContainer1.TabIndex = 16;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 546);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Project setup";
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
        private System.Windows.Forms.CheckBox renameDirectories;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox sourceDirectory;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage SettingsTab;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabPage logTab;
        private System.Windows.Forms.TextBox LogTextBox;
    }
}

