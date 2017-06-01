namespace PPeXUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.trvFiles = new System.Windows.Forms.TreeView();
            this.cxtItems = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trvImageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddArc = new System.Windows.Forms.ToolStripButton();
            this.btnAddFile = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnImportPP = new System.Windows.Forms.ToolStripButton();
            this.btnImportFolder = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.convertxggTowavToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lsvSource = new System.Windows.Forms.ListView();
            this.btnTestCompr = new System.Windows.Forms.Button();
            this.txtFileProg = new System.Windows.Forms.TextBox();
            this.prgFileProgress = new System.Windows.Forms.ProgressBar();
            this.btnExport = new System.Windows.Forms.Button();
            this.txtFileInternal = new System.Windows.Forms.TextBox();
            this.lblFileDispName = new System.Windows.Forms.Label();
            this.txtFileMD5 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtFileSize = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFileType = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtSaveProg = new System.Windows.Forms.TextBox();
            this.prgSaveProgress = new System.Windows.Forms.ProgressBar();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBrowseSave = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSaveLocation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtArchiveName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbArchiveType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbCompression = new System.Windows.Forms.ComboBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.numBitrate = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.cxtItems.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBitrate)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.trvFiles);
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(781, 307);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.TabIndex = 3;
            // 
            // trvFiles
            // 
            this.trvFiles.AllowDrop = true;
            this.trvFiles.ContextMenuStrip = this.cxtItems;
            this.trvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvFiles.ImageIndex = 0;
            this.trvFiles.ImageList = this.trvImageList;
            this.trvFiles.LabelEdit = true;
            this.trvFiles.Location = new System.Drawing.Point(0, 25);
            this.trvFiles.Name = "trvFiles";
            this.trvFiles.PathSeparator = "/";
            this.trvFiles.SelectedImageIndex = 0;
            this.trvFiles.Size = new System.Drawing.Size(215, 282);
            this.trvFiles.TabIndex = 0;
            this.trvFiles.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.trvFiles_AfterLabelEdit);
            this.trvFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvFiles_AfterSelect);
            this.trvFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.trvFiles_DragDrop);
            this.trvFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.trvFiles_DragEnter);
            // 
            // cxtItems
            // 
            this.cxtItems.Enabled = false;
            this.cxtItems.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.cxtItems.Name = "cxtItems";
            this.cxtItems.Size = new System.Drawing.Size(118, 70);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Enabled = false;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Enabled = false;
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Enabled = false;
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // trvImageList
            // 
            this.trvImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("trvImageList.ImageStream")));
            this.trvImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.trvImageList.Images.SetKeyName(0, "package.png");
            this.trvImageList.Images.SetKeyName(1, "document.png");
            this.trvImageList.Images.SetKeyName(2, "document-music.png");
            this.trvImageList.Images.SetKeyName(3, "document-image.png");
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.toolStripSeparator1,
            this.btnAddArc,
            this.btnAddFile,
            this.btnDelete,
            this.toolStripSeparator2,
            this.btnImportPP,
            this.btnImportFolder,
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(215, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = global::PPeXUI.Properties.Resources.page;
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Margin = new System.Windows.Forms.Padding(4, 1, 0, 2);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(23, 22);
            this.btnNew.Text = "New archive";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 22);
            this.btnOpen.Text = "Open archive";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAddArc
            // 
            this.btnAddArc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddArc.Image = global::PPeXUI.Properties.Resources.package_add;
            this.btnAddArc.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddArc.Name = "btnAddArc";
            this.btnAddArc.Size = new System.Drawing.Size(23, 22);
            this.btnAddArc.Text = "Add new subarchive";
            this.btnAddArc.Click += new System.EventHandler(this.btnAddArc_Click);
            // 
            // btnAddFile
            // 
            this.btnAddFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddFile.Image = global::PPeXUI.Properties.Resources.page_white_add;
            this.btnAddFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddFile.Name = "btnAddFile";
            this.btnAddFile.Size = new System.Drawing.Size(23, 22);
            this.btnAddFile.Text = "Add files";
            this.btnAddFile.Click += new System.EventHandler(this.btnAddFile_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelete.Image = global::PPeXUI.Properties.Resources.delete;
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(23, 22);
            this.btnDelete.Text = "Remove selected";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnImportPP
            // 
            this.btnImportPP.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnImportPP.Image = global::PPeXUI.Properties.Resources.database_add;
            this.btnImportPP.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImportPP.Name = "btnImportPP";
            this.btnImportPP.Size = new System.Drawing.Size(23, 22);
            this.btnImportPP.Text = "Import .pp file";
            this.btnImportPP.Click += new System.EventHandler(this.btnImportPP_Click);
            // 
            // btnImportFolder
            // 
            this.btnImportFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnImportFolder.Image = global::PPeXUI.Properties.Resources.folder_add;
            this.btnImportFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImportFolder.Name = "btnImportFolder";
            this.btnImportFolder.Size = new System.Drawing.Size(23, 22);
            this.btnImportFolder.Text = "Import Folder";
            this.btnImportFolder.Click += new System.EventHandler(this.btnImportFolder_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator3,
            this.convertxggTowavToolStripMenuItem});
            this.toolStripDropDownButton1.Image = global::PPeXUI.Properties.Resources.bricks;
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton1.Text = "Plugins";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // convertxggTowavToolStripMenuItem
            // 
            this.convertxggTowavToolStripMenuItem.Name = "convertxggTowavToolStripMenuItem";
            this.convertxggTowavToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.convertxggTowavToolStripMenuItem.Text = "Convert .xgg to .wav";
            this.convertxggTowavToolStripMenuItem.Click += new System.EventHandler(this.convertxggTowavToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(562, 307);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lsvSource);
            this.tabPage1.Controls.Add(this.btnTestCompr);
            this.tabPage1.Controls.Add(this.txtFileProg);
            this.tabPage1.Controls.Add(this.prgFileProgress);
            this.tabPage1.Controls.Add(this.btnExport);
            this.tabPage1.Controls.Add(this.txtFileInternal);
            this.tabPage1.Controls.Add(this.lblFileDispName);
            this.tabPage1.Controls.Add(this.txtFileMD5);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.txtFileSize);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.txtFileType);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.txtFileName);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(554, 281);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lsvSource
            // 
            this.lsvSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvSource.GridLines = true;
            this.lsvSource.Location = new System.Drawing.Point(318, 6);
            this.lsvSource.Name = "lsvSource";
            this.lsvSource.Size = new System.Drawing.Size(228, 153);
            this.lsvSource.TabIndex = 14;
            this.lsvSource.UseCompatibleStateImageBehavior = false;
            this.lsvSource.View = System.Windows.Forms.View.Details;
            // 
            // btnTestCompr
            // 
            this.btnTestCompr.Location = new System.Drawing.Point(163, 136);
            this.btnTestCompr.Name = "btnTestCompr";
            this.btnTestCompr.Size = new System.Drawing.Size(149, 23);
            this.btnTestCompr.TabIndex = 13;
            this.btnTestCompr.Text = "Test Compression";
            this.btnTestCompr.UseVisualStyleBackColor = true;
            // 
            // txtFileProg
            // 
            this.txtFileProg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileProg.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFileProg.Location = new System.Drawing.Point(9, 194);
            this.txtFileProg.Multiline = true;
            this.txtFileProg.Name = "txtFileProg";
            this.txtFileProg.ReadOnly = true;
            this.txtFileProg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFileProg.Size = new System.Drawing.Size(537, 79);
            this.txtFileProg.TabIndex = 12;
            // 
            // prgFileProgress
            // 
            this.prgFileProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgFileProgress.Location = new System.Drawing.Point(9, 165);
            this.prgFileProgress.Name = "prgFileProgress";
            this.prgFileProgress.Size = new System.Drawing.Size(537, 23);
            this.prgFileProgress.TabIndex = 11;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(9, 136);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(149, 23);
            this.btnExport.TabIndex = 10;
            this.btnExport.Text = "Export item";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // txtFileInternal
            // 
            this.txtFileInternal.Location = new System.Drawing.Point(86, 32);
            this.txtFileInternal.Name = "txtFileInternal";
            this.txtFileInternal.ReadOnly = true;
            this.txtFileInternal.Size = new System.Drawing.Size(226, 20);
            this.txtFileInternal.TabIndex = 9;
            // 
            // lblFileDispName
            // 
            this.lblFileDispName.AutoSize = true;
            this.lblFileDispName.Location = new System.Drawing.Point(6, 35);
            this.lblFileDispName.Name = "lblFileDispName";
            this.lblFileDispName.Size = new System.Drawing.Size(74, 13);
            this.lblFileDispName.TabIndex = 8;
            this.lblFileDispName.Text = "Internal name:";
            // 
            // txtFileMD5
            // 
            this.txtFileMD5.Location = new System.Drawing.Point(50, 110);
            this.txtFileMD5.Name = "txtFileMD5";
            this.txtFileMD5.ReadOnly = true;
            this.txtFileMD5.Size = new System.Drawing.Size(262, 20);
            this.txtFileMD5.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 113);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(33, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "MD5:";
            // 
            // txtFileSize
            // 
            this.txtFileSize.Location = new System.Drawing.Point(116, 84);
            this.txtFileSize.Name = "txtFileSize";
            this.txtFileSize.ReadOnly = true;
            this.txtFileSize.Size = new System.Drawing.Size(196, 20);
            this.txtFileSize.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Uncompressed Size:";
            // 
            // txtFileType
            // 
            this.txtFileType.Location = new System.Drawing.Point(50, 58);
            this.txtFileType.Name = "txtFileType";
            this.txtFileType.ReadOnly = true;
            this.txtFileType.Size = new System.Drawing.Size(262, 20);
            this.txtFileType.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Type:";
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(50, 6);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(262, 20);
            this.txtFileName.TabIndex = 1;
            this.txtFileName.TextChanged += new System.EventHandler(this.txtFileName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Name:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtSaveProg);
            this.tabPage2.Controls.Add(this.prgSaveProgress);
            this.tabPage2.Controls.Add(this.btnSave);
            this.tabPage2.Controls.Add(this.btnBrowseSave);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.txtSaveLocation);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.txtArchiveName);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.cmbArchiveType);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.cmbCompression);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(554, 281);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Save";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtSaveProg
            // 
            this.txtSaveProg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaveProg.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSaveProg.Location = new System.Drawing.Point(9, 194);
            this.txtSaveProg.Multiline = true;
            this.txtSaveProg.Name = "txtSaveProg";
            this.txtSaveProg.ReadOnly = true;
            this.txtSaveProg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSaveProg.Size = new System.Drawing.Size(537, 79);
            this.txtSaveProg.TabIndex = 13;
            // 
            // prgSaveProgress
            // 
            this.prgSaveProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgSaveProgress.Location = new System.Drawing.Point(9, 165);
            this.prgSaveProgress.Name = "prgSaveProgress";
            this.prgSaveProgress.Size = new System.Drawing.Size(537, 23);
            this.prgSaveProgress.TabIndex = 10;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(9, 136);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(537, 23);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBrowseSave
            // 
            this.btnBrowseSave.Location = new System.Drawing.Point(271, 86);
            this.btnBrowseSave.Name = "btnBrowseSave";
            this.btnBrowseSave.Size = new System.Drawing.Size(25, 20);
            this.btnBrowseSave.TabIndex = 8;
            this.btnBrowseSave.Text = "...";
            this.btnBrowseSave.UseVisualStyleBackColor = true;
            this.btnBrowseSave.Click += new System.EventHandler(this.btnBrowseSave_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Save location:";
            // 
            // txtSaveLocation
            // 
            this.txtSaveLocation.Location = new System.Drawing.Point(89, 86);
            this.txtSaveLocation.MaxLength = 32766;
            this.txtSaveLocation.Name = "txtSaveLocation";
            this.txtSaveLocation.Size = new System.Drawing.Size(176, 20);
            this.txtSaveLocation.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Archive Title:";
            // 
            // txtArchiveName
            // 
            this.txtArchiveName.Location = new System.Drawing.Point(89, 60);
            this.txtArchiveName.MaxLength = 32766;
            this.txtArchiveName.Name = "txtArchiveName";
            this.txtArchiveName.Size = new System.Drawing.Size(176, 20);
            this.txtArchiveName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Archive Type:";
            // 
            // cmbArchiveType
            // 
            this.cmbArchiveType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbArchiveType.FormattingEnabled = true;
            this.cmbArchiveType.Items.AddRange(new object[] {
            "Base Archive",
            "Mod",
            "BGM Pack"});
            this.cmbArchiveType.Location = new System.Drawing.Point(89, 33);
            this.cmbArchiveType.Name = "cmbArchiveType";
            this.cmbArchiveType.Size = new System.Drawing.Size(121, 21);
            this.cmbArchiveType.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Compression:";
            // 
            // cmbCompression
            // 
            this.cmbCompression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCompression.FormattingEnabled = true;
            this.cmbCompression.Items.AddRange(new object[] {
            "Uncompressed",
            "LZ4",
            "Zstandard"});
            this.cmbCompression.Location = new System.Drawing.Point(89, 6);
            this.cmbCompression.Name = "cmbCompression";
            this.cmbCompression.Size = new System.Drawing.Size(121, 21);
            this.cmbCompression.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.numBitrate);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(554, 281);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Options";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // numBitrate
            // 
            this.numBitrate.Increment = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numBitrate.Location = new System.Drawing.Point(135, 7);
            this.numBitrate.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numBitrate.Name = "numBitrate";
            this.numBitrate.ReadOnly = true;
            this.numBitrate.Size = new System.Drawing.Size(82, 20);
            this.numBitrate.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(123, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Audio bitrate (0 for auto):";
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(781, 307);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(797, 346);
            this.Name = "formMain";
            this.Text = "PPeXUI";
            this.Load += new System.EventHandler(this.formMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.cxtItems.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBitrate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView trvFiles;
        private System.Windows.Forms.ImageList trvImageList;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnAddArc;
        private System.Windows.Forms.ToolStripButton btnImportPP;
        private System.Windows.Forms.ToolStripButton btnAddFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnImportFolder;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ProgressBar prgSaveProgress;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnBrowseSave;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSaveLocation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtArchiveName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbArchiveType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbCompression;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFileMD5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtFileSize;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFileType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtFileInternal;
        private System.Windows.Forms.Label lblFileDispName;
        private System.Windows.Forms.NumericUpDown numBitrate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ContextMenuStrip cxtItems;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.ProgressBar prgFileProgress;
        private System.Windows.Forms.TextBox txtFileProg;
        private System.Windows.Forms.Button btnTestCompr;
        private System.Windows.Forms.TextBox txtSaveProg;
        private System.Windows.Forms.ListView lsvSource;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem convertxggTowavToolStripMenuItem;
    }
}