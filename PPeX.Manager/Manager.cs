using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PPeX.Manager
{
    /// <summary>
    /// Methods used to transfer and decompress data for use in AA2.
    /// </summary>
    public static class Manager
    {
        public static Dictionary<FileEntry, ISubfile> FileCache = new Dictionary<FileEntry, ISubfile>();
        public static PipeClient Client;

        static Manager()
        {
            string dllsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x86");

            //Start the 64bit process
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(dllsPath, "PPeXM64.exe"));
            p.Start();
            
            //Connect to it
            Client = new PipeClient("PPEX");
            var connection = Client.CreateConnection();

            //Wait until it's ready
            string ready = "False";
            while (ready != "True")
            {
                System.Threading.Thread.Sleep(500);
                connection.WriteString("ready");
                connection.WriteString("");
                ready = connection.ReadString();
            }
        }

        /// <summary>
        /// Appends a message to the log.
        /// </summary>
        /// <param name="message">The message to append.</param>
        public static void AppendLog(string message)
        {
            using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                fs.Position = fs.Length;
                writer.WriteLine(message);
            }
        }

        /// <summary>
        /// Returns the size of the buffer needed to decompress a subfile.
        /// </summary>
        /// <param name="paramArchive">The archive that the subfile belongs to.</param>
        /// <param name="paramFile">The name of the subfile.</param>
        /// <returns></returns>
        public static uint PreAlloc(string paramArchive, string paramFile)
        {
            string filename = Path.GetFileNameWithoutExtension(paramArchive);

            ISubfile value;
            if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
            {
                //Already have it cached
                return value.Size;
            }
            else
            {
                //Need to request it
                var connection = Client.CreateConnection();
                connection.WriteString("preload");
                connection.WriteString(filename.ToLower() + "/" + paramFile.ToLower());

                string address = connection.ReadString();
                if (address == "NotAvailable")
                {
                    //We don't have it
                    AppendLog("N/A " + filename + ": " + paramFile);

                    return 0;
                }

                //Ask for metadata over the pipe
                uint compressedSize = uint.Parse(connection.ReadString());
                uint decompressedSize = uint.Parse(connection.ReadString());
                ArchiveFileCompression compression = (ArchiveFileCompression)Enum.Parse(typeof(ArchiveFileCompression), connection.ReadString());
                ArchiveFileType type = (ArchiveFileType)Enum.Parse(typeof(ArchiveFileType), connection.ReadString());

                //Index it
                FileCache.Add(new FileEntry(filename.ToLower(), paramFile.ToLower()),
                        SubfileFactory.Create(
                            new PipedFileSource(address, compressedSize, decompressedSize, compression, type), 
                            type));

                using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    fs.Position = fs.Length;
                    writer.WriteLine("PREALLOC " + filename + ": " + paramFile);
                }

                return FileCache[new FileEntry(filename.ToLower(), paramFile.ToLower())].Size;
            }
        }

        public static object lockObject = new object();

        /// <summary>
        /// Decompresses the file into the specified buffer.
        /// </summary>
        /// <param name="paramArchive">The archive that the subfile belongs to.</param>
        /// <param name="paramFile">The name of the subfile.</param>
        /// <param name="outBuffer">The buffer to copy the decompressed data to.</param>
        public unsafe static void Decompress(string paramArchive, string paramFile, byte* outBuffer)
        {
            lock (lockObject)
            {
                ISubfile value;

                string filename = Path.GetFileNameWithoutExtension(paramArchive);

                if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
                {
                    AppendLog("DECOMP " + filename + ": " + paramFile);

                    using (UnmanagedMemoryStream pt = new UnmanagedMemoryStream(outBuffer, 0, value.Size, FileAccess.Write))
                    {
                        //Decompress the file to the specified buffer
                        value.WriteToStream(pt);
                    }
                }
                else
                {
                    //We don't have the file cached anymore
                    //This means something cleared it between the prealloc and this method which should never happen
                    AppendLog("DEALLOCATED " + filename + ": " + paramFile);
                }
            }
            
        }
    }

    /// <summary>
    /// A file entry that is used for indexing subfiles.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Archive}: {File}")]
    public class FileEntry
    {
        /// <summary>
        /// The archive that the subfile belongs to.
        /// </summary>
        public string Archive;
        /// <summary>
        /// The name of the subfile.
        /// </summary>
        public string File;

        public FileEntry(string Archive, string File)
        {
            this.Archive = Archive.ToLower();
            this.File = File.ToLower();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FileEntry))
                return false;
            var other = obj as FileEntry;
            return (Archive == other.Archive) && (File == other.File);
        }

        public override int GetHashCode()
        {
            return (Archive + File).GetHashCode();
        }
    }
}
