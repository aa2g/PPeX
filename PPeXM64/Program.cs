﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using PPeX;
using System.IO;

namespace PPeXM64
{
    public class Program
    {

        public static Dictionary<FileEntry, ISubfile> FileCache = new Dictionary<FileEntry, ISubfile>();
        public static List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();
        public static Dictionary<string, byte[]> DataCache = new Dictionary<string, byte[]>();

        public static bool LogFiles = false;

        static PipeServer server;

        static void Main(string[] args)
        {
            server = new PipeServer("PPEX");

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

            //System.Diagnostics.Debugger.Launch();

            server.OnRequest += Server_OnRequest;
            string line;

            while ((line = Console.ReadLine()) != "exit")
            {
                switch (line)
                {
                    case "log":
                        LogFiles = !LogFiles;
                        break;
                    case "size":
                        int size = DataCache.Sum(x => x.Value.Length);
                        Console.WriteLine(GetBytesReadable(size));
                        break;
                }
            }
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

            if (!DataCache.ContainsKey(combinedName))
            {
                ArchiveFileSource source = result.Source as ArchiveFileSource;
                ArchiveFileCompression oldCompression = source.Compression;
                source.Compression = ArchiveFileCompression.Uncompressed; //trying to bypass any decompression
                
                using (Stream stream = source.GetStream())
                using (MemoryStream mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    DataCache.Add(combinedName, mem.ToArray());
                }

                source.Compression = oldCompression;
            }

            return true;
        }

        private static void Server_OnRequest(string request, string argument, StreamHandler handler)
        {
            if (request == "preload")
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
            if (request == "load")
            {
                if (LogFiles)
                    Console.WriteLine(argument);

                using (BinaryWriter writer = new BinaryWriter(handler.BaseStream, Encoding.Unicode, true))
                {
                    byte[] data = DataCache[argument];
                    writer.Write(data.Length);
                    writer.Write(data);
                }
            }
            else
            {
                //ignore instead of throwing exception
            }
        }
    }
}
