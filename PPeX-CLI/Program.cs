using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PPeX;
using System.IO;
using PPeX.External.PP;
using PPeX.Encoders;

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

ppex-cli -e [options] input.ppx output-directory/
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

-xx2-precision 0
(-c only)
Sets .xx2 bit precision. Set to 0 for lossless mode. Cannot be used with -xx2-quality.

-xx2-quality
(-c only)
Sets .xx2 encode quality. Not enabled by default. Cannot be used with -xx2-precision.

-threads 1
(-c only)
Sets threads to be used during compression.

-no-encode ""a^""
(-c only)
Defines which files should be left unencoded as a regex. Default is none.

-decode on
(-e only)
Sets rules for decoding any encoded files on extraction, such as .xx3 to .xx.
Default is on, available options are 'on' and 'off'.

-regex "".+""
Sets a regex to use for compressing or extracting.");

                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args[0].ToLower() == "-c")
            {
                CompressArg(args);
            }
            if (args[0].ToLower() == "-e")
            {
                DecompressArg(args);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("-- UNHANDLED EXCEPTION --");
                Console.WriteLine(ex.Message);
                Console.WriteLine($"-- USER DATA ({ex.Data.Count}) --");

                foreach (var key in ex.Data.Keys)
                {
                    Console.WriteLine(key.ToString());
                    Console.WriteLine(ex.Data[key].ToString());
                }
                
                Console.WriteLine("-- STACK TRACE --");
                Console.WriteLine(ex.StackTrace);
            }
        }

        #region Compress
        static void CompressArg(string[] args)
        {
            Regex regex = new Regex(".+");
            Regex unencodedRegex = new Regex("a^");

            int argcounter = 1;
            int chunksize = ConvertFromReadable("16M");
            int threads = 1;
            string name = Path.GetFileNameWithoutExtension(args.Last());
            string compression = "zstd";
            string currentArg;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                Core.Settings.XggFrameSize = 0.060;

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
                    Core.Settings.Xx2IsUsingQuality = false;
                }
                else if (currentArg == "-xx2-quality")
                {
                    Core.Settings.Xx2Quality = float.Parse(args[argcounter++]);
                    Core.Settings.Xx2IsUsingQuality = false;
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
                else if (currentArg == "-no-encode")
                {
                    unencodedRegex = new Regex(args[argcounter++]);
                }
            }

            compression = compression.ToLower();

            using (FileStream fs = new FileStream(args.Last(), FileMode.Create))
            {
                ExtendedArchiveWriter writer = new ExtendedArchiveWriter(fs, name);

                writer.ChunkSizeLimit = (ulong)chunksize;
                writer.Threads = threads;

                if (compression == "zstd")
                    writer.DefaultCompression = ArchiveChunkCompression.Zstandard;
                else if (compression == "lz4")
                    writer.DefaultCompression = ArchiveChunkCompression.LZ4;
                else if (compression == "uncompressed")
                    writer.DefaultCompression = ArchiveChunkCompression.Uncompressed;


                foreach (string path in args.Skip(argcounter - 1).Take(args.Length - argcounter))
                {
                    if (path.EndsWith(".pp") && File.Exists(path))
                    {
                        //.pp file
                        Console.WriteLine("Importing " + Path.GetFileName(path));

                        ImportPP(path, writer, regex, unencodedRegex);
                    }
                    else if (Directory.Exists(path))
                    {
                        name = Path.GetFileNameWithoutExtension(path) + ".pp";

                        Console.WriteLine("Importing \'" + path + "\" as \"" + name + "\"");

                        int imported = 0;
                        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).ToArray();

                        foreach (string file in files)
                        {
                            string fullName = name + "/" + Path.GetFileName(file);

                            if (regex.IsMatch(fullName))
                            {
                                if (unencodedRegex.IsMatch(fullName))
                                {
                                    writer.Files.Add(
                                        new PPeX.Subfile(
                                            new FileSource(file),
                                            Path.GetFileName(file),
                                            name,
                                            ArchiveFileType.Raw));
                                }
                                else
                                {
                                    writer.Files.Add(
                                        new PPeX.Subfile(
                                            new FileSource(file),
                                            Path.GetFileName(file),
                                            name));
                                }


                                imported++;
                            }
                        }

                        Console.WriteLine("Imported " + imported + "/" + files.Length + " files");
                    }
                }

                int lastProgress = 0;

                object progressLock = new object();
                bool isUpdating = true;

                Console.CursorVisible = false;

                Progress<string> progressStatus = new Progress<string>(x =>
                {
                    if (!isUpdating)
                        return;

                    lock (progressLock)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);

                        for (int i = 0; i < Console.WindowWidth - 1; i++)
                            Console.Write(" ");

                        Console.SetCursorPosition(0, Console.CursorTop);

                        Console.WriteLine(x.Trim());

                        Console.Write("[" + lastProgress + "% complete]");
                    }
                });

                Progress<int> progressPercentage = new Progress<int>(x =>
                {
                    if (!isUpdating)
                        return;

                    lock (progressLock)
                    {
                        lastProgress = x;

                        Console.SetCursorPosition(0, Console.CursorTop);

                        Console.Write("[" + lastProgress + "% complete]");
                    }
                });

                writer.Write(progressStatus, progressPercentage);

                //wait for progress to update
                while (lastProgress != 100)
                    System.Threading.Thread.Sleep(50);

                lock (progressPercentage)
                {
                    isUpdating = false;
                    Console.CursorVisible = true;
                }
            }
        }

        static void ImportPP(string filename, ExtendedArchiveWriter writer, Regex regex, Regex unencodedRegex)
        {
            ppParser pp = new ppParser(filename);
            
            string name = Path.GetFileName(filename);

            int imported = 0;

            foreach (IReadFile file in pp.Subfiles)
            {
                string fullName = name + "/" + file.Name;

                if (regex.IsMatch(fullName))
                {
                    if (unencodedRegex.IsMatch(fullName))
                    {
                        writer.Files.Add(
                        new PPeX.Subfile(
                            new PPSource(file),
                            file.Name,
                            name,
                            ArchiveFileType.Raw));
                    }
                    else
                    {
                        writer.Files.Add(
                        new PPeX.Subfile(
                            new PPSource(file),
                            file.Name,
                            name));
                    }

                    imported++;
                }
            }

            Console.WriteLine("Imported " + imported + "/" + pp.Subfiles.Count + " files");
        }
        #endregion

        #region Decompress
        static void DecompressArg(string[] args)
        {
            Regex regex = new Regex(".+");

            int argcounter = 1;
            string decode = "on";
            string currentArg;

            while ((currentArg = args[argcounter++]).StartsWith("-"))
            {
                currentArg = currentArg.ToLower();

                if (currentArg == "-decode")
                {
                    decode = args[argcounter++];
                }
                else if (currentArg == "-regex")
                {
                    regex = new Regex(args[argcounter++]);
                }
            }

            decode = decode.ToLower();

            ExtendedArchive archive = new ExtendedArchive(currentArg);
            DirectoryInfo outputDirectory = new DirectoryInfo(args[argcounter++]);

            if (!outputDirectory.Exists)
                outputDirectory.Create();

            if (decode == "off")
            {
                //decoding raw entries
                foreach (var file in archive.RawFiles)
                {
                    string fullname = file.ArchiveName + "/" + file.Name;
                    string arcname = file.ArchiveName.Replace(".pp", "");

                    if (!Directory.Exists(outputDirectory.FullName + "\\" + arcname))
                        Directory.CreateDirectory(outputDirectory.FullName + "\\" + arcname);

                    if (regex.IsMatch(fullname))
                    {
                        Console.WriteLine("Exporting " + fullname);

                        using (FileStream fs = new FileStream(outputDirectory.FullName + "\\" + arcname + "\\" + file.Name, FileMode.Create))
                        using (Stream subfileStream = file.GetRawStream())
                        {
                            subfileStream.CopyTo(fs);
                        }
                    }
                }
            }
            else if (decode == "on" || decode == "full")
            {
                //decoding raw entries
                foreach (var file in archive.Files)
                {
                    string fullname = file.ArchiveName + "/" + file.Name;
                    string arcname = file.ArchiveName.Replace(".pp", "");

                    if (!Directory.Exists(outputDirectory.FullName + "\\" + arcname))
                        Directory.CreateDirectory(outputDirectory.FullName + "\\" + arcname);

                    if (regex.IsMatch(fullname))
                    {
                        Console.WriteLine("Exporting " + fullname);

                        if (decode == "full" && file.Type == ArchiveFileType.XggAudio)
                        {
                            string fileName = file.Name.Replace(".xgg", ".wav");

                            using (FileStream fs = new FileStream(outputDirectory.FullName + "\\" + arcname + "\\" + fileName, FileMode.Create))
                            using (Stream stream = file.GetRawStream())
                            {
                                stream.CopyTo(fs);
                            }
                        }
                        else if (file.Type != ArchiveFileType.XggAudio)
                        {
                            string fileName;

                            using (IDecoder nameExchanger = EncoderFactory.GetDecoder(Stream.Null, archive, file.Type))
                                fileName = nameExchanger.NameTransform(file.Name);

                            using (FileStream fs = new FileStream(outputDirectory.FullName + "\\" + arcname + "\\" + fileName, FileMode.Create))
                            using (Stream stream = file.GetRawStream())
                            {
                                stream.CopyTo(fs);
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(outputDirectory.FullName + "\\" + arcname + "\\" + file.Name, FileMode.Create))
                            using (Stream stream = (file as ArchiveSubfile).RawSource.GetRawStream())
                            {
                                stream.CopyTo(fs);
                            }
                        }
                    }
                }
            }
        }
        #endregion



    }
}
