namespace PPeXUI
{
    partial class formImporting
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
            this.prgMinor = new System.Windows.Forms.ProgressBar();
            this.prgMajor = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // prgMinor
            // 
            this.prgMinor.Location = new System.Drawing.Point(3, 20);
            this.prgMinor.Name = "prgMinor";
            this.prgMinor.Size = new System.Drawing.Size(365, 16);
            this.prgMinor.TabIndex = 3;
            // 
            // prgMajor
            // 
            this.prgMajor.Location = new System.Drawing.Point(3, 3);
            this.prgMajor.Name = "prgMajor";
            this.prgMajor.Size = new System.Drawing.Size(365, 16);
            this.prgMajor.TabIndex = 2;
            // 
            // formImporting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 39);
            this.ControlBox = false;
            this.Controls.Add(this.prgMinor);
            this.Controls.Add(this.prgMajor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formImporting";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Importing...";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar prgMinor;
        private System.Windows.Forms.ProgressBar prgMajor;
    }
}