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

        string original = @"B:\A00_39_02_04.xx"; //@"B:\A00_00_01_01.xx";

        xxParser parser;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            parser = new xxParser(new FileStream(original, FileMode.Open));

            trvObjects.Nodes.Add(makeNode(parser.RootObject));
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

            Xx2Writer writer = new Xx2Writer();
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
    }
}
