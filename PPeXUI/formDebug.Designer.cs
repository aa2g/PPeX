namespace PPeXUI
{
    partial class formDebug
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
            this.btnTest = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnDecompress = new System.Windows.Forms.Button();
            this.lsvFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnInit = new System.Windows.Forms.Button();
            this.btnRetype = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lsvMD = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lsvSizes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(12, 67);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(29, 24);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(254, 20);
            this.textBox1.TabIndex = 1;
            // 
            // btnDecompress
            // 
            this.btnDecompress.Location = new System.Drawing.Point(94, 67);
            this.btnDecompress.Name = "btnDecompress";
            this.btnDecompress.Size = new System.Drawing.Size(75, 23);
            this.btnDecompress.TabIndex = 2;
            this.btnDecompress.Text = "Decompress";
            this.btnDecompress.UseVisualStyleBackColor = true;
            this.btnDecompress.Click += new System.EventHandler(this.btnDecompress_Click);
            // 
            // lsvFiles
            // 
            this.lsvFiles.AllowDrop = true;
            this.lsvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lsvFiles.GridLines = true;
            this.lsvFiles.Location = new System.Drawing.Point(12, 162);
            this.lsvFiles.Name = "lsvFiles";
            this.lsvFiles.Size = new System.Drawing.Size(321, 433);
            this.lsvFiles.TabIndex = 3;
            this.lsvFiles.UseCompatibleStateImageBehavior = false;
            this.lsvFiles.View = System.Windows.Forms.View.Details;
            this.lsvFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.lsvFiles_DragDrop);
            this.lsvFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.lsvFiles_DragEnter);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 294;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(29, 96);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(254, 20);
            this.textBox2.TabIndex = 4;
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(38, 122);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 5;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnInit
            // 
            this.btnInit.Location = new System.Drawing.Point(175, 67);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(75, 23);
            this.btnInit.TabIndex = 6;
            this.btnInit.Text = "Init";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // btnRetype
            // 
            this.btnRetype.Location = new System.Drawing.Point(119, 122);
            this.btnRetype.Name = "btnRetype";
            this.btnRetype.Size = new System.Drawing.Size(75, 23);
            this.btnRetype.TabIndex = 7;
            this.btnRetype.Text = "Retype";
            this.btnRetype.UseVisualStyleBackColor = true;
            this.btnRetype.Click += new System.EventHandler(this.btnRetype_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(200, 122);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lsvMD
            // 
            this.lsvMD.AllowDrop = true;
            this.lsvMD.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader6});
            this.lsvMD.GridLines = true;
            this.lsvMD.Location = new System.Drawing.Point(339, 12);
            this.lsvMD.Name = "lsvMD";
            this.lsvMD.Size = new System.Drawing.Size(373, 567);
            this.lsvMD.TabIndex = 9;
            this.lsvMD.UseCompatibleStateImageBehavior = false;
            this.lsvMD.View = System.Windows.Forms.View.Details;
            this.lsvMD.DragDrop += new System.Windows.Forms.DragEventHandler(this.lsvMD_DragDrop);
            this.lsvMD.DragEnter += new System.Windows.Forms.DragEventHandler(this.lsvMD_DragEnter);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "MD5";
            this.columnHeader2.Width = 167;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Count";
            this.columnHeader3.Width = 90;
            // 
            // lsvSizes
            // 
            this.lsvSizes.AllowDrop = true;
            this.lsvSizes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            this.lsvSizes.GridLines = true;
            this.lsvSizes.Location = new System.Drawing.Point(718, 12);
            this.lsvSizes.Name = "lsvSizes";
            this.lsvSizes.Size = new System.Drawing.Size(230, 583);
            this.lsvSizes.TabIndex = 10;
            this.lsvSizes.UseCompatibleStateImageBehavior = false;
            this.lsvSizes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "MD5";
            this.columnHeader4.Width = 95;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Size";
            this.columnHeader5.Width = 90;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Total";
            this.columnHeader6.Width = 94;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(508, 582);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "label1";
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 607);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lsvSizes);
            this.Controls.Add(this.lsvMD);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnRetype);
            this.Controls.Add(this.btnInit);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.lsvFiles);
            this.Controls.Add(this.btnDecompress);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnTest);
            this.Name = "formMain";
            this.Text = "PPeX";
            this.Load += new System.EventHandler(this.formMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnDecompress;
        private System.Windows.Forms.ListView lsvFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.Button btnRetype;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ListView lsvMD;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView lsvSizes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Label label1;
    }
}

