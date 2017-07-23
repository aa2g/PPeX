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

#if DEBUG
            LogFiles = true;
#endif

            //Attach the handler to the server
            server.OnRequest += Server_OnRequest;

            //Index all .ppx files in the location
            foreach (string dir in Directory.EnumerateFiles(Core.Settings.PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
            {
                var archive = new ExtendedArchive(dir);

                foreach (var file in archive.ArchiveFiles)
                {
                    FileCache[new FileEntry(file.ArchiveName.Replace(".pp", "").ToLower(), file.Name.ToLower())] = file;
                }

                Console.WriteLine("Loaded " + dir);

                LoadedArchives.Add(archive);
            }

            Console.WriteLine("Finished loading");

            IsLoaded = true;
            
            string line;
            
            //Handle arguments from the user
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

        /// <summary>
        /// The total amount of memory used by cached data.
        /// </summary>
        public static long LoadedMemorySize => DataCache.Sum(x => x.Data.LongLength);

        /// <summary>
        /// The memory pressure thresholds to use when trimming memory.
        /// </summary>
        static byte[] Thresholds = new byte[] { 75, 145, 180, 225, 255 };

        /// <summary>
        /// Trims memory using a generation-based prioritizer.
        /// </summary>
        /// <param name="MaxSize">The maximum allowed size of cached data.</param>
        public static void TrimMemory(long MaxSize)
        {
            lock (loadLock)
            {
                //Iterate on the static thresholds
                for (int i = 0; i < Thresholds.Length; i++)
                {
                    if (LoadedMemorySize < MaxSize)
                        break;

                    long loadedDiff = LoadedMemorySize;

                    byte currentThreshold = Thresholds[i];
                    Console.Write("Pass " + (i + 1) + " at threshold " + currentThreshold + "...  ");

                    List<CachedObject> temp = new List<CachedObject>(DataCache);

                    //Deallocate each file based on its priority
                    foreach (var item in temp)
                    {
                        if (item.Priority <= currentThreshold)
                            DataCache.Remove(item);
                    }

                    loadedDiff -= LoadedMemorySize;

                    Console.WriteLine("(" + Utility.GetBytesReadable(loadedDiff) + ")");
                }
            }

            //Collect garbage and compact the heap
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        
        /// <summary>
        /// Attempt to cache a file in memory.
        /// </summary>
        /// <param name="combinedName">The combined name (or address) of the file to cache.</param>
        /// <returns></returns>
        public static bool TryLoad(string combinedName)
        {
            string[] splitNames = combinedName.Split('/');

            ISubfile result;
            if (!FileCache.TryGetValue(new FileEntry(splitNames[0], splitNames[1]), out result))
                //We don't have the file
                return false;

#warning handle dupes somehow

            if (!DataCache.Any(x => x.Name == combinedName))
            {
                //We haven't cached the file yet
                ArchiveFileSource source = result.Source as ArchiveFileSource;
                ArchiveFileCompression oldCompression = source.Compression;

                //We don't want to decompress the data (to keep memory footprint small) so we mark it as uncompressed
                source.Compression = ArchiveFileCompression.Uncompressed;
                
                using (Stream stream = source.GetStream())
                using (MemoryStream mem = new MemoryStream())
                {
                    //Copy it to a cached object
                    stream.CopyTo(mem);
                    CachedObject obj = new CachedObject()
                    {
                        Data = mem.ToArray(),
                        Metadata = source.Metadata,
                        MD5 = source.Md5,
                        Priority = source.Priority,
                        Name = combinedName,
                    };

                    DataCache.Add(obj);
                }

                //Restore the original compression value
                source.Compression = oldCompression;
            }

            return true;
        }
        
        public static object loadLock = new object();

        /// <summary>
        /// Handler for any pipe requests.
        /// </summary>
        /// <param name="request">The command to execute.</param>
        /// <param name="argument">Any additional arguments.</param>
        /// <param name="handler">The streamhandler to use.</param>
        private static void Server_OnRequest(string request, string argument, StreamHandler handler)
        {
            if (request == "ready")
            {
                //Notify the AA2 instance that we are ready
                if (IsLoaded)
                    Console.WriteLine("Connected to pipe");

                handler.WriteString(IsLoaded.ToString());
            }
            else if (request == "load")
            {
                //Transfer the file
                lock (loadLock)
                {
                    //Ensure we have the file in memory
                    //Cache the file into memory
                    if (!TryLoad(argument))
                    {
                        //We don't have the file
                        handler.WriteString("NotAvailable");
                        return;
                    }

                    if (LogFiles)
                        Console.WriteLine(argument);

                    //Write the data to the pipe
                    using (BinaryWriter writer = new BinaryWriter(handler.BaseStream, Encoding.Unicode, true))
                    {
                        CachedObject cached = DataCache.First(x => x.Name == argument);

                        string[] splitNames = argument.Split('/');
                        ISubfile subfile = FileCache[new FileEntry(splitNames[0], splitNames[1])];
                        ArchiveFileSource source = subfile.Source as ArchiveFileSource;

                        MemorySource mem = new MemorySource(cached.Data, cached.Metadata, source.Compression, source.Encoding);

#warning need to remove this buffer
                        //We can't trust the subfile size reading
                        using (Stream temp = new MemoryStream())
                        {
                            handler.WriteString(temp.Length.ToString());

                            temp.CopyTo(handler.BaseStream);
                        }
                            
                    }
                }
            }
            else
            {
                //Unknown command
                //Ignore instead of throwing exception
            }
        }
    }
}
