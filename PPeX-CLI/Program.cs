using System;
using System.Linq;
using System.Text.RegularExpressions;
using PPeX;
using System.IO;
using PPeX.External.PP;
using PPeX.Encoders;
using System.Collections.Generic;

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

        static Version GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        static string VersionToReadableString(Version version)
        {
            string text = version.ToString().TrimEnd(".0".ToCharArray());
            if (text.Count(x => x == '.') < 2)
                text += ".0";
            return text;
        }

        private static ConsoleColor BaseColor => ConsoleColor.Gray;
        private static ConsoleColor AccentColor => ConsoleColor.Green;

        static void WriteLineAlternating(params string[] args)
        {
            bool isColored = false;
            foreach (string text in args)
            {
                if (isColored)
                    Console.ForegroundColor = AccentColor;
                else
                    Console.ForegroundColor = BaseColor;

                isColored = !isColored;

                Console.Write(text);
            }

            Console.ForegroundColor = BaseColor;
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            WriteLineAlternating("PPeX-CLI ", VersionToReadableString(GetVersion()));
            WriteLineAlternating("PPeX base ", VersionToReadableString(PPeX.Core.GetVersion()));

            Console.WriteLine($"Compiled codecs: {string.Join(", ", Enum.GetNames(typeof(ArchiveFileType)))}");


            Console.WriteLine();

            if (args.Length < 1 || args[0].ToLower() == "-h")
            {
                Console.WriteLine($@"Usage:

ppex-cli -h
Shows this help dialog

ppex-cli -c [options] [folder1 file2.pp wildcard3*.pp ...] output.ppx
Compresses a .ppx archive

ppex-cli -e [options] input.ppx output-directory/
Extracts a .ppx archive

ppex-cli -a [options] [folder1 file2.pp wildcard3*.pp ...] appended.ppx
Appends files to an already existing archive

ppex-cli -d fragmented.ppx
Defragments an archive


[Options]
(defaults listed as well)

-name ""Display Name""
(-c, -a only)
Sets the display name of the archive. Default is the output filename

-compression zstd
(-c, -a only)
Sets the compression method. Can be either 'zstd', 'lz4', or 'uncompressed'

-zstd-level 22
(-c, -a only)
Sets the Zstandard level of compression. Can be between 1 and 22

-chunksize 16M
(-c, -a only)
Sets chunk size of compressed data

-opus-music 44k
(-c, -a only)
Sets music (2 channel audio) bitrate

-opus-voice 32k
(-c, -a only)
Sets voice (1 channel audio) bitrate

-xx2-precision 0
(-c, -a only)
Sets .xx2 bit precision. Set to 0 for lossless mode. Cannot be used with -xx2-quality.

-xx2-quality
(-c, -a only)
Sets .xx2 encode quality. Not enabled by default. Cannot be used with -xx2-precision.

-threads 1
(-c, -a only)
Sets threads to be used during compression.

-convert [input]:[output]
(-c, -a only)
Adds a rule to convert from one format to another. Leave [output] empty to disable transcoding for the input format.
Current defaults:
{
    string.Join("\r\n",
        Core.Settings.DefaultEncodingConversions.Select(x => $"{Enum.GetName(typeof(ArchiveFileType), x.Key)}:{Enum.GetName(typeof(ArchiveFileType), x.Value)}"))
}

-no-encode ""a^""
(-c, -a only)
Defines which files should be left unencoded as a regex. Default is none.

-decode on
(-e only)
Sets rules for decoding any encoded files on extraction, such as .xx3 to .xx.
Default is on, available options are 'on' and 'off'.

-regex "".+""
Sets a regex to use for compressing or extracting.");

#if DEBUG
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
#endif

                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args[0].ToLower() == "-c")
            {
                CompressArg(args);
            }
            else if (args[0].ToLower() == "-e")
            {
                DecompressArg(args);
            }
            else
            {
                Console.WriteLine("No valid switch passed!");
            }

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
#endif
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

            Environment.Exit(9999);
        }

        private static void HaltAndCatchFire(string message, int exitCode)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.WriteLine($"Stopping archival");
            Environment.Exit(1);
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
            Dictionary<ArchiveFileType, ArchiveFileType> encodingTransforms = Core.Settings.DefaultEncodingConversions;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                Core.Settings.OpusFrameSize = 0.060;

            while ((currentArg = args[argcounter++]).StartsWith("-"))
            {
                currentArg = currentArg.ToLower();

                if (currentArg == "-chunksize")
                {
                    chunksize = ConvertFromReadable(args[argcounter++]);
                }
                else if (currentArg == "-opus-music")
                {
                    Core.Settings.OpusMusicBitrate = ConvertFromReadable(args[argcounter++]);
                }
                else if (currentArg == "-opus-voice")
                {
                    Core.Settings.OpusVoiceBitrate = ConvertFromReadable(args[argcounter++]);
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
                else if (currentArg == "-convert")
                {
                    string[] codecs = args[argcounter++].Split(':');

                    if (codecs.Length != 2)
                        HaltAndCatchFire($"Invalid convert argument: {args[argcounter]}", 100);

                    ArchiveFileType input;

                    if (!Enum.TryParse(codecs[0], out input))
                        HaltAndCatchFire($"Invalid input codec for conversion: {codecs[0]}", 101);

                    if (string.IsNullOrWhiteSpace(codecs[1]))
                    {
                        if (encodingTransforms.ContainsKey(input))
                            encodingTransforms.Remove(input);
                    }
                    else
                    {
                        ArchiveFileType output;

                        if (!Enum.TryParse(codecs[1], out output))
                            HaltAndCatchFire($"Invalid output codec for conversion: {codecs[1]}", 102);

                        encodingTransforms[input] = output;
                    }
                }
                else
                {
                    HaltAndCatchFire($"Unknown command: \"{currentArg}\"", 1);
                    return;
                }
            }

            compression = compression.ToLower();

            string filename = args.Last();

            ExtendedArchiveWriter writer = new ExtendedArchiveWriter(name);
            
            writer.ChunkSizeLimit = (ulong)chunksize;
            writer.Threads = threads;

            writer.EncodingConversions.Clear();

            foreach (var kv in encodingTransforms)
                writer.EncodingConversions[kv.Key] = kv.Value;

            if (compression == "zstd")
                writer.DefaultCompression = ArchiveChunkCompression.Zstandard;
            else if (compression == "lz4")
                writer.DefaultCompression = ArchiveChunkCompression.LZ4;
            else if (compression == "uncompressed")
                writer.DefaultCompression = ArchiveChunkCompression.Uncompressed;


            foreach (string path in args.Skip(argcounter - 1).Take(args.Length - argcounter))
            {
                string parentpath = Path.GetDirectoryName(path);
                string localpath = Path.GetFileName(path);

                foreach (string filepath in Directory.EnumerateFiles(parentpath, localpath, SearchOption.TopDirectoryOnly))
                {
                    if (filepath.EndsWith(".pp"))
                    {
                        //.pp file
                        Console.WriteLine("Importing " + Path.GetFileName(filepath));

                        ImportPP(filepath, writer.Files, regex, unencodedRegex);
                    }
                }

                foreach (string dirpath in Directory.EnumerateDirectories(parentpath, localpath, SearchOption.TopDirectoryOnly))
                {
                    name = Path.GetFileNameWithoutExtension(dirpath) + ".pp";

                    Console.WriteLine("Importing \"" + dirpath + "\" as \"" + name + "\"");

                    int imported = 0;
                    var files = Directory.EnumerateFiles(dirpath, "*.*", SearchOption.TopDirectoryOnly).ToArray();

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

            using (FileStream fs = new FileStream(filename, FileMode.Create))
                writer.Write(fs, progressStatus, progressPercentage);

            //wait for progress to update
            while (lastProgress != 100)
                System.Threading.Thread.Sleep(50);

            lock (progressPercentage)
            {
                isUpdating = false;
                Console.CursorVisible = true;
            }
        }

        static void ImportPP(string filename, IList<ISubfile> FilesAdding, Regex regex, Regex unencodedRegex)
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
                        FilesAdding.Add(
                        new PPeX.Subfile(
                            new PPSource(file),
                            file.Name,
                            name,
                            ArchiveFileType.Raw));
                    }
                    else
                    {
                        FilesAdding.Add(
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

                    if (regex.IsMatch(fullname))
                    {
                        Console.WriteLine("Exporting " + fullname);

                        if (!Directory.Exists(Path.Combine(outputDirectory.FullName, arcname)))
                            Directory.CreateDirectory(Path.Combine(outputDirectory.FullName + arcname));

                        using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, file.Name), FileMode.Create))
                        using (Stream subfileStream = file.GetStream())
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

                    if (regex.IsMatch(fullname))
                    {
                        Console.WriteLine("Exporting " + fullname);

                        if (!Directory.Exists(Path.Combine(outputDirectory.FullName, arcname)))
                            Directory.CreateDirectory(Path.Combine(outputDirectory.FullName, arcname));

                        if (decode == "full" && file.Type == ArchiveFileType.OpusAudio)
                        {
                            string fileName = file.Name.Replace(".opus", ".wav");

                            using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, fileName), FileMode.Create))
                            using (Stream stream = file.GetRawStream())
                            {
                                stream.CopyTo(fs);
                            }
                        }
                        else if (file.Type != ArchiveFileType.OpusAudio)
                        {
                            string fileName;
                            
                            fileName = EncoderFactory.TransformName(file.Name, file.Type);

                            using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, fileName), FileMode.Create))
                            using (Stream stream = file.GetRawStream())
                            {
                                stream.CopyTo(fs);
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, file.Name), FileMode.Create))
                            using (Stream stream = (file as ArchiveSubfile).RawSource.GetStream())
                            {
                                stream.CopyTo(fs);
                            }
                        }
                    }
                }
            }
        }
        #endregion

