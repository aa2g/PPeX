using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using Crc32C;
using PPeX.Encoders;

namespace PPeX
{
    /// <summary>
    /// A writer for extended archives, to .ppx files.
    /// </summary>
    public class ExtendedArchiveWriter
    {
        /*
        0 - magic PPEX
        4 - version [ushort]
        6 - archive type [short]
        8 - archive name length in bytes [ushort]
        10 - archive name [unicode]
        10 + n - number of subfiles [uint]
        14 + n - header length [uint]
        18 + n - header
        */

        /// <summary>
        /// The display name of the archive.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of files that the archive contains.
        /// </summary>
        public List<ArchiveFile> Files = new List<ArchiveFile>();
        /// <summary>
        /// The stream that the archive will be written to.
        /// </summary>
        public Stream ArchiveStream { get; protected set; }
        /// <summary>
        /// The type of archive that will be created.
        /// </summary>
        public ArchiveType Type { get; set; }
        /// <summary>
        /// The compression type that the writer will default to.
        /// </summary>
        public ArchiveFileCompression DefaultCompression { get; set; }

        bool leaveOpen;

        /// <summary>
        /// Creates a new extended archive writer.
        /// </summary>
        /// <param name="File">The stream of the .ppx file to be written to.</param>
        /// <param name="Name">The display name for the archive.</param>
        public ExtendedArchiveWriter(Stream File, string Name, bool LeaveOpen = false)
        {
            this.ArchiveStream = File;
            this.Name = Name;
            Type = ArchiveType.Archive;
            DefaultCompression = ArchiveFileCompression.LZ4;

            leaveOpen = LeaveOpen;
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        public void Write()
        {
            IProgress<Tuple<string, int>> progress = new Progress<Tuple<string, int>>();

            Write(progress);
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        /// <param name="progress">The progress callback object.</param>
        public void Write(IProgress<Tuple<string, int>> progress)
        {
            List<WriteReciept> receipts = new List<WriteReciept>();
            
            using (MemoryStream header = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ArchiveStream, Encoding.ASCII, leaveOpen))
            using (BinaryWriter headerWriter = new BinaryWriter(header))
            {
                //Write container header data
                writer.Write(Encoding.ASCII.GetBytes(ExtendedArchive.Magic));

                writer.Write(ExtendedArchive.Version);
                writer.Write((ushort)1);

                byte[] title = Encoding.Unicode.GetBytes(Name);
                writer.Write((ushort)title.Length);
                writer.Write(title);

                //Write individual file header + data
                writer.BaseStream.Position = 1024;

                writer.Write((uint)Files.Count);
                int headerLength = Files.Sum(x => x.HeaderLength);

                writer.Write((uint)headerLength);

                long headerPos = writer.BaseStream.Position;
                writer.BaseStream.Position += headerLength + (512 * 1024); //512kb empty space

                int i = 0;
                foreach (var file in Files)
                {
                    //Reduce the MD5 to a single int
                    i++;

                    var WriteReciept = receipts.FirstOrDefault(x => Utility.CompareBytes(x.Md5, file.Subfile.Source.Md5));
                    bool isDuplicate = WriteReciept != null;

                    try
                    {
                        //Write the file
                        WriteReciept = file.WriteTo(writer, headerWriter, WriteReciept);
                    }
                    catch
                    {
                        //Cancel the write process on error
                        progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + Files.Count + "] Stopped writing " + file.Name + "\r\n",
                                    100 * i / Files.Count));

                        throw;
                    }

                    //Update progress
                    progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + Files.Count + "] Written " + file.Name + "... (" + file.Subfile.Source.Size + " bytes)" + (isDuplicate ? " [duplicate]\r\n" : "\r\n"),
                                    100 * i / Files.Count));

                    receipts.Add(WriteReciept);
                }

