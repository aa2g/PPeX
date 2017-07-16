using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using PPeX;
using System.IO;
using System.Runtime;

namespace PPeXM64
{
    public class Program
    {

        public static Dictionary<FileEntry, ISubfile> FileCache = new Dictionary<FileEntry, ISubfile>();
        public static List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();
        //public static Dictionary<string, byte[]> DataCache = new Dictionary<string, byte[]>();
        public static List<CachedObject> DataCache = new List<CachedObject>();

        public static bool LogFiles = false;
        public static bool IsLoaded = false;

        static PipeServer server;

        static void Main(string[] args)
        {
            server = new PipeServer("PPEX");

            server.OnRequest += Server_OnRequest;

            foreach (string dir in Directory.EnumerateFiles(Core.Settings.PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
            {
                var archive = new ExtendedArchive(dir);

                foreach (var file in archive.ArchiveFiles)
                {
                    ISubfile subfile = SubfileFactory.Create(file, file.ArchiveName);
                    FileCache[new FileEntry(subfile.ArchiveName.Replace(".pp", "").ToLower(), subfile.Name.ToLower())] = subfile;
                }

                Console.WriteLine("Loaded " + dir);

                /*
                using (FileStream fs = new FileStream(dir, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] buffer = new byte[4096];
                    while (0 < br.Read(buffer, 0, 4096))
                    { }
                }
                */

                LoadedArchives.Add(archive);
            }

            Console.WriteLine("Finished loading");

            IsLoaded = true;

            //System.Diagnostics.Debugger.Launch();
            string line;
            
            while (true)
            {
                line = Console.ReadLine();

                string[] arguments = line.Split(' ');

                switch (arguments[0])
                {
                    case "exit":
                        return;
                    case "log":
                        LogFiles = !LogFiles;
                        break;
                    case "size":
                        Console.WriteLine(GetBytesReadable(LoadedMemorySize));
                        break;
                    case "trim":
                        Console.WriteLine(GetBytesReadable(LoadedMemorySize));
                        TrimMemory((long)(float.Parse(arguments[1]) * 1024 * 1024));
                        Console.WriteLine(GetBytesReadable(LoadedMemorySize));
                        break;
                }
            }
        }

        public static long LoadedMemorySize => DataCache.Sum(x => x.Data.LongLength);

        static byte[] Thresholds = new byte[] { 75, 145, 180, 225, 255 };
        public static void TrimMemory(long MaxSize)
        {
            lock (loadLock)
            {
                for (int i = 0; i < Thresholds.Length; i++)
                {
                    if (LoadedMemorySize < MaxSize)
                        break;

                    long loadedDiff = LoadedMemorySize;

                    byte currentThreshold = Thresholds[i];
                    Console.Write("Pass " + (i + 1) + " at threshold " + currentThreshold + "...  ");

                    List<CachedObject> temp = new List<CachedObject>(DataCache);

                    foreach (var item in temp)
                    {
                        if (item.Priority <= currentThreshold)
                            DataCache.Remove(item);
                    }

                    loadedDiff -= LoadedMemorySize;

                    Console.WriteLine("(" + GetBytesReadable(loadedDiff) + ")");
                }
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

        public static bool TryLoad(string combinedName)
        {
            string[] splitNames = combinedName.Split('/');

            ISubfile result;
            if (!FileCache.TryGetValue(new FileEntry(splitNames[0], splitNames[1]), out result))
                return false;

#warning handle dupes somehow

            if (!DataCache.Any(x => x.Name == combinedName))
            {
                ArchiveFileSource source = result.Source as ArchiveFileSource;
                ArchiveFileCompression oldCompression = source.Compression;
                source.Compression = ArchiveFileCompression.Uncompressed; //trying to bypass any decompression
                
                using (Stream stream = source.GetStream())
                using (MemoryStream mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    CachedObject obj = new CachedObject()
                    {
                        Data = mem.ToArray(),
                        MD5 = source.Md5,
                        Priority = source.Priority,
                        Name = combinedName,
                    };

                    DataCache.Add(obj);
                }

                source.Compression = oldCompression;
            }

            return true;
        }
        
        public static object loadLock = new object();

        private static void Server_OnRequest(string request, string argument, StreamHandler handler)
        {
            if (request == "ready")
            {
                if (IsLoaded)
                    Console.WriteLine("Connected to pipe");

                handler.WriteString(IsLoaded.ToString());
            }
            else if (request == "preload")
            {
                if (!TryLoad(argument))
                {
                    handler.WriteString("NotAvailable");
                    return;
                }

                string[] splitNames = argument.Split('/');
                ISubfile subfile = FileCache[new FileEntry(splitNames[0], splitNames[1])];
                ArchiveFileSource source = subfile.Source as ArchiveFileSource;

                string compression = Enum.GetName(source.Compression.GetType(), source.Compression);
                string type = Enum.GetName(source.Type.GetType(), source.Type);

                handler.WriteString(argument);
                handler.WriteString(source.Length.ToString());
                handler.WriteString(subfile.Size.ToString());
                handler.WriteString(compression);
                handler.WriteString(type);
            }
            else if (request == "load")
            {
                lock (loadLock)
                {
                    TryLoad(argument);

                    if (LogFiles)
                        Console.WriteLine(argument);

                    using (BinaryWriter writer = new BinaryWriter(handler.BaseStream, Encoding.Unicode, true))
                    {
                        byte[] data = DataCache.First(x => x.Name == argument).Data;
                        writer.Write(data.Length);
                        writer.Write(data);
                    }
                }

                
            }
            else
            {
                //ignore instead of throwing exception
            }
        }
    }
}
