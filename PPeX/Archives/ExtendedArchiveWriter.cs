﻿using System;
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
        /// The filename that the archive will be written to.
        /// </summary>
        public string Filename { get; protected set; }
        /// <summary>
        /// The type of archive that will be created.
        /// </summary>
        public ArchiveType Type { get; set; }
        /// <summary>
        /// The compression type that the writer will default to.
        /// </summary>
        public ArchiveFileCompression DefaultCompression { get; set; }

        /// <summary>
        /// Creates a new extended archive writer.
        /// </summary>
        /// <param name="Filename">The filename of the .ppx file to be created.</param>
        /// <param name="Name">The display name for the archive.</param>
        public ExtendedArchiveWriter(string Filename, string Name)
        {
            this.Filename = Filename;
            this.Name = Name;
            Type = ArchiveType.Archive;
            DefaultCompression = ArchiveFileCompression.LZ4;
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
            List<uint> md5s = new List<uint>();

            using (FileStream arc = new FileStream(Filename, FileMode.Create))
            using (MemoryStream header = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(arc))
            using (BinaryWriter headerWriter = new BinaryWriter(header))
            {
                //Write container header data
                writer.Write(Encoding.ASCII.GetBytes(ExtendedArchive.Magic));

                writer.Write(ExtendedArchive.Version);
                writer.Write((ushort)1);

                byte[] title = Encoding.Unicode.GetBytes(Name);
                writer.Write((ushort)title.Length);
                writer.Write(title);

                writer.Write((uint)Files.Count);

                //Write individual file header + data
                int headerLength = Files.Sum(x => x.HeaderLength);

                writer.Write((uint)headerLength);

                long headerPos = writer.BaseStream.Position;
                writer.BaseStream.Position = headerLength + (512 * 1024); //512kb empty space

                int i = 0;
                foreach (var file in Files)
                {
                    //Reduce the MD5 to a single int
                    uint crc = Crc32CAlgorithm.Compute(file.Source.Md5);
                    i++;

                    bool isDuplicate = md5s.Contains(crc);

                    try
                    {
                        //Write the file
                        file.WriteTo(writer, headerWriter, isDuplicate);
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

                    md5s.Add(crc);
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
        /// <param name="asDuplicate">True if the file is a duplicate.</param>
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
                    //Compress the data
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

            //Write the header data
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
