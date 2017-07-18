using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LZ4;
using Crc32C;

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

                    var WriteReciept = receipts.FirstOrDefault(x => Utility.CompareBytes(x.Md5, file.Source.Md5));
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
                                    "[" + i + " / " + Files.Count + "] Written " + file.Name + "... (" + file.Source.Size + " bytes)" + (isDuplicate ? " [duplicate]\r\n" : "\r\n"),
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
        24 - [Reserved] {48 bytes long}
        72 - File name in bytes [ushort]
        74 - File name [unicode]
        74 + name - File offset [ulong]
        82 - File uncompressed length [uint]
        86 - File compressed length [uint]
        */

        /// <summary>
        /// The data source of the archive.
        /// </summary>
        public IDataSource Source;

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
                return 90 + Encoding.Unicode.GetByteCount(Name);
            }
        }

        /// <summary>
        /// The type of the file.
        /// </summary>
        public ArchiveFileType Type = ArchiveFileType.Raw;
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

        /// <summary>
        /// Creates a subfile to be written.
        /// </summary>
        /// <param name="Source">The source of the data to be written.</param>
        /// <param name="Name">The name of the subfile.</param>
        /// <param name="Compression">The compression to use on the subfile.</param>
        /// <param name="Priority">The memory priority of the subfile.</param>
        public ArchiveFile(IDataSource Source, string Name, ArchiveFileCompression Compression, byte Priority)
        {
            //Determine type and compression
            if (Name.EndsWith(".wav"))
            {
                //Wrap the source into an opus stream
                this.Source = new Xgg.XggWrappedSource(Source);
                this.Name = Name.Replace(".wav", ".xgg");
            }
            else
            {
                this.Source = Source;
                this.Name = Name;
            }


            if (this.Name.EndsWith(".xgg"))
                Type = ArchiveFileType.Audio;
            else if (this.Name.EndsWith(".png"))
                Type = ArchiveFileType.Image;
            else
                this.Compression = Compression;

            this.Priority = Priority;
        }

        /// <summary>
        /// Write the subfile to the archive writer.
        /// </summary>
        /// <param name="dataWriter">The archive writer to write the subfile data to.</param>
        /// <param name="metadataWriter">The archive writer to write the header data to.</param>
        /// <param name="reciept">The write receipt to use if the file is a duplicate. Null if not a duplicate.</param>
        public WriteReciept WriteTo(BinaryWriter dataWriter, BinaryWriter metadataWriter, WriteReciept reciept = null)
        {
            uint actualsize = 0;
            uint crc = 0;
            ulong offset = (ulong)dataWriter.BaseStream.Position;

            bool isDupe = reciept != null;

            if (!isDupe)
            {

                using (MemoryStream buffer = new MemoryStream()) 
                using (Stream source = Source.GetStream())
                {
                    //Compress the data
                    switch (Compression)
                    {
                        case ArchiveFileCompression.LZ4:
                            using (LZ4Stream lz = new LZ4Stream(buffer, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression | LZ4StreamFlags.IsolateInnerStream, 4 * 1048576))
                                source.CopyTo(lz);
                            break;
                        case ArchiveFileCompression.Zstandard:
                            using (ZstdNet.Compressor zstd = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(6)))
                            using (MemoryStream temp = new MemoryStream())
                            {
                                source.CopyTo(temp);
                                byte[] output = zstd.Wrap(temp.ToArray());
                                buffer.Write(output, 0, output.Length);
                            }
                            break;
                        case ArchiveFileCompression.Uncompressed:
                            source.CopyTo(buffer);
                            break;
                        default:
                            throw new InvalidOperationException("Compression type is invalid.");
                    }

                    //Write the data and get a crc
                    buffer.Position = 0;
                    byte[] bBuffer = buffer.ToArray();
                    crc = Crc32CAlgorithm.Compute(bBuffer);
                    dataWriter.Write(bBuffer);
                }
                
                //Calculate the position of the data
                long newsize = dataWriter.BaseStream.Position;
                actualsize = (uint)(newsize - (long)offset);
            }
            else
            {
                offset = reciept.offset;
                actualsize = reciept.length;
                crc = reciept.crc;
                Compression = reciept.compression;
            }

            //Write the header data
            metadataWriter.Write((byte)Type);
            metadataWriter.Write((byte)Flags);
            
            metadataWriter.Write((byte)Compression);

            metadataWriter.Write(Priority);

            metadataWriter.Write(crc);

            metadataWriter.Write(Source.Md5);

            metadataWriter.BaseStream.Seek(48, SeekOrigin.Current);

            byte[] uName = Encoding.Unicode.GetBytes(Name);
            metadataWriter.Write((ushort)uName.Length);
            metadataWriter.Write(uName);

            metadataWriter.Write(offset);
            metadataWriter.Write(Source.Size);
            metadataWriter.Write(actualsize);

            return new WriteReciept
            {
                Md5 = Source.Md5,
                offset = offset,
                length = actualsize,
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
