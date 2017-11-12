using PPeX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crc32C;
using System.IO;

namespace PPeXUI
{
    public partial class formVerify : Form
    {
        ExtendedArchive arc;

        public formVerify(ExtendedArchive arc)
        {
            InitializeComponent();

            this.arc = arc;
        }

        private void formVerify_Shown(object sender, EventArgs e)
        {
            prgChunkProgress.Maximum = arc.Chunks.Count;

            var files = arc.Files;
                /*arc.Chunks.Where(x => x.Type == ChunkType.Generic)
                .SelectMany(x => x.Files)
                .Where(x => x.Type == ArchiveFileType.Raw);*/

            prgFileProgress.Maximum = files.Count();

            Task.Factory.StartNew(() =>
            {
                int fails = 0;

                foreach (var chunk in arc.Chunks)
                {
                    ListViewItem item = null;

                    lsvChunkHash.Invoke(new MethodInvoker(() =>
                    {
                        item = lsvChunkHash.Items.Add(chunk.ID.ToString());
                        item.SubItems.Add(chunk.CRC32C.ToString("X8"));
                    }));

                    uint crc;

                    using (MemoryStream mem = new MemoryStream())
                    using (Stream stream = chunk.GetRawStream())
                    {
                        stream.CopyTo(mem);
                        mem.Position = 0;
                        crc = Crc32CAlgorithm.Compute(mem.ToArray());
                    }

                    lsvChunkHash.Invoke(new MethodInvoker(() =>
                    {
                        item.SubItems.Add(crc.ToString("X8"));
                        if (crc != chunk.CRC32C)
                        {
                            item.BackColor = Color.Red;
                            fails++;
                        }
                        prgChunkProgress.Value++;
                    }));
                }

                foreach (var file in files)
                {
                    ListViewItem item = null;

                    lsvFileHash.Invoke(new MethodInvoker(() =>
                    {
                        item = lsvFileHash.Items.Add(file.ArchiveName);
                        item.SubItems.Add(file.Name);
                        item.SubItems.Add(((Md5Hash)file.Source.Md5).ToString());
                    }));

                    Md5Hash hash;
                    
                    using (Stream stream = file.GetRawStream())
                    {
                        hash = Utility.GetMd5(stream);
                    }

                    lsvFileHash.Invoke(new MethodInvoker(() =>
                    {
                        item.SubItems.Add(hash.ToString());
                        if (!Utility.CompareBytes(file.Source.Md5, hash))
                        {
                            item.BackColor = Color.Red;
                            fails++;
                        }
                       prgFileProgress.Value++;
                    }));
                }

                btnOk.Invoke(new MethodInvoker(() =>
                {
                    MessageBox.Show("Operation completed with " + fails + " hash fails.");
                    btnOk.Enabled = true;
                }));
            });
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
