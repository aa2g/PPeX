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
        public static List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();
        public static List<CachedChunk> ChunkCache = new List<CachedChunk>();
        public static Dictionary<FileEntry, CachedFile> FileCache = new Dictionary<FileEntry, CachedFile>();

        public static bool LogFiles = false;
        public static bool IsLoaded = false;

        static PipeServer server;

        static void Main(string[] args)
        {
            server = new PipeServer("PPEX");

#if DEBUG
            LogFiles = true;
#endif

            if (args.Length > 0 &&
                Directory.Exists(args[0]))
            {
                Core.Settings.PPXLocation = args[0];
            }
            else
            {
                Core.Settings.PPXLocation = Utility.GetGameDir() + "\\data";
            }

            Core.Settings.PPXLocation = Core.Settings.PPXLocation.Replace("\\\\", "\\");

            //Attach the handler to the server
            server.OnRequest += Server_OnRequest;
            server.OnDisconnect += Server_OnDisconnect;

            if (Directory.Exists(Core.Settings.PPXLocation))
            {
                Console.WriteLine("Loading from " + Core.Settings.PPXLocation);

                //Index all .ppx files in the location
                foreach (string arc in Directory.EnumerateFiles(Core.Settings.PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
                {
                    var archive = new ExtendedArchive(arc);

                    foreach (var chunk in archive.Chunks)
                    {
                        ChunkCache[(int)chunk.ID] = new CachedChunk(chunk);
                    }

                    foreach (var file in archive.Files)
                    {
                        FileCache[new FileEntry(file.ArchiveName, file.Name)] = new CachedFile(file, ChunkCache.First(x => x.ID == file.ChunkID));
                    }

                    Console.WriteLine("Loaded \"" + archive.Title + "\" (" + archive.Files.Count + " files)");

                    LoadedArchives.Add(archive);
                }
            }
            else
                Console.WriteLine("Invalid load directory! (" + Core.Settings.PPXLocation + ")");

            Console.WriteLine("Finished loading " + LoadedArchives.Count + " archive(s)");

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
                        Environment.Exit(0);
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
        public static long LoadedMemorySize => ChunkCache.Sum(x => (long)x.Data?.LongLength);

        /// <summary>
        /// Trims memory using a generation-based prioritizer.
        /// </summary>
        /// <param name="MaxSize">The maximum allowed size of cached data.</param>
        public static void TrimMemory(long MaxSize)
        {
            lock (loadLock)
            {
                long loadedDiff = LoadedMemorySize;

                IOrderedEnumerable<CachedChunk> sortedChunks = ChunkCache.AsEnumerable().OrderBy(x => x.Accesses);

                long accumulatedSize = 0;

                IEnumerable<CachedChunk> removedChunks = sortedChunks.TakeWhile(x => (accumulatedSize += x.Data.Length) < (loadedDiff - MaxSize));

                foreach (var chunk in removedChunks)
                    chunk.Deallocate();

                Console.WriteLine("Freed " + removedChunks.Count() + " chunk(s) (" + Utility.GetBytesReadable(accumulatedSize) + ")");
            }

            //Collect garbage and compact the heap
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
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
                    string[] splitNames = argument.Split('/');
                    FileEntry entry = new FileEntry(splitNames[0], splitNames[1]);

                    //Ensure we have the file
                    if (!FileCache.ContainsKey(entry))
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
                        CachedFile cached = FileCache[entry];
                        
                        using (Stream output = cached.GetStream())
                        {
                            handler.WriteString(cached.Length.ToString());

                            output.CopyTo(handler.BaseStream);
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

        private static void Server_OnDisconnect(object sender, EventArgs e)
        {
            TrimMemory(0); //deallocate everything

            Console.WriteLine("Pipe disconnected, game has closed");

            System.Threading.Thread.Sleep(1500);
            Environment.Exit(0);
        }
    }
}
