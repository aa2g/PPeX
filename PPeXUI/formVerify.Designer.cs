namespace PPeXUI
{
    partial class formVerify
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
            this.prgChunkProgress = new System.Windows.Forms.ProgressBar();
            this.lsvChunkHash = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lsvFileHash = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.prgFileProgress = new System.Windows.Forms.ProgressBar();
            this.btnOk = new System.Windows.Forms.Button();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // prgChunkProgress
            // 
            this.prgChunkProgress.Location = new System.Drawing.Point(12, 12);
            this.prgChunkProgress.Name = "prgChunkProgress";
            this.prgChunkProgress.Size = new System.Drawing.Size(458, 23);
            this.prgChunkProgress.TabIndex = 0;
            // 
            // lsvChunkHash
            // 
            this.lsvChunkHash.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lsvChunkHash.GridLines = true;
            this.lsvChunkHash.Location = new System.Drawing.Point(12, 41);
            this.lsvChunkHash.Name = "lsvChunkHash";
            this.lsvChunkHash.Size = new System.Drawing.Size(458, 89);
            this.lsvChunkHash.TabIndex = 1;
            this.lsvChunkHash.UseCompatibleStateImageBehavior = false;
            this.lsvChunkHash.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Expected CRC";
            this.columnHeader2.Width = 160;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Actual CRC";
            this.columnHeader3.Width = 160;
            // 
            // lsvFileHash
            // 
            this.lsvFileHash.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lsvFileHash.GridLines = true;
            this.lsvFileHash.Location = new System.Drawing.Point(12, 163);
            this.lsvFileHash.Name = "lsvFileHash";
            this.lsvFileHash.Size = new System.Drawing.Size(458, 89);
            this.lsvFileHash.TabIndex = 3;
            this.lsvFileHash.UseCompatibleStateImageBehavior = false;
            this.lsvFileHash.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Name";
            this.columnHeader4.Width = 80;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Expected MD5";
            this.columnHeader5.Width = 140;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Actual MD5";
            this.columnHeader6.Width = 140;
            // 
            // prgFileProgress
            // 
            this.prgFileProgress.Location = new System.Drawing.Point(12, 134);
            this.prgFileProgress.Name = "prgFileProgress";
            this.prgFileProgress.Size = new System.Drawing.Size(458, 23);
            this.prgFileProgress.TabIndex = 2;
            // 
            // btnOk
            // 
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(12, 258);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(458, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Archive Name";
            this.columnHeader7.Width = 80;
            // 
            // formVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 288);
            this.ControlBox = false;
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lsvFileHash);
            this.Controls.Add(this.prgFileProgress);
            this.Controls.Add(this.lsvChunkHash);
            this.Controls.Add(this.prgChunkProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "formVerify";
            this.Text = "formVerify";
            this.Shown += new System.EventHandler(this.formVerify_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar prgChunkProgress;
        private System.Windows.Forms.ListView lsvChunkHash;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView lsvFileHash;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ProgressBar prgFileProgress;
        private System.Windows.Forms.Button btnOk;
    }
}