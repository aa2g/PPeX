using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// Contains core properties and methods.
    /// </summary>
    public static class Core
    {
        public static Settings Settings = new Settings();

        public static Version GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
