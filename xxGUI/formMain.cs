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
using PPeX.Xx2;

namespace xxGUI
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }

        string original = @"B:\A00_00_00_00.xx"; //@"B:\A00_00_01_01.xx"; 

        xxParser parser;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            parser = new xxParser(new FileStream(original, FileMode.Open));

            trvObjects.Nodes.Add(makeNode(parser.RootObject));

            foreach (var texture in parser.Textures)
            {
                var item = lsvTextures.Items.Add(texture.Name);

                item.SubItems.Add(texture.Width + "x" + texture.Height);

                //item.SubItems.Add(texture.Checksum.ToString("X2"));
                using (MemoryStream mem = new MemoryStream(texture.ImageData))
                    item.SubItems.Add(BitConverter.ToString(PPeX.Utility.GetMd5(mem)));

                item.SubItems.Add(PPeX.Utility.GetBytesReadable(texture.ImageData.Length));

                item.Tag = texture.ImageData;
            }
        }

        TreeNode makeNode(xxObject obj)
        {
            TreeNode node = new TreeNode(obj.Name);

            foreach (xxObject child in obj.Children)
                node.Nodes.Add(makeNode(child));

            return node;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Xx2File file = new Xx2File(parser);

            Xx2Writer writer = new Xx2Writer(16);
            writer.Write(file, @"B:\test.xx2");

            ZstdNet.Compressor originalcomp = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(22));
            File.WriteAllBytes(@"B:\test.xx2.zs", originalcomp.Wrap(File.ReadAllBytes(@"B:\test.xx2")));
            //File.WriteAllBytes(original + ".zs", originalcomp.Wrap(File.ReadAllBytes(original)));
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            //var reference = parser.RootObject.Children[0].Children[0].Children[0].Meshes[0].Verticies[2].Points;

            //int count = parser.RootObject.Children[0].Children[0].Children[0].Meshes[0].Verticies.Count(x => compare(x.Points, reference));

            //MessageBox.Show(count.ToString());
        }

        bool compare(float[] a, float[] b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        private void OpenXX2_Click(object sender, EventArgs e)
        {
            trvObjects.Nodes.Clear();

            Xx2File file = Xx2Reader.Read(@"B:\test.xx2");

            trvObjects.Nodes.Add(makeNode(file.RootObject));
        }

        private void btnSaveXX_Click(object sender, EventArgs e)
        {
            Xx2File file = new Xx2File(parser);

            using (FileStream fs = new FileStream("B:\\test.xx", FileMode.Create))
                file.DecodeToXX(fs);
        }

        private void btnScanFolder_Click(object sender, EventArgs e)
        {
            foreach (string file in Directory.EnumerateFiles(@"I:\Artificial Academy 2\patching\jg2e01_00_00\", "*.xx"))
            {
                byte[] data = File.ReadAllBytes(file);

                var xxfile = new Xx2File(new xxParser(new FileStream(file, FileMode.Open)));

                using (MemoryStream mem = new MemoryStream())
                using (MemoryStream write = new MemoryStream())
                {
                    new Xx2Writer(0).Write(xxfile, write);

                    write.Position = 0;

                    var xxfile2 = Xx2Reader.Read(write);

                    xxfile2.DecodeToXX(mem);

                    File.WriteAllBytes("B:\\data1.xx", data);
                    File.WriteAllBytes("B:\\data2.xx", mem.ToArray());

                    if (!PPeX.Utility.CompareBytes(data, mem.ToArray()))
                        MessageBox.Show(file);
                }
            }
        }

        private void lsvTextures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvTextures.SelectedItems.Count == 1)
            {
                var item = lsvTextures.SelectedItems[0];

                byte[] data = (item.Tag as byte[]);

                ImageMagick.MagickImage mimage;

                if (item.Text.EndsWith(".bmp"))
                {
                    data[0] = (byte)'B';
                    data[1] = (byte)'M';

                    mimage = new ImageMagick.MagickImage(data);
                }
                else
                {
                    string path = "B:\\" + item.Text;

                    File.WriteAllBytes(path, data);

                    mimage = new ImageMagick.MagickImage(path);

                    File.Delete(path);
                }
                

                Bitmap image;

                using (var raw = mimage.ToBitmap())
                    image = new Bitmap(raw);

                //var image = Image.FromStream(new MemoryStream(data));

                if (imgTexture.Image != null)
                {
                    imgTexture.Image.Dispose();
                }

                imgTexture.Image = image;

                mimage.Dispose();
            }
        }
    }
}
