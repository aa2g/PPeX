using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXM64
{
    public static class Logger
    {
        static bool IsLogging = true;

        static List<string> OrderedList = new List<string>();
        static Dictionary<string, int> AccessedList = new Dictionary<string, int>();
        static Dictionary<string, int> ExtensionList = new Dictionary<string, int>();

        public static void LogFile(string file)
        {
            if (IsLogging)
            {
                OrderedList.Add(file);
                if (AccessedList.TryGetValue(file, out int count))
                    AccessedList[file] = count + 1;
                else
                    AccessedList[file] = 1;

                string extension = System.IO.Path.GetExtension(file);
                if (ExtensionList.TryGetValue(extension, out count))
                    ExtensionList[extension] = count + 1;
                else
                    ExtensionList[extension] = 1;
            }
        }

        public static void Dump()
        {
            System.IO.File.WriteAllText("B:\\ordered.txt", OrderedList.Aggregate((a, b) => a + "\r\n" + b));
            System.IO.File.WriteAllText("B:\\accessed.csv", AccessedList.Select(x => x.Key + "," + x.Value.ToString()).Aggregate((a, b) => a + "\r\n" + b));
            System.IO.File.WriteAllText("B:\\extension.csv", ExtensionList.Select(x => x.Key + "," + x.Value.ToString()).Aggregate((a, b) => a + "\r\n" + b));
        }
    }
}