#region Append
        static void AppendArg(string[] args)
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
                Core.Settings.OpusFrameSize = 0.060;

            while ((currentArg = args[argcounter++]).StartsWith("-"))
            {
                currentArg = currentArg.ToLower();

                if (currentArg == "-chunksize")
                {
                    chunksize = ConvertFromReadable(args[argcounter++]);
                }
                else if (currentArg == "-opus-music")
                {
                    Core.Settings.OpusMusicBitrate = ConvertFromReadable(args[argcounter++]);
                }
                else if (currentArg == "-opus-voice")
                {
                    Core.Settings.OpusVoiceBitrate = ConvertFromReadable(args[argcounter++]);
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

            string filename = args.Last();

            ExtendedArchiveAppender appender = new ExtendedArchiveAppender(filename);

            appender.ChunkSizeLimit = (ulong)chunksize;
            appender.Threads = threads;
            appender.Name = name;

            if (compression == "zstd")
                appender.DefaultCompression = ArchiveChunkCompression.Zstandard;
            else if (compression == "lz4")
                appender.DefaultCompression = ArchiveChunkCompression.LZ4;
            else if (compression == "uncompressed")
                appender.DefaultCompression = ArchiveChunkCompression.Uncompressed;


            foreach (string path in args.Skip(argcounter - 1).Take(args.Length - argcounter))
            {
                string parentpath = Path.GetDirectoryName(path);
                string localpath = Path.GetFileName(path);

                foreach (string filepath in Directory.EnumerateFiles(parentpath, localpath, SearchOption.TopDirectoryOnly))
                {
                    if (filepath.EndsWith(".pp"))
                    {
                        //.pp file
                        Console.WriteLine("Importing " + Path.GetFileName(filepath));

                        ImportPP(filepath, appender.FilesToAdd, regex, unencodedRegex);
                    }
                }

                foreach (string dirpath in Directory.EnumerateDirectories(parentpath, localpath, SearchOption.TopDirectoryOnly))
                {
                    name = Path.GetFileNameWithoutExtension(dirpath) + ".pp";

                    Console.WriteLine("Importing \"" + dirpath + "\" as \"" + name + "\"");

                    int imported = 0;
                    var files = Directory.EnumerateFiles(dirpath, "*.*", SearchOption.TopDirectoryOnly).ToArray();

                    foreach (string file in files)
                    {
                        string fullName = name + "/" + Path.GetFileName(file);

                        if (regex.IsMatch(fullName))
                        {
                            if (unencodedRegex.IsMatch(fullName))
                            {
                                appender.Files.Add(
                                    new PPeX.Subfile(
                                        new FileSource(file),
                                        Path.GetFileName(file),
                                        name,
                                        ArchiveFileType.Raw));
                            }
                            else
                            {
                                appender.Files.Add(
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
            
            appender.Write(progressStatus, progressPercentage);

            //wait for progress to update
            while (lastProgress != 100)
                System.Threading.Thread.Sleep(50);

            lock (progressPercentage)
            {
                isUpdating = false;
                Console.CursorVisible = true;
            }
        }
        #endregion

        #region Defragment
        static void DefragmentArgs(string[] args)
        {
            string filename = args[0];

            ExtendedArchiveAppender appender = new ExtendedArchiveAppender(filename);

            appender.Defragment();

            Console.WriteLine($"\"{appender.Name}\" defragmented.");
        }
        #endregion
    }
}
