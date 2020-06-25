using System;

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