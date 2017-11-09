using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PPeX;
using System.IO;
using SB3Utility;

namespace PPeX_CLI
{
    class Program
    {
        static int ConvertFromReadable(string readable)
        {
            if (readable.ToLower().EndsWith("k"))
                return 1000 * int.Parse(readable.Remove(readable.Length - 1));
            else if (readable.ToLower().EndsWith("m"))
                return 1000000 * int.Parse(readable.Remove(readable.Length - 1));
            else if (readable.ToLower().EndsWith("g"))
                return 1000000000 * int.Parse(readable.Remove(readable.Length - 1));
            else
                return int.Parse(readable);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1 || args[0].ToLower() == "-h")
            {
                Console.WriteLine(@"Usage:

ppex-cli -h
Shows this help dialog

ppex-cli -c [options] folder1 file2.pp output.ppx
Compresses a .ppx archive

ppex-cli -e [options] input.ppx output
Extracts a .ppx archive


[Options]
(defaults listed as well)

-name ""Display Name""
(-c only)
Sets the display name of the archive. Default is the output filename

-compression zstd
(-c only)
Sets the compression method. Can be either 'zstd', 'lz4', or 'uncompressed'

-zstd-level 22
(-c only)
Sets the Zstandard level of compression. Can be between 1 and 22

-chunksize 16M
(-c only)
Sets chunk size of compressed data

-xgg-music 44k
(-c only)
Sets music (2 channel audio) bitrate

-xgg-voice 32k
(-c only)
Sets voice (1 channel audio) bitrate

-xx2-precision 16
(-c only)
Sets .xx2 bit precision. Set to 0 for lossless mode

-threads 1
(-c only)
Sets threads to be used during compression.

-regex .+
Sets a regex to use for compressing or extracting");

                return;
            }

            Regex regex = new Regex(".+");

            int argcounter = 1;
            int chunksize = ConvertFromReadable("16M");
            int threads = -1;
            string name = Path.GetFileNameWithoutExtension(args.Last());
            string compression = "zstd";

            if (args[0].ToLower() == "-c")
            {
                string currentArg;

                while ((currentArg = args[argcounter++]).StartsWith("-"))
                {
                    currentArg = currentArg.ToLower();

                    if (currentArg == "-chunksize")
                    {
                        chunksize = ConvertFromReadable(args[argcounter++]);
                    }
                    else if (currentArg == "-xgg-music")
                    {
                        Core.Settings.XggMusicBitrate = ConvertFromReadable(args[argcounter++]);
                    }
                    else if (currentArg == "-xgg-voice")
                    {
                        Core.Settings.XggVoiceBitrate = ConvertFromReadable(args[argcounter++]);
                    }
                    else if (currentArg == "-xx2-precision")
                    {
                        Core.Settings.Xx2Precision = int.Parse(args[argcounter++]);
                    }
                    else if (currentArg == "-threads")
                    {
                        threads = int.Parse(args[argcounter++]);
                    }
                    else if (currentArg == "-compression")
                    {
                        compression = args[argcounter++].ToLower();
                    }
                    else if (currentArg == "-zstd-level")
                    {
                        Core.Settings.ZstdCompressionLevel = int.Parse(args[argcounter++]);
                    }
                    else if (currentArg == "-name")
                    {
                        name = args[argcounter++];
                    }
                    else if (currentArg == "-regex")
                    {
                        regex = new Regex(args[argcounter++]);
                    }
                }
                
                using (FileStream fs = new FileStream(args.Last(), FileMode.Create))
                {
                    ExtendedArchiveWriter writer = new ExtendedArchiveWriter(fs, name);

                    writer.ChunkSizeLimit = (ulong)chunksize;

                    if (compression == "zstd")
                        writer.DefaultCompression = ArchiveChunkCompression.Zstandard;
                    else if (compression == "lz4")
                        writer.DefaultCompression = ArchiveChunkCompression.LZ4;
                    else if (compression == "uncompressed")
                        writer.DefaultCompression = ArchiveChunkCompression.Uncompressed;

                    if (threads > 0)
                        writer.Threads = threads;


                    foreach (string path in args.Skip(argcounter - 1).Take(args.Length - argcounter))
                    {
                        if (path.EndsWith(".pp") && File.Exists(path))
                        {
                            //.pp file
                            Console.WriteLine("Importing " + Path.GetFileName(path));

                            ImportPP(path, writer, regex);
                        }
                        else if (Directory.Exists(path))
                        {
                            name = Path.GetFileNameWithoutExtension(path) + ".pp";

                            Console.WriteLine("Importing \'" + path + "\" as \"" + name + "\"");

                            int imported = 0;
                            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).ToArray();

                            foreach (string file in files)
                            {
                                if (regex.IsMatch(Path.GetFileName(file)))
                                {
                                    writer.Files.Add(
                                        new PPeX.Subfile(
                                            new FileSource(file),
                                            Path.GetFileName(file),
                                            name));

                                    imported++;
                                }
                            }

                            Console.WriteLine("Imported " + imported + "/" + files.Length + " files");
                        }
                    }

                    writer.Write(new Progress<Tuple<string, int>>(x =>
                    {
                        if (x.Item1.Trim() == "")
                            return;

                        Console.WriteLine("[" + x.Item2 + "%] " + x.Item1.Trim());
                    }));
                }
            }
        }

        static void ImportPP(string filename, ExtendedArchiveWriter writer, Regex regex)
        {
            ppParser pp = new ppParser(filename);
            
            string name = Path.GetFileName(filename);

            int imported = 0;

            foreach (IReadFile file in pp.Subfiles)
            {
                if (regex.IsMatch(file.Name))
                {
                    writer.Files.Add(
                        new PPeX.Subfile(
                            new PPSource(file),
                            file.Name,
                            name));

                    imported++;
                }
            }

            Console.WriteLine("Imported " + imported + "/" + pp.Subfiles.Count + " files");
        }
    }
}
