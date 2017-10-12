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

        private void btnOpen_Click(object sender, EventArgs e)
        {
            xxParser parser = new xxParser(new FileStream(@"B:\A00_00_01_01.xx", FileMode.Open));

            trvObjects.Nodes.Add(makeNode(parser.RootObject));

            System.Diagnostics.Debugger.Break();
        }

        TreeNode makeNode(xxObject obj)
        {
            TreeNode node = new TreeNode(obj.Name);

            foreach (xxObject child in obj.Children)
                node.Nodes.Add(makeNode(child));

            return node;
        } 
    }
}
