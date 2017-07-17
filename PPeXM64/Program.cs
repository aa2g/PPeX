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

                LoadedArchives.Add(archive);
            }

            Console.WriteLine("Finished loading");

            IsLoaded = true;
            
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
                        Console.WriteLine(Utility.GetBytesReadable(LoadedMemorySize));
                        break;
                    case "trim":
                        Console.WriteLine(Utility.GetBytesReadable(LoadedMemorySize));
                        TrimMemory((long)(float.Parse(arguments[1]) * 1024 * 1024));
                        Console.WriteLine(Utility.GetBytesReadable(LoadedMemorySize));
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

                    Console.WriteLine("(" + Utility.GetBytesReadable(loadedDiff) + ")");
                }
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
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
