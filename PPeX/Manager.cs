using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public static class Manager
    {
        public static Dictionary<FileEntry, ISubfile> FileCache = new Dictionary<FileEntry, ISubfile>();
        public static List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();

        public static List<string> wew = new List<string>();

        public static List<ExtendedArchive> archives = new List<ExtendedArchive>();

        private static Settings Settings;


        static Manager()
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            string dllsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x86");//Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
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
            /*
            foreach (string path in new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("*.dll").Select(x => x.FullName))
            {
                try
                {
                    assemblies.Add(Assembly.LoadFile(path));
                }
                catch (Exception ex)
                {

                }
            }*/

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return assemblies.First(x => args.Name == x.FullName);
            };

            Settings = Settings.Load();

            foreach (string dir in Directory.EnumerateFiles(Settings.PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
            {
                var archive = new ExtendedArchive(dir);

                foreach (var file in archive.ArchiveFiles)
                {
                    ISubfile subfile = SubfileFactory.Create(file, file.ArchiveName);
                    FileCache[new FileEntry(subfile.ArchiveName.Replace(".pp", "").ToLower(), subfile.Name.ToLower())] = subfile;
                }

                using (FileStream fs = new FileStream(dir, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] buffer = new byte[4096];
                    while (0 < br.Read(buffer, 0, 4096))
                    { }
                }

                LoadedArchives.Add(archive);
            }

            
            Dictionary<string, List<string>> ppf = new Dictionary<string, List<string>>();
            foreach (var kv in FileCache)
            {
                if (!ppf.ContainsKey(kv.Key.Archive))
                {
                    ppf.Add(kv.Key.Archive, new List<string>());
                }
                ppf[kv.Key.Archive].Add(kv.Key.File);
            }

            foreach (var arc in ppf)
            {
                File.WriteAllBytes(@"I:\AA2\Pure\Play\data\placeholder\" + arc.Key + ".pp", Utility.CreateHeader(arc.Value));
            }
            
        }


        public static uint PreAlloc(string paramArchive, string paramFile)
        {
            string filename = Path.GetFileNameWithoutExtension(paramArchive);

            ISubfile value;
            if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
            {
                return value.Size;
            }
            else
            {
                using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    fs.Position = fs.Length;
                    writer.WriteLine(filename + " : " + paramFile + " False");
                }
            }
            return 0;
        }

        public unsafe static void Decompress(string paramArchive, string paramFile, byte* outBuffer) //IntPtr outBuffer
        {
            ISubfile value;

            string filename = Path.GetFileNameWithoutExtension(paramArchive);

            if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
            {
                using (UnmanagedMemoryStream pt = new UnmanagedMemoryStream(outBuffer, 0, value.Size, FileAccess.Write))
                {
                    value.WriteToStream(pt);
                }
            }
            else
            {
                using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    fs.Position = fs.Length;
                    writer.WriteLine("LEAK! " + filename + ": " + paramFile + " False");
                }
            }
        }

        [System.Diagnostics.DebuggerDisplay("{Archive}: {File}")]
        public class FileEntry
        {
            public string Archive;
            public string File;

            public FileEntry(string Archive, string File)
            {
                this.Archive = Archive.ToLower();
                this.File = File.ToLower();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is FileEntry))
                    return false;
                var other = obj as FileEntry;
                return (Archive == other.Archive) && (File == other.File);
            }

            public override int GetHashCode()
            {
                return (Archive + File).GetHashCode();
            }
        }
    }
}
