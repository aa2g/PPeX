using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PPeX;
using System.IO;
using SB3Utility;
using System.Diagnostics;

namespace PPeXUI
{
    public partial class formDebug : Form
    {
        public formDebug()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            //string dir = textBox1.Text;//@"C:\Users\Admin\Documents\GitHub\AA2Install\AA2Pack\bin\x86\Release\jg2p07_00_00";
            foreach (ListViewItem item in lsvFiles.Items)
            //ParallelOptions opt = new ParallelOptions();
            //opt.MaxDegreeOfParallelism = 6;
            //Parallel.ForEach(lsvFiles.Items.Cast<ListViewItem>(), opt, item =>
            {
                string entry = item.Text;

                string name = Path.GetFileName(entry);

                ExtendedArchiveWriter writer = null;

                if (name.EndsWith(".ppx"))
                {
                    using (FileStream fs = new FileStream(entry.Replace(".ppx", ".pp"), FileMode.Create))
                    {
                        byte[] buffer = PPeX.Utility.CreateHeader(new ExtendedArchive(entry));

                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                else if (name.EndsWith(".pp"))
                {
                    writer = new ExtendedArchiveWriter(entry + "x", name);

                    ppParser pp = new ppParser(entry);

                    List<ppSubfile> read = pp.Subfiles.Cast<ppSubfile>().ToList();

                    /*Parallel.ForEach(read, subfile =>
                    {
                        var arc = new ArchiveFile(new PPSource(subfile), subfile.Name);
                        lock (writer)
                        {
                            writer.Files.Add(arc);
                        }
                        
                    });*/

                    foreach (IReadFile subfile in pp.Subfiles)
                    {
                        //writer.Files.Add(new ArchiveFile(new PPSource(subfile), subfile.Name));
                        Application.DoEvents();
                    }
                }
                else
                {
                    writer = new ExtendedArchiveWriter(entry + ".ppx", name + ".pp");

                    foreach (string file in Directory.EnumerateFiles(entry))
                    {
                        if (file.EndsWith("ogg"))
                            continue;

                       // writer.Files.Add(new ArchiveFile(new FileSource(file), Path.GetFileName(file)));

                        Application.DoEvents();
                    }
                }


                sw.Stop();
                //MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
                sw.Restart();

                if (writer != null)
                    writer.Write();

                item.ForeColor = Color.ForestGreen;
                Application.DoEvents();
                //});
            }
            
            sw.Stop();
            MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
        }

        private void btnDecompress_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var archive = new ExtendedArchive(textBox1.Text);

            string dir = Path.Combine(Path.GetDirectoryName(textBox1.Text), Path.GetFileNameWithoutExtension(textBox1.Text));

            Directory.CreateDirectory(dir);

            sw.Stop();
            MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
            sw.Restart();

            foreach (var file in archive.ArchiveFiles)
            {
                string filename = dir + "\\" + file.Name;
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                using (Stream source = file.GetStream())
                {
                    source.CopyTo(fs);
                }
            }

            sw.Stop();

            MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
        }

        private void lsvFiles_DragDrop(object sender, DragEventArgs e)
        {
            //lsvFiles.Items.Add(e.Data.ToString());
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
                lsvFiles.Items.Add(file);
        }

        private void lsvFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            using (MemoryStream mem = new MemoryStream())
            using (FileStream wav = new FileStream(textBox2.Text.Replace("xgg", "wav"), FileMode.Create))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                AudioSubfile aud = new AudioSubfile(new FileSource(textBox2.Text), "e.xgg", "f.pp");
                aud.WriteToStream(mem);

                sw.Stop();
                mem.Position = 0;
                mem.CopyTo(wav);
                MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
            }
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            PPeX.Manager.wew.Add("");
        }

        private void btnRetype_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lsvFiles.Items)
            {
                ExtendedArchive ex = new ExtendedArchive(item.Text);
                ExtendedArchiveWriter wr = new ExtendedArchiveWriter("B:\\" + Path.GetFileName(item.Text), (ex.Title + ".pp").Replace(".pp.pp", ".pp"));

                foreach (var s in ex.ArchiveFiles)
                {
                    //wr.Files.Add(new ArchiveFile(s, s.Name));
                }

                wr.Write();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lsvFiles.Items.Clear();
        }

        public Dictionary<string, int> MD5counter = new Dictionary<string, int>();
        public Dictionary<string, int> filesizes = new Dictionary<string, int>();

        private void formMain_Load(object sender, EventArgs e)
        {

        }

        private void lsvMD_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void Recalchash()
        {
            long totalcount = 0;

            lsvMD.Items.Clear();
            lsvMD.ListViewItemSorter = null;

            foreach (var item in MD5counter)
            {
                var ses = lsvMD.Items.Add(item.Key);
                ses.SubItems.Add(item.Value.ToString());
                ses.SubItems.Add((item.Value * filesizes[item.Key]).ToString());

                totalcount += ((item.Value - 1) * filesizes[item.Key]);
            }

            lsvMD.ListViewItemSorter = new ListViewItemComparer(1);
            lsvMD.Sort();

            lsvSizes.Items.Clear();
            lsvSizes.ListViewItemSorter = null;

            foreach (var item in filesizes)
            {
                var ses = lsvSizes.Items.Add(item.Key);
                ses.SubItems.Add(item.Value.ToString());
            }

            lsvSizes.ListViewItemSorter = new ListViewItemComparer(0);
            lsvSizes.Sort();

            label1.Text = totalcount.ToString();
        }

        private void lsvMD_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                int check;

                ExtendedArchive ex = new ExtendedArchive(file);
                
                foreach (var item in ex.ArchiveFiles)
                {
                    string md5 = ByteArrayToString(item.Md5);

                    if (!MD5counter.TryGetValue(md5, out check))
                    {
                        if (!item.Flags.HasFlag(ArchiveFileFlags.Duplicate))
                        {
                            MD5counter[md5] = 1;
                            filesizes[md5] = (int)item.Size;
                        }
                    }
                    else
                        MD5counter[md5] = 1 + check;
                }

                
            }

            Recalchash();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        class ListViewItemComparer : System.Collections.IComparer
        {
            private int col;
            public ListViewItemComparer()
            {
                col = 0;
            }
            public ListViewItemComparer(int column)
            {
                col = column;
            }
            public int Compare(object x, object y)
            {
                int returnVal = -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                ((ListViewItem)y).SubItems[col].Text);
                return returnVal * -1;
            }
        }
    }
}
