using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PPeX.Manager
{
    public static class Manager
    {
        public static Dictionary<FileEntry, ISubfile> FileCache = new Dictionary<FileEntry, ISubfile>();
        public static PipeClient Client;

        static Manager()
        {


            string dllsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x86");
            /*
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
            };*/

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(dllsPath, "PPeXM64.exe"));
            p.Start();

            Client = new PipeClient("PPEX");
            var connection = Client.CreateConnection();

            string ready = "False";
            while (ready != "True")
            {
                System.Threading.Thread.Sleep(500);
                connection.WriteString("ready");
                connection.WriteString("");
                ready = connection.ReadString();
            }
        }

        public static void AppendLog(string message)
        {
            using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                fs.Position = fs.Length;
                writer.WriteLine(message);
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
                var connection = Client.CreateConnection();
                connection.WriteString("preload");
                connection.WriteString(filename.ToLower() + "/" + paramFile.ToLower());

                string address = connection.ReadString();
                if (address == "NotAvailable")
                {
                    AppendLog("N/A " + filename + ": " + paramFile);

                    return 0;
                }

                uint compressedSize = uint.Parse(connection.ReadString());
                uint decompressedSize = uint.Parse(connection.ReadString());
                ArchiveFileCompression compression = (ArchiveFileCompression)Enum.Parse(typeof(ArchiveFileCompression), connection.ReadString());
                ArchiveFileType type = (ArchiveFileType)Enum.Parse(typeof(ArchiveFileType), connection.ReadString());

                FileCache.Add(new FileEntry(filename.ToLower(), paramFile.ToLower()),
                        SubfileFactory.Create(
                            new PipedFileSource(address, compressedSize, decompressedSize, compression, type), 
                            type));

                using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    fs.Position = fs.Length;
                    writer.WriteLine("PREALLOC " + filename + ": " + paramFile);
                }

                return FileCache[new FileEntry(filename.ToLower(), paramFile.ToLower())].Size;
            }
        }

        public static object lockObject = new object();

        public unsafe static void Decompress(string paramArchive, string paramFile, byte* outBuffer)
        {
            lock (lockObject)
            {
                ISubfile value;

                string filename = Path.GetFileNameWithoutExtension(paramArchive);

                if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
                {
                    using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        fs.Position = fs.Length;
                        writer.Write("DECOMP " + filename + ": " + paramFile);
                    }

                    using (UnmanagedMemoryStream pt = new UnmanagedMemoryStream(outBuffer, 0, value.Size, FileAccess.Write))
                    {
                        value.WriteToStream(pt);
                    }

                    using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        fs.Position = fs.Length;
                        writer.WriteLine(" || SUCCESS");
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        fs.Position = fs.Length;
                        writer.WriteLine("DEALLOCATED " + filename + ": " + paramFile);
                    }
                }
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
