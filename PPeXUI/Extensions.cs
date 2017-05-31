using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPeXUI
{
    public static class Extensions
    {
        public static void DynamicInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(new MethodInvoker(action));
            else
                action();
        }
    }
}
