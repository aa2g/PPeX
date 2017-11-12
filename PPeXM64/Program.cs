using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using PPeX;
using System.IO;
using System.Runtime;
using System.Timers;

namespace PPeXM64
{
    public class Program
    {
        public static CompressedCache Cache;

        public static bool LogFiles = false;
        public static bool IsLoaded = false;

        static PipeServer server;

        static Timer timer = new Timer(10000);

        static void Main(string[] args)
        {
            timer.Stop();

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

                List<ExtendedArchive> ArchivesToLoad = new List<ExtendedArchive>();

                //Index all .ppx files in the location
                foreach (string arc in Directory.EnumerateFiles(Core.Settings.PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
                {
                    var archive = new ExtendedArchive(arc);

                    ArchivesToLoad.Add(archive);
                }

                Cache = new CompressedCache(ArchivesToLoad, new Progress<string>((x) =>
                {
                    Console.WriteLine(x);
                }));
            }
            else
                Console.WriteLine("Invalid load directory! (" + Core.Settings.PPXLocation + ")");

            timer.Elapsed += (s, e) =>
            {
                Cache.Trim((long)2 * 1024 * 1024 * 1024);
            };

            timer.Start();

            Console.WriteLine("Finished loading " + Cache.LoadedArchives.Count + " archive(s)");

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
                        Console.WriteLine(Utility.GetBytesReadable(Cache.AllocatedMemorySize));
                        Console.WriteLine(Cache.TotalFiles.Count(x => x.Allocated) + " files allocated");
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
            if (request == "pplist")
            {
                //Send a list of all loaded .pp files

                var loadedPP = Cache.TotalFiles.Select(x => x.ArchiveName).Distinct();

                if (loadedPP.Count() > 0)
                    handler.WriteString(loadedPP.Aggregate((a, b) => a + " " + b));
                else
                    handler.WriteString("");
            }
            else if (request == "load")
            {
                //Transfer the file
                lock (Cache.LoadLock)
                {
                    string[] splitNames = argument.Replace("data/", "").Split('/');
                    FileEntry entry = new FileEntry(splitNames[0], splitNames[1]);

                    //Ensure we have the file
                    if (!Cache.LoadedFiles.ContainsKey(entry))
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
                    using (BinaryWriter writer = new BinaryWriter(handler.BaseStream, Encoding.Unicode, true))
                    {
                        CachedFile cached = Cache.LoadedFiles[entry];
                        
                        using (Stream output = cached.GetStream())
                        {
                            handler.WriteString(output.Length.ToString());

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
            Cache.Trim(TrimMethod.All); //deallocate everything

            Console.WriteLine("Pipe disconnected, game has closed");

            System.Threading.Thread.Sleep(1500);
            Environment.Exit(0);
        }
    }
}
