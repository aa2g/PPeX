using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Manager
{
    public static class Initializer
    {
        public static void BindDependencies()
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            string dllsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x86");
            var assemblies = new List<Assembly>();

            foreach (string path in new DirectoryInfo(dllsPath).GetFiles("*.dll").Select(x => x.FullName))
            {
                try
                {
                    assemblies.Add(Assembly.LoadFile(path));
                }
                catch (Exception ex)
                {

                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return assemblies.First(x => args.Name == x.FullName);
            };
        }
    }
}
