using LZ4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public static class Manager
    {
        public static Dictionary<FileEntry, ISubfile> FileCache = new Dictionary<FileEntry, ISubfile>();
        //public static List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();
        public static PipeClient Client; // = new PipeClient("PPEX");

        static Manager()
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            string dllsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x86");//Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            var assemblies = new List<Assembly>();

            foreach (string path in new DirectoryInfo(dllsPath).GetFiles("*.dll").Select(x => x.FullName))
            {
                try
                {
                    assemblies.Add(Assembly.LoadFile(path));
                }
                catch (Exception ex)
                {
                    
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return assemblies.First(x => args.Name == x.FullName);
            };

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(dllsPath, "PPeXM64.exe"));
            p.Start();

            //System.Threading.Thread.Sleep(1000);

            Client = new PipeClient("PPEX");
            var connection = Client.CreateConnection();

            string ready = "False";
            while (ready != "True")
            {
                System.Threading.Thread.Sleep(500);
                connection.WriteString("ready");
                connection.WriteString("");
                ready = connection.ReadString();
            }
            

            /*
            foreach (string dir in Directory.EnumerateFiles(Core.Settings.PPXLocation, "*.ppx", SearchOption.TopDirectoryOnly).OrderBy(x => x))
            {
                var archive = new ExtendedArchive(dir);

                foreach (var file in archive.ArchiveFiles)
                {
                    ISubfile subfile = SubfileFactory.Create(file, file.ArchiveName);
                    FileCache[new FileEntry(subfile.ArchiveName.Replace(".pp", "").ToLower(), subfile.Name.ToLower())] = subfile;
                }

                using (FileStream fs = new FileStream(dir, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] buffer = new byte[4096];
                    while (0 < br.Read(buffer, 0, 4096))
                    { }
                }

                LoadedArchives.Add(archive);
            }

            
            Dictionary<string, List<string>> ppf = new Dictionary<string, List<string>>();
            foreach (var kv in FileCache)
            {
                if (!ppf.ContainsKey(kv.Key.Archive))
                {
                    ppf.Add(kv.Key.Archive, new List<string>());
                }
                ppf[kv.Key.Archive].Add(kv.Key.File);
            }

            foreach (var arc in ppf)
            {
                File.WriteAllBytes(Core.Settings.PlaceholdersLocation + "\\" + arc.Key + ".pp", Utility.CreateHeader(arc.Value));
            }
            */
        }


        public static uint PreAlloc(string paramArchive, string paramFile)
        {
            string filename = Path.GetFileNameWithoutExtension(paramArchive);

            ISubfile value;
            if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
            {
                return value.Size;
            }
            else
            {
                var connection = Client.CreateConnection();
                //using ()
                //{
                    connection.WriteString("preload");
                    connection.WriteString(filename.ToLower() + "/" + paramFile.ToLower());

                    string address = connection.ReadString();
                    if (address == "NotAvailable")
                    {
                        /*FileCache.Add(new FileEntry(filename.ToLower(), paramFile.ToLower()),
                            SubfileFactory.Create(
                                new PipedFileSource(null, 0, 0, ArchiveFileCompression.Uncompressed, ArchiveFileType.Raw)));*/

                        using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            fs.Position = fs.Length;
                            writer.WriteLine("N/A " + filename + ": " + paramFile);
                        }

                        return 0;
                    }

                    uint compressedSize = uint.Parse(connection.ReadString());
                    uint decompressedSize = uint.Parse(connection.ReadString());
                    ArchiveFileCompression compression = (ArchiveFileCompression)Enum.Parse(typeof(ArchiveFileCompression), connection.ReadString());
                    ArchiveFileType type = (ArchiveFileType)Enum.Parse(typeof(ArchiveFileType), connection.ReadString());

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
                //}
            }
        }

        public static object lockObject = new object();

        public unsafe static void Decompress(string paramArchive, string paramFile, byte* outBuffer) //IntPtr outBuffer
        {
            lock (lockObject)
            {
                ISubfile value;

                string filename = Path.GetFileNameWithoutExtension(paramArchive);

                if (FileCache.TryGetValue(new FileEntry(filename.ToLower(), paramFile.ToLower()), out value))
                {
                    using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        fs.Position = fs.Length;
                        writer.Write("DECOMP " + filename + ": " + paramFile);
                    }

                    using (UnmanagedMemoryStream pt = new UnmanagedMemoryStream(outBuffer, 0, value.Size, FileAccess.Write))
                    {
                        value.WriteToStream(pt);
                    }

                    using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        fs.Position = fs.Length;
                        writer.WriteLine(" || SUCCESS");
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream("ppex.log", FileMode.OpenOrCreate))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        fs.Position = fs.Length;
                        writer.WriteLine("DEALLOCATED " + filename + ": " + paramFile);
                    }
                }
            }
            
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Archive}: {File}")]
    public class FileEntry
    {
        public string Archive;
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

    public class PipedFileSource : IDataSource
    {
        public string Address;
        protected uint CompressedSize;
        public uint DecompressedSize;
        public ArchiveFileCompression Compression;
        public ArchiveFileType Type;

        public uint Size => DecompressedSize;

        public byte[] Md5
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PipedFileSource(string address, uint compressedSize, uint size, ArchiveFileCompression compression, ArchiveFileType type)
        {
            Address = address;
            DecompressedSize = size;
            CompressedSize = compressedSize;
            Compression = compression;
            Type = type;
        }

        static object lockObject = new object();

        public Stream GetStream()
        {
            Stream stream;
            lock (lockObject)
            {
                var handler = Manager.Client.CreateConnection();
                handler.WriteString("load");
                handler.WriteString(Address);

                //MemoryMappedFile MMFile = MemoryMappedFile.OpenExisting(Address, MemoryMappedFileRights.Read);
                //Stream stream = MMFile.CreateViewStream(0, CompressedSize, MemoryMappedFileAccess.Read);
                using (BinaryReader reader = new BinaryReader(handler.BaseStream, Encoding.Unicode, true))
                {
                    int length = reader.ReadInt32();
                    stream = new MemoryStream(
                        reader.ReadBytes(length), 
                        false);
                }
            }


            switch (Compression)
            {
                case ArchiveFileCompression.LZ4:
                    return new LZ4Stream(stream,
                        LZ4StreamMode.Decompress);
                case ArchiveFileCompression.Zstandard:
                    byte[] output;
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        stream.CopyTo(buffer);
                        output = buffer.ToArray();
                    }
                    using (ZstdNet.Decompressor zstd = new ZstdNet.Decompressor())
                        return new MemoryStream(zstd.Unwrap(output), false); //, (int)_size
                case ArchiveFileCompression.Uncompressed:
                    return stream;
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            };
        }
    }

    public class PipeClient
    {
        protected string Name;
        protected StreamHandler temp;

        public PipeClient(string name)
        {
            Name = name;
            NamedPipeClientStream client = new NamedPipeClientStream(Name);
            client.Connect();
            temp = new StreamHandler(client);
        }

        public StreamHandler CreateConnection()
        {
            return temp;
        }

        /*
        public delegate void PipeMessageRecievedHandler(int instance, string message);
        public event PipeMessageRecievedHandler PipeMessageRecieved;*/
    }

    public class StreamHandler : IDisposable
    {
        private Stream ioStream;
        public Stream BaseStream => ioStream;

        public StreamHandler(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

        public void Dispose()
        {
            ((IDisposable)ioStream).Dispose();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() << 8;
            len |= ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return Encoding.Unicode.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = Encoding.Unicode.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }
            ioStream.WriteByte((byte)(len >> 8));
            ioStream.WriteByte((byte)(len & 0xFF));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
