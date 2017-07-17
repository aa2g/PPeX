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
        public string Name { get; set; }

        public List<ArchiveFile> Files = new List<ArchiveFile>();
        public string Filename { get; protected set; }
        public ArchiveType Type { get; set; }
        public ArchiveFileCompression DefaultCompression { get; set; }

        public ExtendedArchiveWriter(string Filename, string Name)
        {
            this.Filename = Filename;
            this.Name = Name;
            Type = ArchiveType.Archive;
            DefaultCompression = ArchiveFileCompression.LZ4;
        }

        public void Write()
        {
            IProgress<Tuple<string, int>> progress = new Progress<Tuple<string, int>>();

            Write(progress);
        }

        public void Write(IProgress<Tuple<string, int>> progress)
        {
            List<uint> md5s = new List<uint>();

            using (FileStream arc = new FileStream(Filename, FileMode.Create))
            using (MemoryStream header = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(arc))
            using (BinaryWriter headerWriter = new BinaryWriter(header))
            {
                writer.Write(Encoding.ASCII.GetBytes(ExtendedArchive.Magic));

                writer.Write(ExtendedArchive.Version);
                writer.Write((ushort)1);

                byte[] title = Encoding.Unicode.GetBytes(Name);
                writer.Write((ushort)title.Length);
                writer.Write(title);

                writer.Write((uint)Files.Count);

                int headerLength = Files.Sum(x => x.HeaderLength);

                writer.Write((uint)headerLength);

                long headerPos = writer.BaseStream.Position;
                writer.BaseStream.Position = headerLength + (512 * 1024); //512kb empty space

                int i = 0;
                foreach (var file in Files)
                {
                    uint crc = Crc32CAlgorithm.Compute(file.Source.Md5);
                    i++;

                    bool isDuplicate = md5s.Contains(crc);

                    try
                    {
                        file.WriteTo(writer, headerWriter, isDuplicate);
                    }
                    catch
                    {
                        progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + Files.Count + "] Stopped writing " + file.Name + "\r\n",
                                    100 * i / Files.Count));

                        throw;
                    }

                    progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + Files.Count + "] Written " + file.Name + "... (" + file.Source.Size + " bytes)" + (isDuplicate ? " [duplicate]\r\n" : "\r\n"),
                                    100 * i / Files.Count));

                    md5s.Add(crc);
                }

                writer.BaseStream.Position = headerPos;
                writer.Write(header.ToArray());
            }

            progress.Report(new Tuple<string, int>("Finished.\n", 100));
        }
    }

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

        public IDataSource Source;

        public string Name { get; set; }

        internal int HeaderLength
        {
            get
            {
                return 90 + Encoding.Unicode.GetByteCount(Name);
            }
        }

        public ArchiveFileType Type = ArchiveFileType.Raw;
        public ArchiveFileFlags Flags = ArchiveFileFlags.None;
        public ArchiveFileCompression Compression = ArchiveFileCompression.Uncompressed;
        public byte Priority;

        public ArchiveFile(IDataSource Source, string Name, ArchiveFileCompression Compression, byte Priority)
        {
            if (Name.EndsWith(".wav"))
            {
                //this.Source = new CompressedDataSource(Source, ArchiveFileType.Audio);
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

        public void WriteTo(BinaryWriter dataWriter, BinaryWriter metadataWriter, bool asDuplicate)
        {
            uint actualsize = 0;
            uint crc = 0;
            ulong offset = (ulong)dataWriter.BaseStream.Position;

            if (!asDuplicate)
            {

                using (MemoryStream buffer = new MemoryStream()) 
                using (Stream source = Source.GetStream())
                {
                    switch (Compression)
                    {
                        case ArchiveFileCompression.LZ4:
                            using (LZ4Stream lz = new LZ4Stream(buffer, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression | LZ4StreamFlags.IsolateInnerStream, 4 * 1048576))
                                source.CopyTo(lz);
                            break;
                        case ArchiveFileCompression.Zstandard:
                            using (ZstdNet.Compressor zstd = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(3)))
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

                    buffer.Position = 0;
                    byte[] bBuffer = buffer.ToArray();
                    crc = Crc32CAlgorithm.Compute(bBuffer);
                    dataWriter.Write(bBuffer);
                }
                

                long newsize = dataWriter.BaseStream.Position;
                actualsize = (uint)(newsize - (long)offset);
            }

            metadataWriter.Write((byte)Type);
            metadataWriter.Write((byte)Flags);

            if (asDuplicate)
                metadataWriter.Write((byte)ArchiveFileCompression.Duplicate);
            else
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
        }
    }
}