                //Go back and write the file header
                writer.BaseStream.Position = headerPos;
                writer.Write(header.ToArray());
            }

            progress.Report(new Tuple<string, int>("Finished.\n", 100));
        }
    }

    /// <summary>
    /// A subfile that is to be written to an archive.
    /// </summary>
    public class ArchiveFile
    {
        /*
        0 - File type [byte]
        1 - File flags [byte]
        2 - Compression type [byte]
        3 - File priority [byte]
        4 - CRC32C of packed data [uint]
        8 - MD5 of uncompressed file {16 bytes long}
        24 - Metadata related to encoding {48 bytes long}
        72 - File name in bytes [ushort]
        74 - File name [unicode]
        74 + name - File offset [ulong]
        82 - File uncompressed length [uint]
        86 - File compressed length [uint]
        */

        /// <summary>
        /// The data source of the file.
        /// </summary>
        public ISubfile Subfile;

        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The size of the header of the file.
        /// </summary>
        internal int HeaderLength
        {
            get
            {
                return 90 + System.Text.Encoding.Unicode.GetByteCount(Name);
            }
        }

        /// <summary>
        /// The type of the file.
        /// </summary>
        public ArchiveFileEncoding Encoding = ArchiveFileEncoding.Raw;
        /// <summary>
        /// The metadata flags associated with the subfile.
        /// </summary>
        public ArchiveFileFlags Flags = ArchiveFileFlags.None;
        /// <summary>
        /// The compression that will be used on the subfile.
        /// </summary>
        public ArchiveFileCompression Compression = ArchiveFileCompression.Uncompressed;
        /// <summary>
        /// The memory priority of the subfile.
        /// </summary>
        public byte Priority;

        protected bool IsEncoding = true;
        protected bool IsCopying = true;

        /// <summary>
        /// Creates a subfile to be written.
        /// </summary>
        /// <param name="Source">The source of the data to be written.</param>
        /// <param name="Name">The name of the subfile.</param>
        /// <param name="Compression">The compression to use on the subfile.</param>
        /// <param name="Priority">The memory priority of the subfile.</param>
        public ArchiveFile(ISubfile Subfile, ArchiveFileCompression Compression, byte Priority)
        {
            this.Subfile = Subfile;
            Name = Subfile.ArchiveName + "/" + Subfile.Name;

            this.Compression = Compression;
            this.Priority = Priority;

            //Determine type and compression
            if (Name.EndsWith(".wav"))
            {
                Encoding = ArchiveFileEncoding.XggAudio;
                Compression = ArchiveFileCompression.Uncompressed;
                Name = Name.Replace(".wav", ".xgg");
            }
            else if (Name.EndsWith(".xgg"))
            {
                Encoding = ArchiveFileEncoding.XggAudio;
                Compression = ArchiveFileCompression.Uncompressed;
                IsEncoding = false;
            }
            else
            {
                Encoding = ArchiveFileEncoding.Raw;
            }

            if (Subfile is IsolatedSubfile &&
                Compression == (Subfile.Source as ArchiveFileSource).Compression)
            {
                IsCopying = true;
            }
        }

        /// <summary>
        /// Write the subfile to the archive writer.
        /// </summary>
        /// <param name="dataWriter">The archive writer to write the subfile data to.</param>
        /// <param name="metadataWriter">The archive writer to write the header data to.</param>
        /// <param name="reciept">The write receipt to use if the file is a duplicate. Null if not a duplicate.</param>
        public WriteReciept WriteTo(BinaryWriter dataWriter, BinaryWriter metadataWriter, WriteReciept reciept = null)
        {
            uint compressedSize = 0;
            uint crc = 0;
            ulong offset = (ulong)dataWriter.BaseStream.Position;

            bool isDupe = reciept != null;

            if (!isDupe)
            {
                if (IsCopying)
                {
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        Subfile.WriteToStream(buffer);
                        compressedSize = Subfile.Size;

                        //Write the data and get a crc
                        byte[] bBuffer = buffer.ToArray();
                        crc = Crc32CAlgorithm.Compute(bBuffer);
                        buffer.Position = 0;
                        dataWriter.Write(bBuffer);
                    }
                }
                else
                {
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        IEncoder encoder;

                        if (IsEncoding)
                            encoder = EncoderFactory.GetEncoder(Subfile.Source, Encoding);
                        else
                            encoder = new PassthroughEncoder(Subfile.Source.GetStream());

                        //Compress and encode the data
                        using (encoder)
                        using (ICompressor compressor = CompressorFactory.GetCompressor(encoder.Encode(), Compression))
                        {
                            compressor.WriteToStream(buffer);
                            compressedSize = compressor.CompressedSize;
                        }
                        
                        //Write the data and get a crc
                        byte[] bBuffer = buffer.ToArray();
                        crc = Crc32CAlgorithm.Compute(bBuffer);
                        buffer.Position = 0;
                        dataWriter.Write(bBuffer);
                    }
                }
            }
            else
            {
                offset = reciept.offset;
                compressedSize = reciept.length;
                crc = reciept.crc;
                Compression = reciept.compression;
            }

            //Write the header data
            metadataWriter.Write((byte)Encoding);
            metadataWriter.Write((byte)Flags);
            
            metadataWriter.Write((byte)Compression);

            metadataWriter.Write(Priority);

            metadataWriter.Write(crc);

            metadataWriter.Write(Subfile.Source.Md5);

            metadataWriter.BaseStream.Seek(48, SeekOrigin.Current);

            byte[] uName = System.Text.Encoding.Unicode.GetBytes(Name);
            metadataWriter.Write((ushort)uName.Length);
            metadataWriter.Write(uName);

            metadataWriter.Write(offset);
            metadataWriter.Write(Subfile.Source.Size);
            metadataWriter.Write(compressedSize);

            return new WriteReciept
            {
                Md5 = Subfile.Source.Md5,
                offset = offset,
                length = compressedSize,
                crc = crc,
                compression = Compression
            };
        }
    }

    public class WriteReciept
    {
        public byte[] Md5;
        public ulong offset;
        public uint length;
        public uint crc;
        public ArchiveFileCompression compression;
    }
}
