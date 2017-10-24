namespace xxGUI
{
    partial class formMain
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
            this.trvObjects = new System.Windows.Forms.TreeView();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.OpenXX2 = new System.Windows.Forms.Button();
            this.btnSaveXX = new System.Windows.Forms.Button();
            this.btnScanFolder = new System.Windows.Forms.Button();
            this.lsvTextures = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imgTexture = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgTexture)).BeginInit();
            this.SuspendLayout();
            // 
            // trvObjects
            // 
            this.trvObjects.Location = new System.Drawing.Point(12, 12);
            this.trvObjects.Name = "trvObjects";
            this.trvObjects.Size = new System.Drawing.Size(260, 207);
            this.trvObjects.TabIndex = 0;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 226);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(93, 226);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(174, 226);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(75, 23);
            this.btnScan.TabIndex = 3;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // OpenXX2
            // 
            this.OpenXX2.Location = new System.Drawing.Point(12, 256);
            this.OpenXX2.Name = "OpenXX2";
            this.OpenXX2.Size = new System.Drawing.Size(75, 23);
            this.OpenXX2.TabIndex = 4;
            this.OpenXX2.Text = "Open .xx2";
            this.OpenXX2.UseVisualStyleBackColor = true;
            this.OpenXX2.Click += new System.EventHandler(this.OpenXX2_Click);
            // 
            // btnSaveXX
            // 
            this.btnSaveXX.Location = new System.Drawing.Point(93, 255);
            this.btnSaveXX.Name = "btnSaveXX";
            this.btnSaveXX.Size = new System.Drawing.Size(75, 23);
            this.btnSaveXX.TabIndex = 5;
            this.btnSaveXX.Text = "Save .xx";
            this.btnSaveXX.UseVisualStyleBackColor = true;
            this.btnSaveXX.Click += new System.EventHandler(this.btnSaveXX_Click);
            // 
            // btnScanFolder
            // 
            this.btnScanFolder.Location = new System.Drawing.Point(174, 255);
            this.btnScanFolder.Name = "btnScanFolder";
            this.btnScanFolder.Size = new System.Drawing.Size(75, 23);
            this.btnScanFolder.TabIndex = 6;
            this.btnScanFolder.Text = "Scan Folder";
            this.btnScanFolder.UseVisualStyleBackColor = true;
            this.btnScanFolder.Click += new System.EventHandler(this.btnScanFolder_Click);
            // 
            // lsvTextures
            // 
            this.lsvTextures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lsvTextures.GridLines = true;
            this.lsvTextures.Location = new System.Drawing.Point(278, 12);
            this.lsvTextures.Name = "lsvTextures";
            this.lsvTextures.Size = new System.Drawing.Size(317, 265);
            this.lsvTextures.TabIndex = 7;
            this.lsvTextures.UseCompatibleStateImageBehavior = false;
            this.lsvTextures.View = System.Windows.Forms.View.Details;
            this.lsvTextures.SelectedIndexChanged += new System.EventHandler(this.lsvTextures_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 102;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Dimensions";
            this.columnHeader2.Width = 74;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Checksum";
            this.columnHeader3.Width = 67;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Size";
            // 
            // imgTexture
            // 
            this.imgTexture.Location = new System.Drawing.Point(601, 12);
            this.imgTexture.Name = "imgTexture";
            this.imgTexture.Size = new System.Drawing.Size(210, 265);
            this.imgTexture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgTexture.TabIndex = 8;
            this.imgTexture.TabStop = false;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 289);
            this.Controls.Add(this.imgTexture);
            this.Controls.Add(this.lsvTextures);
            this.Controls.Add(this.btnScanFolder);
            this.Controls.Add(this.btnSaveXX);
            this.Controls.Add(this.OpenXX2);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.trvObjects);
            this.Name = "formMain";
            this.Text = "Big Boy .xx Decoder";
            ((System.ComponentModel.ISupportInitialize)(this.imgTexture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView trvObjects;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button OpenXX2;
        private System.Windows.Forms.Button btnSaveXX;
        private System.Windows.Forms.Button btnScanFolder;
        private System.Windows.Forms.ListView lsvTextures;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.PictureBox imgTexture;
    }
}

