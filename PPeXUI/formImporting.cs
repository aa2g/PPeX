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
    public partial class formImporting : Form
    {
        public formImporting()
        {
            InitializeComponent();
        }

        public void SetProgress(int progress, int minor)
        {
            if (prgMajor.InvokeRequired)
                prgMajor.Invoke(new MethodInvoker(() => {
                    prgMajor.Value = progress;
                    prgMinor.Value = minor;
                }));
            else
            {
                prgMajor.Value = progress;
                prgMinor.Value = minor;
            }
        }
    }
}
