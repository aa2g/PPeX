using System;
using System.Buffers;
using System.Linq;
using System.Text.RegularExpressions;
using PPeX;
using System.IO;
using PPeX.External.PP;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPeX.External.CRC32;
using Utility = PPeX.Utility;

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

        static string ConvertToReadable(long length)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = (int)Math.Floor(Math.Log(length, 1024));
            double len = length / Math.Pow(1024, order);
            
            return String.Format("{0:0.##} {1}", len, sizes[order]);
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

            Console.ResetColor();
            Console.WriteLine();
        }

        private static async Task Main(string[] args)
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

ppex-cli -v [options] archive.ppx
Verifies an archive


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
Sets a regex to use for compressing or extracting.

-fullverify off
(-v only)
Verifies individual file MD5 results, instead of just the CRC32.
Default is off, available options are 'on' and 'off'.");

                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args[0].ToLower() == "-c")
            {
                await CompressArg(args);
            }
            else if (args[0].ToLower() == "-e")
            {
                await DecompressArg(args);
            }
            else if (args[0].ToLower() == "-d")
            {
                DefragmentArgs(args);
            }
            else if (args[0].ToLower() == "-v")
            {
                VerifyArgs(args);
            }
            else
            {
                Console.WriteLine("No valid switch passed!");
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("-- UNHANDLED EXCEPTION --");
            Console.WriteLine(ex.ToString());

            Console.CursorVisible = true;
            Console.ResetColor();
            Environment.Exit(9999);
        }

        private static void HaltAndCatchFire(string message, int exitCode)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.WriteLine($"Stopping...");
            Console.CursorVisible = true;
            Console.ResetColor();
            Environment.Exit(1);
        }

        #region Compress
        static async Task CompressArg(string[] args)
        {
            Regex regex = new Regex(".+");
            Regex unencodedRegex = new Regex("a^");

            int argcounter = 1;
            int chunksize = ConvertFromReadable("16M");
            int threads = 2;
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
                if (path.EndsWith(".pp") && File.Exists(path))
                {
	                Console.WriteLine("Importing " + Path.GetFileName(path));

	                ImportPP(path, writer.Files, regex, unencodedRegex);

                    continue;
                }

                string parentPath = Path.GetDirectoryName(path);
                string localPath = Path.GetFileName(path);

                foreach (string filepath in Directory.EnumerateFiles(parentPath, localPath, SearchOption.TopDirectoryOnly))
                {
	                if (Path.GetFileName(filepath) == "base.pp")
		                continue;

                    if (filepath.EndsWith(".pp"))
                    {
                        //.pp file
                        Console.WriteLine("Importing " + Path.GetFileName(filepath));

                        ImportPP(filepath, writer.Files, regex, unencodedRegex);
                    }
                }

                foreach (string dirpath in Directory.EnumerateDirectories(parentPath, localPath, SearchOption.TopDirectoryOnly))
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

            await using (FileStream fs = new FileStream(filename, FileMode.Create))
                await writer.WriteAsync(fs, progressStatus, progressPercentage);

            //wait for progress to update
            while (lastProgress != 100)
                await Task.Delay(50);

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

            foreach (ppSubfile file in pp.Subfiles)
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
        static async Task DecompressArg(string[] args)
        {
            Regex regex = new Regex(".+");

            int argcounter = 1;
            string decode = "off";
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

            var chunks = archive.RawFiles
                .Where(x => regex.IsMatch($"{x.ArchiveName}/{x.Name}"))
                .GroupBy(x => x.ChunkID);

            foreach (var chunkListing in chunks)
            {
                var chunk = chunkListing.First().Chunk;

                using var decompressionBuffer = MemoryPool<byte>.Shared.Rent((int)chunk.UncompressedLength);

                chunk.CopyToMemory(decompressionBuffer.Memory);

                List<Task> tasks = new List<Task>();

                foreach (var file in chunkListing)
                {
	                string arcname = file.ArchiveName.Replace(".pp", "");

	                Console.WriteLine($"Exporting {file.ArchiveName}/{file.Name}");

	                if (!Directory.Exists(Path.Combine(outputDirectory.FullName, arcname)))
		                Directory.CreateDirectory(Path.Combine(outputDirectory.FullName, arcname));

                    tasks.Add(Task.Run(async () =>
                    {
                        await using FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, file.Name), FileMode.Create);

                        await fs.WriteAsync(decompressionBuffer.Memory.Slice((int)file.Offset, (int)file.Size));
                    }));
                }

                await Task.WhenAll(tasks);
            }

            //else if (decode == "on" || decode == "full")
            //{
            //    //decoding raw entries
            //    foreach (var file in archive.Files)
            //    {
            //        string fullname = file.ArchiveName + "/" + file.Name;
            //        string arcname = file.ArchiveName.Replace(".pp", "");

            //        if (regex.IsMatch(fullname))
            //        {
            //            Console.WriteLine("Exporting " + fullname);

            //            if (!Directory.Exists(Path.Combine(outputDirectory.FullName, arcname)))
            //                Directory.CreateDirectory(Path.Combine(outputDirectory.FullName, arcname));

            //            if (decode == "full" && file.Type == ArchiveFileType.OpusAudio)
            //            {
            //                string fileName = file.Name.Replace(".opus", ".wav");

            //                using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, fileName), FileMode.Create))
            //                using (Stream stream = file.GetRawStream())
            //                {
            //                    stream.CopyTo(fs);
            //                }
            //            }
            //            else if (file.Type != ArchiveFileType.OpusAudio)
            //            {
            //                string fileName;
                            
            //                fileName = EncoderFactory.TransformName(file.Name, file.Type);

            //                using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, fileName), FileMode.Create))
            //                using (Stream stream = file.GetRawStream())
            //                {
            //                    stream.CopyTo(fs);
            //                }
            //            }
            //            else
            //            {
            //                using (FileStream fs = new FileStream(Path.Combine(outputDirectory.FullName, arcname, file.Name), FileMode.Create))
            //                using (Stream stream = (file as ArchiveSubfile).RawSource.GetStream())
            //                {
            //                    stream.CopyTo(fs);
            //                }
            //            }
            //        }
            //    }
            //}
        }
        #endregion

        #region Append
        static void AppendArg(string[] args)
        {
            //Regex regex = new Regex(".+");
            //Regex unencodedRegex = new Regex("a^");

            //int argcounter = 1;
            //int chunksize = ConvertFromReadable("16M");
            //int threads = 1;
            //string name = Path.GetFileNameWithoutExtension(args.Last());
            //string compression = "zstd";
            //string currentArg;

            //if (Environment.OSVersion.Platform == PlatformID.Unix)
            //    Core.Settings.OpusFrameSize = 0.060;

            //while ((currentArg = args[argcounter++]).StartsWith("-"))
            //{
            //    currentArg = currentArg.ToLower();

            //    if (currentArg == "-chunksize")
            //    {
            //        chunksize = ConvertFromReadable(args[argcounter++]);
            //    }
            //    else if (currentArg == "-opus-music")
            //    {
            //        Core.Settings.OpusMusicBitrate = ConvertFromReadable(args[argcounter++]);
            //    }
            //    else if (currentArg == "-opus-voice")
            //    {
            //        Core.Settings.OpusVoiceBitrate = ConvertFromReadable(args[argcounter++]);
            //    }
            //    else if (currentArg == "-xx2-precision")
            //    {
            //        Core.Settings.Xx2Precision = int.Parse(args[argcounter++]);
            //        Core.Settings.Xx2IsUsingQuality = false;
            //    }
            //    else if (currentArg == "-xx2-quality")
            //    {
            //        Core.Settings.Xx2Quality = float.Parse(args[argcounter++]);
            //        Core.Settings.Xx2IsUsingQuality = false;
            //    }
            //    else if (currentArg == "-threads")
            //    {
            //        threads = int.Parse(args[argcounter++]);
            //    }
            //    else if (currentArg == "-compression")
            //    {
            //        compression = args[argcounter++].ToLower();
            //    }
            //    else if (currentArg == "-zstd-level")
            //    {
            //        Core.Settings.ZstdCompressionLevel = int.Parse(args[argcounter++]);
            //    }
            //    else if (currentArg == "-name")
            //    {
            //        name = args[argcounter++];
            //    }
            //    else if (currentArg == "-regex")
            //    {
            //        regex = new Regex(args[argcounter++]);
            //    }
            //    else if (currentArg == "-no-encode")
            //    {
            //        unencodedRegex = new Regex(args[argcounter++]);
            //    }
            //}

            //compression = compression.ToLower();

            //string filename = args.Last();

            //ExtendedArchiveAppender appender = new ExtendedArchiveAppender(filename);

            //appender.ChunkSizeLimit = (ulong)chunksize;
            //appender.Threads = threads;
            //appender.Name = name;

            //if (compression == "zstd")
            //    appender.DefaultCompression = ArchiveChunkCompression.Zstandard;
            //else if (compression == "lz4")
            //    appender.DefaultCompression = ArchiveChunkCompression.LZ4;
            //else if (compression == "uncompressed")
            //    appender.DefaultCompression = ArchiveChunkCompression.Uncompressed;


            //foreach (string path in args.Skip(argcounter - 1).Take(args.Length - argcounter))
            //{
            //    string parentpath = Path.GetDirectoryName(path);
            //    string localpath = Path.GetFileName(path);

            //    foreach (string filepath in Directory.EnumerateFiles(parentpath, localpath, SearchOption.TopDirectoryOnly))
            //    {
            //        if (filepath.EndsWith(".pp"))
            //        {
            //            //.pp file
            //            Console.WriteLine("Importing " + Path.GetFileName(filepath));

            //            ImportPP(filepath, appender.FilesToAdd, regex, unencodedRegex);
            //        }
            //    }

            //    foreach (string dirpath in Directory.EnumerateDirectories(parentpath, localpath, SearchOption.TopDirectoryOnly))
            //    {
            //        name = Path.GetFileNameWithoutExtension(dirpath) + ".pp";

            //        Console.WriteLine("Importing \"" + dirpath + "\" as \"" + name + "\"");

            //        int imported = 0;
            //        var files = Directory.EnumerateFiles(dirpath, "*.*", SearchOption.TopDirectoryOnly).ToArray();

            //        foreach (string file in files)
            //        {
            //            string fullName = name + "/" + Path.GetFileName(file);

            //            if (regex.IsMatch(fullName))
            //            {
            //                if (unencodedRegex.IsMatch(fullName))
            //                {
            //                    appender.Files.Add(
            //                        new PPeX.Subfile(
            //                            new FileSource(file),
            //                            Path.GetFileName(file),
            //                            name,
            //                            ArchiveFileType.Raw));
            //                }
            //                else
            //                {
            //                    appender.Files.Add(
            //                        new PPeX.Subfile(
            //                            new FileSource(file),
            //                            Path.GetFileName(file),
            //                            name));
            //                }


            //                imported++;
            //            }
            //        }

            //        Console.WriteLine("Imported " + imported + "/" + files.Length + " files");
            //    }
            //}

            //int lastProgress = 0;

            //object progressLock = new object();
            //bool isUpdating = true;

            //Console.CursorVisible = false;

            //Progress<string> progressStatus = new Progress<string>(x =>
            //{
            //    if (!isUpdating)
            //        return;

            //    lock (progressLock)
            //    {
            //        Console.SetCursorPosition(0, Console.CursorTop);

            //        for (int i = 0; i < Console.WindowWidth - 1; i++)
            //            Console.Write(" ");

            //        Console.SetCursorPosition(0, Console.CursorTop);

            //        Console.WriteLine(x.Trim());

            //        Console.Write("[" + lastProgress + "% complete]");
            //    }
            //});

            //Progress<int> progressPercentage = new Progress<int>(x =>
            //{
            //    if (!isUpdating)
            //        return;

            //    lock (progressLock)
            //    {
            //        lastProgress = x;

            //        Console.SetCursorPosition(0, Console.CursorTop);

            //        Console.Write("[" + lastProgress + "% complete]");
            //    }
            //});
            
            //appender.Write(progressStatus, progressPercentage);

            ////wait for progress to update
            //while (lastProgress != 100)
            //    System.Threading.Thread.Sleep(50);

            //lock (progressPercentage)
            //{
            //    isUpdating = false;
            //    Console.CursorVisible = true;
            //}
        }
        #endregion

        #region Defragment
        static void DefragmentArgs(string[] args)
        {
            if (args.Length < 2)
            {
                HaltAndCatchFire($"Invalid amount of arguments.", -1);
            }

            string filename = args[1];

            if (!File.Exists(filename))
            {
                HaltAndCatchFire($"Input file does not exist: {filename}", 1);
            }

            //ExtendedArchiveAppender appender = new ExtendedArchiveAppender(filename);

            //appender.Defragment();

            //Console.WriteLine($"\"{appender.Name}\" defragmented.");
        }
        #endregion

        #region Verify
        static void VerifyArgs(string[] args)
        {
	        string currentArg;
	        int argcounter = 1;

            bool fullVerify = true;

            while ((currentArg = args[argcounter++]).StartsWith("-"))
            {
                currentArg = currentArg.ToLower();

                if (currentArg == "-fullverify")
                {
	                fullVerify = args[argcounter++].ToLower() == "on";
                }
                else
                {
                    HaltAndCatchFire($"Unknown command: \"{currentArg}\"", 1);
                    return;
                }
            }

            string filename = args[--argcounter];

            if (!File.Exists(filename))
            {
                HaltAndCatchFire($"Input file does not exist: {filename}", 1);
            }

            ExtendedArchive arc;

            try
            {
                arc = new ExtendedArchive(filename);
            }
            catch (PpexException ex)
            {
                if (ex.ErrorCode == PpexException.PpexErrorCode.FileNotPPXArchive ||
                    ex.ErrorCode == PpexException.PpexErrorCode.IncorrectVersionNumber)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                    return;
                }
                else
                    throw;
            }
            

            Console.CursorVisible = false;

            Action<int, string> UpdateProgress = (i, x) =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);

                string message = $"Verification {i}% complete [{x}]";

                message = message.PadRight(Console.BufferWidth - 1);

                Console.Write(message);
            };

            var orderedChunks = arc.Chunks.OrderBy(x => x.Offset).ToArray();

            Console.WriteLine("Archive name: " + arc.Title);
            Console.WriteLine("File count: " + arc.Files.Count);

            long currentOffset = 0;
            long totalOffset = orderedChunks.Sum(x => (long)x.CompressedLength);

            HashSet<(Md5Hash, ulong, ulong)> verifiedHashes = new HashSet<(Md5Hash, ulong, ulong)>();

            for (int i = 0; i < orderedChunks.Length; i++)
            {
                ExtendedArchiveChunk chunk = orderedChunks[i];

                using var compressedBuffer = MemoryPool<byte>.Shared.Rent((int)chunk.CompressedLength);
                var compressedMemory = compressedBuffer.Memory.Slice(0, (int)chunk.CompressedLength);

                using (var rawStream = chunk.GetRawStream())
                {
	                rawStream.Read(compressedMemory.Span);
                }

                if (chunk.CRC32 != CRC32.Compute(compressedMemory.Span))
					HaltAndCatchFire($"Chunk hash mismatch; id: {chunk.ID}, offset: {chunk.Offset}", 8);


				if (fullVerify)
                {
	                using var uncompressedBuffer = MemoryPool<byte>.Shared.Rent((int)chunk.UncompressedLength);
	                var uncompressedMemory = uncompressedBuffer.Memory.Slice(0, (int)chunk.UncompressedLength);

	                chunk.CopyToMemory(uncompressedMemory);

                    foreach (var file in chunk.Files)
                    {
	                    var expectedHash = (file.RawSource.Md5, file.RawSource.Offset, file.RawSource.Size);

	                    if (verifiedHashes.Contains(expectedHash))
		                    continue;

                        var actualMd5 =
			                Utility.GetMd5(uncompressedMemory.Span.Slice((int)file.RawSource.Offset,
				                (int)file.RawSource.Size));

		                if ((Md5Hash)file.RawSource.Md5 != (Md5Hash)actualMd5)
		                {
			                HaltAndCatchFire($"File md5 mismatch; chunk id: {chunk.ID}, archive: {file.ArchiveName}, file: {file.Name}", 9);
                        }

		                verifiedHashes.Add((actualMd5, file.RawSource.Offset, file.RawSource.Size));
	                }

                    verifiedHashes.Clear();
                }


                currentOffset += (long)chunk.CompressedLength;

                //int percentage = (int)((float)(100 * i) / (float)orderedChunks.Length);
                int percentage = (int)Math.Round((float)(100 * currentOffset) / (float)totalOffset);

                UpdateProgress(percentage, $"{i + 1}/{orderedChunks.Length} : {ConvertToReadable(currentOffset)}/{ConvertToReadable(totalOffset)}");
            }

            Console.WriteLine();

            Console.WriteLine($"\"{arc.Title}\" successfully verified.");
            Console.CursorVisible = true;
        }
        #endregion
    }
}