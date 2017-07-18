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

        public static object lockObject = new object();

        public delegate IntPtr AllocateDelegate(int size);

        /// <summary>
        /// Decompresses the file into the specified buffer.
        /// </summary>
        /// <param name="paramArchive">The archive that the subfile belongs to.</param>
        /// <param name="paramFile">The name of the subfile.</param>
        /// <param name="alloc">The memory allocation method to use.</param>
        /// <param name="outBuffer">The allocated buffer that contains the decompressed data.</param>
        public unsafe static bool Decompress(string paramArchive, string paramFile, AllocateDelegate alloc, byte* outBuffer)
        {
            lock (lockObject)
            {
                string filename = Path.GetFileNameWithoutExtension(paramArchive);

                //Need to request it
                var connection = Client.CreateConnection();
                connection.WriteString("load");
                connection.WriteString(filename.ToLower() + "/" + paramFile.ToLower());

                string address = connection.ReadString();
                if (address == "NotAvailable")
                {
                    //We don't have it
                    AppendLog("N/A " + filename + ": " + paramFile);

                    return false;
                }

                int size = int.Parse(address);

                outBuffer = (byte*)alloc(size);

                AppendLog("DECOMP " + filename + ": " + paramFile);

                

                using (UnmanagedMemoryStream pt = new UnmanagedMemoryStream(outBuffer, 0, size, FileAccess.Write))
                {
                    //Decompress the file to the specified buffer

                    //value.WriteToStream(pt);
                    byte[] buffer = new byte[size];
                    connection.BaseStream.Read(buffer, 0, size);
                    pt.Write(buffer, 0, size);
                }
            }

            return true;
            
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
