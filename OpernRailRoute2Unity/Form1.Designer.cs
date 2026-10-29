namespace OpernRailRoute2Unity
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtLog = new TextBox();
            txtTdb = new TextBox();
            btnTdb = new Button();
            txtTsection = new TextBox();
            btnTsection = new Button();
            btnExport = new Button();
            SuspendLayout();
            // 
            // txtLog
            // 
            txtLog.Location = new Point(25, 179);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Both;
            txtLog.Size = new Size(735, 259);
            txtLog.TabIndex = 0;
            // 
            // txtTdb
            // 
            txtTdb.Location = new Point(25, 23);
            txtTdb.Name = "txtTdb";
            txtTdb.Size = new Size(651, 23);
            txtTdb.TabIndex = 1;
            // 
            // btnTdb
            // 
            btnTdb.Location = new Point(697, 23);
            btnTdb.Name = "btnTdb";
            btnTdb.Size = new Size(75, 23);
            btnTdb.TabIndex = 2;
            btnTdb.Text = "button1";
            btnTdb.UseVisualStyleBackColor = true;
            btnTdb.Click += btnTdb_Click;
            // 
            // txtTsection
            // 
            txtTsection.Location = new Point(25, 52);
            txtTsection.Name = "txtTsection";
            txtTsection.Size = new Size(651, 23);
            txtTsection.TabIndex = 3;
            // 
            // btnTsection
            // 
            btnTsection.Location = new Point(697, 52);
            btnTsection.Name = "btnTsection";
            btnTsection.Size = new Size(75, 23);
            btnTsection.TabIndex = 4;
            btnTsection.Text = "button1";
            btnTsection.UseVisualStyleBackColor = true;
            btnTsection.Click += btnTsection_Click;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(25, 114);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(722, 45);
            btnExport.TabIndex = 5;
            btnExport.Text = "button1";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnExport);
            Controls.Add(btnTsection);
            Controls.Add(txtTsection);
            Controls.Add(btnTdb);
            Controls.Add(txtTdb);
            Controls.Add(txtLog);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLog;
        private TextBox txtTdb;
        private Button btnTdb;
        private TextBox txtTsection;
        private Button btnTsection;
        private Button btnExport;
    }
}
