using Microsoft.WindowsAPICodePack.Dialogs;
using PPeX;
using PPeX.External.PP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PPeX.External.Zstandard;

namespace PPeXUI
{
    public partial class formMain : Form
    {
        public string currentlyOpenedFile = "";

        bool _modified = false;
        public bool IsModified
        {
            get
            {
                return _modified;
            }
            set
            {
                this.Text = "PPeXUI - " + currentlyOpenedFile + (value ? "*" : "");

                _modified = value;
            }
        }

        public formMain()
        {
            InitializeComponent();
        }

        static Version GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        protected void LoadAboutInfo()
        {
            lblBaseVersion.Text = $"Base Version: {PPeX.Core.GetVersion().ToString()}";
            lblGUIVersion.Text = $"GUI Version: {GetVersion().ToString()}";

            string platformMode;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                platformMode = "Unix/Mono";
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.Is64BitProcess)
                platformMode = "Win64";
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                platformMode = "Win32";
            else
                platformMode = "Unsupported";

            lblPlatformMode.Text = $"Current platform mode: {platformMode}";
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            cmbCompression.SelectedIndex = 2;

#if !DEBUG
            verifyArchiveToolStripMenuItem.Enabled = false;
            verifyArchiveToolStripMenuItem.Visible = false;
#endif
            numVoiceBitrate.Value = Core.Settings.OpusVoiceBitrate / 1000;
            numMusicBitrate.Value = Core.Settings.OpusMusicBitrate / 1000;
            numXx2Precision.Value = Core.Settings.Xx2Precision;

            //aiming for 1/2 processor count, we get diminishing returns on higher thread counts
            decimal threads = Math.Floor(Environment.ProcessorCount / (decimal)2);

            //cap it between 1 and 8
            threads = Math.Max(1, threads);
            threads = Math.Min(threads, 8);

            numThreads.Value = threads;

            LoadAboutInfo();
        }

        public void CloseFile()
        {
            currentlyOpenedFile = "";
            cxtItems.Enabled = false;
            IsModified = false;


            trvFiles.BeginUpdate();

            trvFiles.Nodes.Clear();

            trvFiles.EndUpdate();


            lsvChunks.Items.Clear();
        }

        public void OpenFile()
        {
            txtSaveLocation.Text = "";
            txtArchiveName.Text = "";
            cxtItems.Enabled = true;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            CloseFile();
            OpenFile();
        }

        private void btnAddArc_Click(object sender, EventArgs e)
        {
            var node = trvFiles.Nodes.Add("");
            node.ImageIndex = 0;
            IsModified = true;
            node.BeginEdit();
        }

        public static void SetAutoIcon(TreeNode node)
        {
            SubfileHolder current = null;

            if (node.Tag != null)
                current = node.Tag as SubfileHolder;

            if (node.Level == 0)
                node.ImageIndex = node.SelectedImageIndex = 0;
            else if (current.Type == "Audio")
                node.ImageIndex = node.SelectedImageIndex = 2;
            else if (current.Type == "Image")
                node.ImageIndex = node.SelectedImageIndex = 3;
            else
                node.ImageIndex = node.SelectedImageIndex = 1;
        }

        private bool isreloading = false;

        public void ReloadInfo()
        {
            isreloading = true;

            if (trvFiles.SelectedNode == null)
            {
                txtFileName.Text = "";
                txtFileInternal.Text = "";
                txtFileSize.Text = "";
                txtFileType.Text = "";
                txtFileMD5.Text = "";
            }
            else if (trvFiles.SelectedNode.Tag != null)
            {
                var current = trvFiles.SelectedNode.Tag as SubfileHolder;

                txtFileName.Text = current.Name;
                txtFileInternal.Text = current.InternalName;
                txtFileSize.Text = current.Size.ToString();
                txtFileType.Text = current.Type;
                txtFileMD5.Text = current.MD5;
            }
            else
            {
                string itemname = trvFiles.SelectedNode.Text;

                txtFileName.Text = itemname;
                txtFileInternal.Text = (itemname + ".pp").Replace(".pp.pp", ".pp");
                txtFileSize.Text = "";
                txtFileType.Text = "";
                txtFileMD5.Text = "";
            }

            isreloading = false;
        }

