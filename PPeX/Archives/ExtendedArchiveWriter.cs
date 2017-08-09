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
        public List<ISubfile> Files = new List<ISubfile>();

        /// <summary>
        /// The stream that the archive will be written to.
        /// </summary>
        public Stream ArchiveStream { get; protected set; }

        /// <summary>
        /// The compression type that the writer will default to.
        /// </summary>
        public ArchiveChunkCompression DefaultCompression { get; set; }

        public ulong ChunkSizeLimit { get; set; }

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
            DefaultCompression = ArchiveChunkCompression.Zstandard;

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

        protected List<ChunkWriter> AllocateChunks()
        {
            List<ChunkWriter> chunks = new List<ChunkWriter>();
            uint ID = 0;

            //bunch duplicate files together
            Queue<ISubfile> fileList = new Queue<ISubfile>(Files.OrderBy(x => x.Source.Md5, new ByteArrayComparer()));

            ChunkWriter currentChunk = new ChunkWriter(ID++, DefaultCompression);
            uint currentSize = 0;

            while (fileList.Count > 0)
            {
                ISubfile file = fileList.Dequeue();

                if (currentChunk.Files.Any(x => Utility.CompareBytes(x.Source.Md5, file.Source.Md5)))
                {
                    //if the file is a dupe, add the file regardless
                    currentChunk.Files.Add(file);
                    continue;
                }

                if (file.Type == ArchiveFileType.XggAudio)
                {
                    //non-compressable data, assign it and any duplicates to a new chunk

                    ChunkWriter tempChunk = new ChunkWriter(ID++, ArchiveChunkCompression.Uncompressed);

                    tempChunk.Files.Add(file);

                    byte[] md5Template = file.Source.Md5;

                    //keep peeking and dequeuing until we find another file
                    while (true)
                    {
                        if (fileList.Count == 0)
                            break; //no more files, need to stop

                        ISubfile next = fileList.Peek();

                        if (!Utility.CompareBytes(
                            md5Template,
                            next.Source.Md5))
                        {
                            //we've stopped finding duplicates
                            break;
                        }

                        tempChunk.Files.Add(fileList.Dequeue());
                    }

                    chunks.Add(tempChunk);
                    break;
                }

                if (currentSize + file.Source.Size > ChunkSizeLimit)
                {
                    //cut off the chunk here
                    chunks.Add(currentChunk);

                    //create a new chunk
                    currentChunk = new ChunkWriter(ID++, DefaultCompression);
                    currentSize = 0;
                }

                currentChunk.Files.Add(file);
                currentSize += file.Source.Size;

            }

            if (currentChunk.Files.Count > 0)
                chunks.Add(currentChunk);

            return chunks;
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        /// <param name="progress">The progress callback object.</param>
        public void Write(IProgress<Tuple<string, int>> progress)
        {
            using (MemoryStream fileTableStream = new MemoryStream())
            using (BinaryWriter fileTableWriter = new BinaryWriter(fileTableStream))
            using (MemoryStream chunkTableStream = new MemoryStream())
            using (BinaryWriter chunkTableWriter = new BinaryWriter(chunkTableStream))
            using (BinaryWriter dataWriter = new BinaryWriter(ArchiveStream, Encoding.ASCII, leaveOpen))
            {
                //Write container header data
                dataWriter.Write(Encoding.ASCII.GetBytes(ExtendedArchive.Magic));

                dataWriter.Write(ExtendedArchive.Version);

                byte[] title = Encoding.Unicode.GetBytes(Name);

                dataWriter.Write((ushort)title.Length);
                dataWriter.Write(title);

                //Write individual file header + data
                long tableInfoOffset = dataWriter.BaseStream.Position;

                dataWriter.Seek(16, SeekOrigin.Current);


                progress.Report(new Tuple<string, int>(
                                    "Allocating chunks...",
                                    0));

                List<ChunkWriter> allocatedChunks = AllocateChunks();

                int i = 0;

                foreach (ChunkWriter chunk in allocatedChunks)
                {
                    //try
                    //{
                        //Write the chunk
                        chunk.Write(chunkTableWriter, dataWriter, fileTableWriter);
                    //}
                    //catch
                    //{
                        //Cancel the write process on error
                        /*progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + allocatedChunks.Count + "] Stopped writing chunk id:" + chunk.ID + "\r\n",
                                    100 * i / allocatedChunks.Count));*/

                        //throw;
                    //}
                
                    //Update progress
                    progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + Files.Count + "] Written chunk id:" + chunk.ID + " (" + chunk.Files.Count + " files)\r\n",
                                    100 * i / allocatedChunks.Count));
                }

                ulong chunkTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)allocatedChunks.Count);

                chunkTableStream.Position = 0;
                chunkTableStream.CopyTo(ArchiveStream);


                ulong fileTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)Files.Count);

                fileTableStream.Position = 0;
                fileTableStream.CopyTo(ArchiveStream);


                ArchiveStream.Position = tableInfoOffset;
                dataWriter.Write(chunkTableOffset);
                dataWriter.Write(fileTableOffset);
            }

            progress.Report(new Tuple<string, int>("Finished.\n", 100));
        }


        protected class ChunkWriter
        {
            public uint ID { get; protected set; }
            public ArchiveChunkCompression Compression { get; protected set; }
            
            ulong UncompressedSize
            {
                get
                {
                    return (ulong)Files.Sum(x => x.Size);
                }
            }

            public ChunkWriter(uint id, ArchiveChunkCompression compression)
            {
                ID = id;
                Compression = compression;
            }

            public List<ISubfile> Files = new List<ISubfile>();

            public void Write(BinaryWriter chunkTableWriter, BinaryWriter chunkDataWriter, BinaryWriter fileTableWriter)
            {
                //generate the chunk
                using (MemoryStream mem = new MemoryStream())
                {
                    ulong offset = 0;

                    using (MemoryStream uncompressed = new MemoryStream())
                    {
                        List<WriteReciept> reciepts = new List<WriteReciept>();

                        foreach (var file in Files)
                        {
                            //write each file metadata
                            byte[] archive_name = Encoding.Unicode.GetBytes(file.ArchiveName);

                            fileTableWriter.Write((ushort)archive_name.Length);
                            fileTableWriter.Write(archive_name);


                            byte[] file_name = Encoding.Unicode.GetBytes(file.Name);

                            fileTableWriter.Write((ushort)file_name.Length);
                            fileTableWriter.Write(file_name);

                            fileTableWriter.Write((ushort)file.Type);
                            fileTableWriter.Write(file.Source.Md5);

                            fileTableWriter.Write(ID);

                            //check if it's a dupe
                            var reciept = reciepts.FirstOrDefault(x => Utility.CompareBytes(x.Md5, file.Source.Md5));

                            if (reciept != null)
                            {
                                //dupe
                                fileTableWriter.Write(reciept.Offset);
                                fileTableWriter.Write(reciept.Length);
                            }
                            else
                            {
                                //not a dupe
                                fileTableWriter.Write(offset);

                                ulong size = file.Source.Size;

                                fileTableWriter.Write(size);

                                reciepts.Add(new WriteReciept
                                {
                                    Md5 = file.Source.Md5,
                                    Length = size,
                                    Offset = offset
                                });

                                file.WriteToStream(uncompressed);
                                offset += size;
                            }

                            
                        }

                        uncompressed.Position = 0;

                        using (ICompressor compressor = CompressorFactory.GetCompressor(uncompressed, Compression))
                            compressor.WriteToStream(mem);
                    }

                    mem.Position = 0;

                    //start writing the chunk metadata
                    chunkTableWriter.Write(ID);

                    chunkTableWriter.Write((byte)Compression);


                    uint crc = Crc32CAlgorithm.Compute(mem.ToArray());

                    chunkTableWriter.Write(crc);

                    ulong fileOffset = (ulong)chunkDataWriter.BaseStream.Position;

                    chunkTableWriter.Write(fileOffset);
                    chunkTableWriter.Write(mem.Length);
                    chunkTableWriter.Write(UncompressedSize);

                    //finally write the chunk data
                    mem.CopyTo(chunkDataWriter.BaseStream);
                }

            }
        }

        public class WriteReciept
        {
            public byte[] Md5;
            public ulong Offset;
            public ulong Length;
        }
    }
}
