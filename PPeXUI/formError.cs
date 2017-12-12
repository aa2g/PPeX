using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPeXUI
{
    public partial class formError : Form
    {
        public formError()
        {
            InitializeComponent();
        }

        public formError(string exception, string[] files)
        {
            InitializeComponent();

            txtException.Text = exception;

            foreach (string file in files)
                lsbFiles.Items.Add(file);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