        private void btnAddFile_Click(object sender, EventArgs e)
        {

            if (trvFiles.SelectedNode != null)
            {
                var parent = trvFiles.SelectedNode.Level == 0 ?
                    trvFiles.SelectedNode :
                    trvFiles.SelectedNode.Parent;

                IEnumerable<string> paths = ShowOpenFileDialog("All Files|*.*", true);

                if (!paths.Any())
                    return;

                foreach (string file in paths)
                {
                    var node = parent.Nodes.Add(Path.GetFileName(file));

                    node.Tag = new SubfileHolder(new FileSource(file), Path.GetFileName(file));
                    SetAutoIcon(node);

                    if (!node.Parent.IsExpanded)
                        node.Parent.Expand();
                }


                //trvFiles.SelectedNode = node;

                IsModified = true;
                //node.edi
            }
        }

        private void trvFiles_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (!e.CancelEdit && e.Label != null)
            {
                e.Node.Text = e.Label;

                if (e.Node.Tag != null)
                {
                    var sub = e.Node.Tag as SubfileHolder;
                    sub.Name = e.Label;

                    SetAutoIcon(e.Node);
                }
                IsModified = true;

                ReloadInfo();
            }

        }

        private void TryDeleteSelected()
        {
            if (trvFiles.SelectedNode != null)
            {
                var node = trvFiles.SelectedNode;

                if (node.Level > 0)
                {
                    if (MessageBox.Show("Are you sure you want to remove \"" + node.Text + "\"?", "Remove file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        node.Remove();
                    }
                }
                else
                {
                    if (MessageBox.Show("Are you sure you want to remove \"" + node.Text + "\"?\nThis will remove " + node.GetNodeCount(false) + " additional files.",
                    "Remove emulated pp", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        node.Remove();
                    }
                }

                trvFiles.SelectedNode = null;
                ReloadInfo();

                IsModified = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            TryDeleteSelected();
        }

        private void trvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ReloadInfo();
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            if (!isreloading && trvFiles.SelectedNode != null)
            {
                var node = trvFiles.SelectedNode;

                if (node.Tag != null)
                {
                    var current = node.Tag as SubfileHolder;

                    current.Name = txtFileName.Text;
                }

                node.Text = txtFileName.Text;
                IsModified = true;

                ReloadInfo();
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trvFiles.SelectedNode != null)
            {
                trvFiles.SelectedNode.BeginEdit();
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TryDeleteSelected();
        }

        public void ExportItem()
        {
            if (trvFiles.SelectedNode != null)
            {
                IProgress<Tuple<string, int>> progress = new Progress<Tuple<string, int>>((x) =>
                {
                    prgFileProgress.Value = x.Item2;
                    txtFileProg.AppendText(x.Item1);
                });

                if (trvFiles.SelectedNode.Tag == null)
                {
                    //Is a PP file node
                    List<SubfileHolder> items = trvFiles.SelectedNode.Nodes
                        .Cast<TreeNode>()
                        .Select(x => x.Tag as SubfileHolder)
                        .ToList();

                    string path = ShowFolderDialog().FirstOrDefault();

                    if (string.IsNullOrEmpty(path))
                        return;

                    prgFileProgress.Value = 0;
                    txtFileProg.AppendText("Beginning export to \"" + path + "\"...\n");

                    int i = 0;

                    Task.Run(() =>
                    {
                        foreach (var item in items)
                        {
                            string filename = Path.Combine(path, item.Name);
                            i++;

                            if (File.Exists(filename))
                            {
                                progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + items.Count + "] Skipping " + item.Name + " as it already exists\n",
                                    i));

                                continue;
                            }

                            using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
                            {
                                (item.Source as ArchiveFileSource).GetStream().CopyTo(fs);
                            }

                            progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + items.Count + "] Exported " + item.Name + " (" + item.Size + " bytes)\n",
                                    100 * i / items.Count));
                        }

                        progress.Report(new Tuple<string, int>(
                                    "Done!\n",
                                    100));
                    });
                }
                else
                {
                    //Is a PP subfile node
                    SubfileHolder sh = trvFiles.SelectedNode.Tag as SubfileHolder;

                    string path = ShowSaveFileDialog("All Files|*.*", sh.Name);

                    if (string.IsNullOrEmpty(path))
                        return;

                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        (sh.Source as ArchiveFileSource).GetStream().CopyTo(fs);
                        //sh.Source.GetStream().CopyTo(fs);
                    }

                    prgFileProgress.Value = 100;
                    txtFileProg.AppendText("Exported " + Path.GetFileName(path) + " (" + sh.Source.Size + " bytes)\n");
                }



            }
        }

        public void TestCompression()
        {
            if (trvFiles.SelectedNode != null)
            {
                IProgress<Tuple<string, int>> progress = new Progress<Tuple<string, int>>((x) =>
                {
                    prgFileProgress.Value = x.Item2;
                    txtFileProg.AppendText(x.Item1);
                });
                ArchiveChunkCompression method = (ArchiveChunkCompression)cmbCompression.SelectedIndex;

                if (trvFiles.SelectedNode.Tag == null)
                {
                    //Is a PP file node
                    List<SubfileHolder> items = trvFiles.SelectedNode.Nodes
                        .Cast<TreeNode>()
                        .Select(x => x.Tag as SubfileHolder)
                        .ToList();

                    prgFileProgress.Value = 0;

                    int i = 0;

                    Task.Run(() =>
                    {
                        long ucb = 0;
                        long cb = 0;

                        foreach (var item in items)
                        {
                            ucb += (long)item.Size;

                            using (Stream data = item.Source.GetStream())
                                cb += PPeX.Utility.TestCompression(data, method);

                            progress.Report(new Tuple<string, int>(
                                    "",
                                    100 * i / items.Count));
                        }

                        string uncompressedSize = PPeX.Utility.GetBytesReadable(ucb);

                        string size = PPeX.Utility.GetBytesReadable(cb);

                        string ratio = ((double)cb / ucb).ToString("P2");
                        switch (method)
                        {
                            case ArchiveChunkCompression.Uncompressed:
                                progress.Report(new Tuple<string, int>("No compression: " + uncompressedSize + " => " + size + " (" + ratio + ")\n", 100));
                                break;
                            case ArchiveChunkCompression.LZ4:
                                progress.Report(new Tuple<string, int>("LZ4 compression: " + uncompressedSize + " => " + size + " (" + ratio + ")\n", 100));
                                break;
                            case ArchiveChunkCompression.Zstandard:
                                progress.Report(new Tuple<string, int>("Zstandard compression: " + uncompressedSize + " => " + size + " (" + ratio + ")\n", 100));
                                break;
                        }
                    });
                }
                else
                {
                    //Is a PP subfile node
                    SubfileHolder sh = trvFiles.SelectedNode.Tag as SubfileHolder;

                    using (Stream stream = sh.Source.GetStream())
                    {
                        string uncompressedSize = PPeX.Utility.GetBytesReadable(stream.Length);

                        long bytes = PPeX.Utility.TestCompression(stream, method);
                        string size = PPeX.Utility.GetBytesReadable(bytes);

                        string ratio = ((double)bytes / stream.Length).ToString("P2");
                        switch (method)
                        {
                            case ArchiveChunkCompression.Uncompressed:
                                progress.Report(new Tuple<string, int>("No compression: " + uncompressedSize + " => " + size + " (" + ratio + ")\n", 100));
                                break;
                            case ArchiveChunkCompression.LZ4:
                                progress.Report(new Tuple<string, int>("LZ4 compression: " + uncompressedSize + " => " + size + " (" + ratio + ")\n", 100));
                                break;
                            case ArchiveChunkCompression.Zstandard:
                                progress.Report(new Tuple<string, int>("Zstandard compression: " + uncompressedSize + " => " + size + " (" + ratio + ")\n", 100));
                                break;
                        }
                    }

                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportItem();
        }

        private void btnTestCompr_Click(object sender, EventArgs e)
        {
            TestCompression();
        }

        private IEnumerable<TreeNode> GetNodeBranch(TreeNode node)
        {
            yield return node;

            foreach (TreeNode child in node.Nodes)
                foreach (var childChild in GetNodeBranch(child))
                    yield return childChild;
        }

        public void Save()
        {
            if (txtArchiveName.Text == "" || txtSaveLocation.Text == "")
                return;

            btnSave.Enabled = false;

            ExtendedArchiveWriter writer = new ExtendedArchiveWriter(txtArchiveName.Text);

            writer.DefaultCompression = (ArchiveChunkCompression)cmbCompression.SelectedIndex;
            writer.ChunkSizeLimit = (ulong)numChunkSize.Value * 1024 * 1024;
            writer.Threads = (int)numThreads.Value;

            Core.Settings.Xx2Precision = (int)numXx2Precision.Value;
            Core.Settings.OpusMusicBitrate = (int)(numMusicBitrate.Value * 1000);
            Core.Settings.OpusVoiceBitrate = (int)(numVoiceBitrate.Value * 1000);

            IProgress<string> progressStatus = new Progress<string>(x =>
            {
                txtSaveProg.AppendText(x);
            });

            IProgress<int> progressPercentage = new Progress<int>(x =>
            {
                prgSaveProgress.Value = x;
            });

            //attempt loading md5 cache
            Core.Settings.UseMd5Cache = chkMd5Cache.Checked;

            if (Core.Settings.UseMd5Cache && File.Exists("HashCache.md5.zs"))
            {
                progressStatus.Report("Loading MD5 cache...\r\n");
                progressPercentage.Report(0);

                using (var decom = new ZstdDecompressor())
                {
                    string rawCache = Encoding.ASCII.GetString(decom.Unwrap(File.ReadAllBytes("HashCache.md5.zs")));

                    Core.Settings.Md5Cache = rawCache.Split('\n').Select(x => CachedMd5.FromString(x)).ToDictionary(x => x.Filename);
                }
            }

            Task.Run(() =>
            {
                FileStream arc = new FileStream(txtSaveLocation.Text, FileMode.Create);

                try
                {
                    progressStatus.Report("Performing first pass...\r\n");
                    progressPercentage.Report(0);

                    var allNodes = trvFiles.Nodes
                        .Cast<TreeNode>()
                        .SelectMany(GetNodeBranch);

                    int i = 1;
                    int total = allNodes.Count();

                    foreach (TreeNode node in allNodes)
                    {
                        if (node.Tag == null)
                            continue;

                        var holder = node.Tag as SubfileHolder;

                        ISubfile subfile = new PPeX.Subfile(
                                holder.Source,
                                node.Text,
                                node.Parent.Text);

                        writer.Files.Add(subfile);

                        i++;
                        if (i % 20 == 0)
                            progressPercentage.Report(100 * i / total);
                    }
                    writer.Write(arc, progressStatus, progressPercentage);

                    btnSave.DynamicInvoke(() => btnSave.Enabled = true);
                }
#if !DEBUG
                catch (Exception ex)
                {
                    progressStatus.Report("ERROR: " + ex.Message + "\n");
                    progressPercentage.Report(0);
                }
#endif
                finally
                {
                    arc.Close();

                    //write hash cache
                    if (Core.Settings.UseMd5Cache)
                        using (var comp = new ZstdCompressor(new ZstdCompressionOptions(3)))
                        {
                            var strings = Core.Settings.Md5Cache.Values.Select(x => x.ToWritableString());

                            if (strings.Count() > 0)
                            {
                                string rawCache = strings.Aggregate((x, y) => x + '\n' + y);

                                File.WriteAllBytes("HashCache.md5.zs", comp.Wrap(Encoding.ASCII.GetBytes(rawCache)));
                            }
                        }

                    this.Invoke(new MethodInvoker(() =>
                    {
                        currentlyOpenedFile = Path.GetFileName(txtSaveLocation.Text);
                        IsModified = false;
                        btnSave.Enabled = true;
                    }));
                }


            });


        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        public void Open()
        {
            string path = ShowOpenFileDialog("Extended PP archive|*.ppx").FirstOrDefault();

            if (string.IsNullOrEmpty(path))
                return;

            CloseFile();
            OpenFile();

            currentlyOpenedFile = Path.GetFileName(path);
            IsModified = false;

            ExtendedArchive arc = new ExtendedArchive(path);

            List<TreeNode> parents = new List<TreeNode>();

            trvFiles.BeginUpdate();

            foreach (var file in arc.RawFiles)
            {
                TreeNode parent = parents.FirstOrDefault(x => x.Text == file.ArchiveName);

                if (parent == null)
                {
                    parent = trvFiles.Nodes.Add(file.ArchiveName);
                    parents.Add(parent);
                }

                TreeNode node = parent.Nodes.Add(file.Name);
                node.Tag = new SubfileHolder(file, file.Name);
                SetAutoIcon(node);
            }

            trvFiles.EndUpdate();

            foreach (var chunk in arc.Chunks)
            {
                var item = lsvChunks.Items.Add(chunk.ID.ToString());
                item.SubItems.Add(PPeX.Utility.GetBytesReadable((long)chunk.UncompressedLength));
                item.SubItems.Add(PPeX.Utility.GetBytesReadable((long)chunk.CompressedLength));
                item.SubItems.Add(chunk.Files.Count.ToString());
                item.SubItems.Add(chunk.Offset.ToString());
                item.SubItems.Add(chunk.Compression.ToString());

                if (chunk.Files.Any(x => x.ArchiveName.Contains("TextureBank")))
                    item.BackColor = Color.LightSalmon;
            }

            lblFileCount.Text = "Total files: " + arc.Chunks.Sum(x => x.Files.Count) + " - " + PPeX.Utility.GetBytesReadable(arc.Chunks.Sum(x => (long)x.UncompressedLength));
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Open();
        }

        public void ImportPP(string filename, IProgress<int> progress)
        {
            ppParser pp = new ppParser(filename);

            TreeNode parent = null;

            string name = Path.GetFileNameWithoutExtension(filename);

            trvFiles.DynamicInvoke(() =>
            {
                parent = trvFiles.Nodes.Add(name);
            });

            int counter = 0;
            foreach (IReadFile file in pp.Subfiles)
            {
                SubfileHolder tag = new SubfileHolder(new PPSource(file), file.Name);
                progress.Report(100 * counter++ / pp.Subfiles.Count);

                trvFiles.DynamicInvoke(() =>
                {
                    TreeNode node = parent.Nodes.Add(file.Name);
                    node.Tag = tag;
                    SetAutoIcon(node);
                });
            }



            this.DynamicInvoke(() =>
            {
                IsModified = true;
            });
        }

        private void btnImportPP_Click(object sender, EventArgs e)
        {
            IEnumerable<string> paths = ShowOpenFileDialog("PP archive files|*.pp", true);

            if (!paths.Any())
                return;

            var progform = new formImporting();

            int counter = 0;

            IProgress<int> progress = new Progress<int>((x) =>
            {
                progform.SetProgress(100 * counter / paths.Count(), x);
            });

            Task t = Task.Factory.StartNew(() =>
            {
                foreach (string file in paths)
                {
                    counter++;
                    ImportPP(file, progress);
                }

                progform.DynamicInvoke(() =>
                    progform.Close()
                    );
            });

            trvFiles.BeginUpdate();
            progform.ShowDialog(this);
            trvFiles.EndUpdate();
        }

        public void ImportFolder(string path)
        {
            var files = Directory.EnumerateFiles(path);

            var parent = trvFiles.Nodes.Add(Path.GetFileName(path));

            string name = Path.GetFileName(path);

            foreach (string file in files)
            {
                TreeNode node = parent.Nodes.Add(Path.GetFileName(file));

                var tag = new SubfileHolder(new FileSource(file), Path.GetFileName(file));
                node.Tag = tag;
                SetAutoIcon(node);
            }

            IsModified = true;
        }

        private void btnImportFolder_Click(object sender, EventArgs e)
        {
            IEnumerable<string> paths = ShowFolderDialog("", true);

            if (!paths.Any())
                return;

            foreach (string file in paths)
            {
                ImportFolder(file);
            }
        }

        private void btnBrowseSave_Click(object sender, EventArgs e)
        {
            string path = ShowSaveFileDialog("Extended PP archive|*.ppx");

            if (string.IsNullOrEmpty(path))
                return;

            txtSaveLocation.Text = path;
        }

        private void trvFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void trvFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            trvFiles.BeginUpdate();

            List<string> PPs = new List<string>();

            foreach (string file in files)
            {
                IsModified = true;

                if (Directory.Exists(file)) //is directory
                {
                    ImportFolder(file);
                }
                else if (file.EndsWith(".pp"))
                {
                    PPs.Add(file);
                }
                else
                {
                    if (trvFiles.SelectedNode != null)
                    {
                        var parent = trvFiles.SelectedNode.Level == 0 ?
                            trvFiles.SelectedNode :
                            trvFiles.SelectedNode.Parent;


                        var node = parent.Nodes.Add(Path.GetFileName(file));

                        node.Tag = new SubfileHolder(new FileSource(file), Path.GetFileName(file));
                        SetAutoIcon(node);

                        if (!node.Parent.IsExpanded)
                            node.Parent.Expand();


                        //trvFiles.SelectedNode = node;

                        //node.edi
                    }
                }
            }
            if (PPs.Count > 0)
            {
                var progform = new formImporting();

                int counter = 0;

                IProgress<int> progress = new Progress<int>((x) =>
                {
                    progform.SetProgress(100 * counter / PPs.Count, x);
                });

                Task t = Task.Factory.StartNew(() =>
                {
                    foreach (string file in PPs)
                    {
                        counter++;
                        ImportPP(file, progress);
                    }

                    progform.DynamicInvoke(() =>
                        progform.Close()
                        );
                });

                progform.ShowDialog(this);
            }

            trvFiles.EndUpdate();
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            if (File.Exists("HashCache.md5.zs"))
                File.Delete("HashCache.md5.zs");
        }

        public void LoadMd5Cache()
        {

        }

        private void verifyArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = ShowOpenFileDialog("Extended PP archive|*.ppx").FirstOrDefault();

            if (string.IsNullOrEmpty(path))
                return;

            var verifyForm = new formVerify(new ExtendedArchive(path));
            verifyForm.ShowDialog();
        }

        private static string showSaveFileDialogWin32(string[][] filters, string defaultname)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog()
            {
                OverwritePrompt = true
            };

            if (defaultname != "")
                dialog.DefaultFileName = defaultname;

            foreach (string[] filterSet in filters)
                dialog.Filters.Add(new CommonFileDialogFilter(filterSet[0], filterSet[1]));

            var result = dialog.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return "";

            return dialog.FileName;
        }


        public static string ShowSaveFileDialog(string filter, string defaultname = "")
        {
            string[][] filters = filter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return showSaveFileDialogWin32(filters, defaultname);
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    OverwritePrompt = true
                };

                if (defaultname != "")
                    dialog.FileName = defaultname;

                string fullfilter = "";
                for (int i = 0; i < filters.Length; i++)
                {
                    if (i > 0)
                        fullfilter += "|";

                    fullfilter += $"{filters[i][0]} ({filters[i][1]})";
                    fullfilter += $"|{filters[i][1]}";
                }

                dialog.Filter = fullfilter;

                var result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return "";

                return dialog.FileName;
            }
        }

        private static IEnumerable<string> showOpenFileDialogWin32(string[][] filters, bool multiple)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = multiple,
                EnsureValidNames = true,
                EnsureFileExists = true,
                EnsurePathExists = true
            };

            foreach (string[] filterSet in filters)
                dialog.Filters.Add(new CommonFileDialogFilter(filterSet[0], filterSet[1]));

            var result = dialog.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return new string[0];

            return dialog.FileNames;
        }

        public static IEnumerable<string> ShowOpenFileDialog(string filter = "", bool multiple = false)
        {
            string[][] filters = filter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return showOpenFileDialogWin32(filters, multiple);
            }
            else
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Multiselect = multiple,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    ValidateNames = true
                };

                string fullfilter = "";
                for (int i = 0; i < filters.Length; i++)
                {
                    if (i > 0)
                        fullfilter += "|";

                    fullfilter += $"{filters[i][0]} ({filters[i][1]})";
                    fullfilter += $"|{filters[i][1]}";
                }

                dialog.Filter = fullfilter;

                var result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return new string[0];

                return dialog.FileNames;
            }
        }

        private static IEnumerable<string> showFolderDialogWin32(string title, bool multiple)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = multiple,
                IsFolderPicker = true,
                EnsureValidNames = true,
                EnsurePathExists = true
            };

            if (title != "")
                dialog.Title = title;

            var result = dialog.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return new string[0];

            return dialog.FileNames;
        }

        public static IEnumerable<string> ShowFolderDialog(string title = "", bool multiple = false)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return showFolderDialogWin32(title, multiple);
            }
            else
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog()
                {
                    SelectedPath = Environment.CurrentDirectory,

                };

                if (title != "")
                    dialog.Description = title;

                var result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return new string[0];

                return new[] { dialog.SelectedPath };
            }
        }

        private void convertExtractedModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path;

            if (string.IsNullOrEmpty(path = ShowFolderDialog().FirstOrDefault()))
                return;

            DirectoryInfo directory = new DirectoryInfo(path);

            if (!directory.Exists)
                return;

            foreach (DirectoryInfo subdir in directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                if (subdir.Name.ToUpper() == "AA2_PLAY" ||
                    subdir.Name.ToUpper() == "AA2_MAKE")
                {
                    foreach (DirectoryInfo ppdir in subdir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                        ImportFolder(ppdir.FullName);
                }
            }
        }

        private void imgLogo_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift) && ModifierKeys.HasFlag(Keys.Control))
            {
                MessageBox.Show("Harsh is still worst", "Fact", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }
        }
    }

    public class SubfileHolder
    {
        public SubfileHolder(IDataSource source, string name)
        {
            Source = source;
            Name = name;
        }

        public IDataSource Source;

        public string MD5 => BitConverter.ToString(Source.Md5).Replace("-", "");

        public ulong Size => Source.Size;

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;

                InternalName = _name.Replace(".wav", ".opus");
            }
        }

        public string InternalName { get; protected set; }

        public string Type
        {
            get
            {
                if (_name.EndsWith(".wav") || _name.EndsWith(".opus"))
                    return "Audio";
                else if (_name.EndsWith(".bmp"))
                    return "Image";
                else
                    return "Raw";

            }
        }
    }
}
