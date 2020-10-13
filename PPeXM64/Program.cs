using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using PPeX;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using PPeX.Common;
using PPeX.Compressors;
using PPeX.Encoders;
using Timer = System.Timers.Timer;

namespace PPeXM64
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static CompressedCache Cache;

        public static bool LogFiles = false;
        public static bool IsLoaded = false;

        public static bool Preloading = true;
        public static bool TurboMode = true;

        public static string PPXLocation { get; set; }

        static PipeServer server;

        static Timer TrimTimer = new Timer(5000);

        public static ZstdDecompressor ZstdDecompressor = new ZstdDecompressor();

        public static OpusEncoder OpusEncoder = new OpusEncoder();

        static void Main(string[] args)
        {
            server = new PipeServer("PPEX");

#if DEBUG
            LogFiles = true;
#endif

            TrimTimer = new Timer(TurboMode ? 10000 : 5000);

            if (args.Length > 0 &&
                Directory.Exists(args[0]))
            {
                PPXLocation = args[0];
            }

            PPXLocation = PPXLocation.Replace("\\\\", "\\");

            if (args.Length > 0 &&
                args.Any(x => x.ToLower() == "-nowindow"))
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);
            }
            
            //Attach the handler to the server
            server.OnRequest += Server_OnRequest;
            server.OnDisconnect += Server_OnDisconnect;

            if (Directory.Exists(PPXLocation))
            {
                Console.WriteLine("Loading from " + PPXLocation);

                List<ExtendedArchive> ArchivesToLoad = new List<ExtendedArchive>();

                //Index all .ppx files in the location
                foreach (string arc in Directory.EnumerateFiles(PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
                {
                    var archive = new ExtendedArchive(arc);

                    ArchivesToLoad.Add(archive);
                }

                Cache = new CompressedCache(ArchivesToLoad, new Progress<string>((x) =>
                {
                    Console.WriteLine(x);
                }), 512 * 1024 * 1024);
            }
            else
                Console.WriteLine("Invalid load directory! (" + PPXLocation + ")");

            TrimTimer.Elapsed += (s, e) =>
            {
                Cache.Trim((long)2 * 1024 * 1024 * 1024);
                Console.WriteLine("Cache size:" + Utility.GetBytesReadable(Cache.AllocatedMemorySize));
            };

            TrimTimer.Start();

            Console.WriteLine("Finished loading " + Cache.LoadedArchives.Count + " archive(s)");

            if (Preloading)
            {
                Console.WriteLine("Preloading files...");

                foreach (var chunk in Cache.ReferenceMd5Sums.Keys.Where(x => x.File.EndsWith(".lst")).Select(x => Cache.LoadedFileReferences[x]).Distinct())
                    chunk.Allocate();

                //foreach (var chunk in Cache.LoadedFiles.Where(x => x.Key.Archive.StartsWith("jg2p06")).Select(x => x.Value.Chunk).Distinct())
                //    chunk.Allocate();

                foreach (var chunk in Cache.ReferenceMd5Sums.Keys.Where(x => x.File.EndsWith(".bmp")).Select(x => Cache.LoadedFileReferences[x]).Distinct())
	                chunk.Allocate();

                foreach (var chunk in Cache.ReferenceMd5Sums.Keys.Where(x => x.File.EndsWith(".tga")).Select(x => Cache.LoadedFileReferences[x]).Distinct())
	                chunk.Allocate();

                //foreach (var chunk in Cache.LoadedFiles.Where(x => x.Key.Archive.StartsWith("jg2p07")).Select(x => x.Value.Chunk).Distinct())
                //    chunk.Allocate();

                Console.WriteLine("Preloading complete.");
            }

            Cache.Trim(TrimMethod.GCCompactOnly);

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
                    case "turbo":
                        TurboMode = !TurboMode;
                        break;
                    case "size":
                        Console.WriteLine(Utility.GetBytesReadable(Cache.AllocatedMemorySize));
                        Console.WriteLine(Cache.LoadedFiles.Count + " files allocated");
                        break;
                    case "trim":
                        Cache.Trim((long)(float.Parse(arguments[1]) * 1024 * 1024));
                        break;
                }
            }
        }
        
        

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
            else if (request == "matchfiles")
            {
                //Send a list of all loaded .pp files

                if (LogFiles)
                    Console.WriteLine("!!LOADED FILELIST!!");

                var loadedPP = Cache.LoadedFileReferences.Keys.Select(x => x.Archive).Distinct();

                foreach (string pp in loadedPP)
                {
	                handler.WriteString(pp + ".pp");
                    
	                if (LogFiles)
						Console.WriteLine(pp);
                }

                handler.WriteString("");
            }
            else if (request == "load")
            {
                //Transfer the file
                lock (Cache.LoadLock)
                {
                    string[] splitNames = argument.Replace("data/", "").Split('/');
                    FileEntry entry = new FileEntry(splitNames[0], splitNames[1]);

                    Logger.LogFile(argument);

                    //Ensure we have the file
                    if (!Cache.ReferenceMd5Sums.ContainsKey(entry))
                    {
                        //We don't have the file
                        handler.WriteString("NotAvailable");

                        if (LogFiles)
                            Console.WriteLine("!" + argument);

                        return;
                    }

                    if (LogFiles)
                        Console.WriteLine(argument);

                    //Write the data to the pipe

                    var fileMd5 = Cache.ReferenceMd5Sums[entry];

                    if (!Cache.LoadedFiles.TryGetValue(fileMd5, out var cachedFile))
                    {
                        //Console.WriteLine("Cache miss");

	                    var chunk = Cache.LoadedFileReferences[entry];

                        chunk.Allocate();

                        // wait for the file to be available
                        while (true)
                        {
	                        if (Cache.LoadedFiles.TryGetValue(fileMd5, out cachedFile) && cachedFile.Ready)
	                        {
		                        break;
	                        }

                            Thread.Sleep(50);
                        }
                    }

                    while (!cachedFile.Ready)
                    {
	                    Thread.Sleep(50);
                    }

                    using var compressedDataRef = cachedFile.GetMemory();
                    using var buffer = MemoryPool<byte>.Shared.Rent((int)cachedFile.UncompressedSize * 2);

                    ZstdDecompressor.DecompressData(compressedDataRef.Memory.Span, buffer.Memory.Span, out int uncompressedSize);

                    //Console.WriteLine($"Expected: {Utility.GetBytesReadable(cachedFile.UncompressedSize)} Actual: {Utility.GetBytesReadable(uncompressedSize)}");

                    if (entry.File.EndsWith("wav"))
                    {
	                    using (var inputStream = new ReadOnlyMemoryStream(buffer.Memory.Slice(0, (int)uncompressedSize)))
                        //using (var rentedSpan = MemoryPool<byte>.Shared.Rent(48_000_000))
                        using (var outputStream = new MemoryStream())
	                    {
		                    OpusEncoder.Decode(inputStream, outputStream, false);

		                    outputStream.Position = 0;

                            //Console.WriteLine($"Decompressed wav size: {Utility.GetBytesReadable(outputStream.Length)}");

		                    handler.WriteString(outputStream.Length.ToString());
                            outputStream.CopyTo(handler.BaseStream);
                        }
                        
                    }
                    else
                    {
	                    handler.WriteString(uncompressedSize.ToString());
                        handler.BaseStream.Write(buffer.Memory.Slice(0, (int)uncompressedSize).Span);
                    }
                }
            }
            else
            {
                //Unknown command
                //Ignore instead of throwing exception
                Console.WriteLine("Unknown request: " + request + " [:] " + argument);
            }
        }

        private static void Server_OnDisconnect(object sender, EventArgs e)
        {
            Cache.Trim(TrimMethod.All); //deallocate everything

            Console.WriteLine("Pipe disconnected, game has closed");
            //Logger.Dump();

            System.Threading.Thread.Sleep(1500);
            Environment.Exit(0);
        }
    }
}
