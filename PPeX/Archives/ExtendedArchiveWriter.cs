﻿using System;
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

        /// <summary>
        /// The maximum uncompressed size a chunk can consist of.
        /// </summary>
        public ulong ChunkSizeLimit { get; set; }

        /// <summary>
        /// The amount of threads to use when writing the .ppx file.
        /// </summary>
        public int Threads { get; set; }

        protected bool leaveOpen;

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

            ChunkSizeLimit = 16 * 1024 * 1024;
            Threads = 4;

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

        protected List<ChunkWriter> AllocateChunks(IProgress<Tuple<string, int>> progress)
        {
            List<ChunkWriter> chunks = new List<ChunkWriter>();
            uint ID = 0;

            Queue<ISubfile> fileList;

            ChunkWriter currentChunk;
            ulong currentSize = 0;

            double total;


            List<ISubfile> GenericFiles = new List<ISubfile>(Files);

            //XX3 chunks
            progress.Report(new Tuple<string, int>("Allocating Xx3 chunks...\r\n", 0));

            List<ISubfile> Xx3Files = new List<ISubfile>();
            
            foreach (var file in Files)
            {
                if (file.Type == ArchiveFileType.Xx3Mesh)
                {
                    Xx3Files.Add(file);
                    GenericFiles.Remove(file);
                }
            }

            fileList = new Queue<ISubfile>(Xx3Files.OrderBy(x => x.Source.Md5, new ByteArrayComparer()));

            total = fileList.Count;

            currentChunk = new ChunkWriter(ID++, DefaultCompression, ChunkType.Xx3);

            while (fileList.Count > 0)
            {
                progress.Report(new Tuple<string, int>("", (int)(((total - fileList.Count) * 100) / total)));

                ISubfile file = fileList.Dequeue();

                if (currentChunk.Files.Any(x => Utility.CompareBytes(x.Source.Md5, file.Source.Md5)))
                {
                    //if the file is a dupe, add the file regardless
                    currentChunk.Files.Add(file);
                    continue;
                }

                ulong fileSize = file.Source.Size;

                //This takes longer but maximises space efficiency
                /*
                //need to calculate size based on encoded data
                using (var encoder = file.GetEncoder())
                {
                    encoder.Encode().Dispose();
                    fileSize = encoder.EncodedLength;
                }
                */

                if (currentSize + fileSize > ChunkSizeLimit)
                {
                    //cut off the chunk here
                    chunks.Add(currentChunk);

                    //create a new chunk
                    currentChunk = new ChunkWriter(ID++, DefaultCompression, ChunkType.Xx3);
                    currentSize = 0;
                }

                currentChunk.Files.Add(file);
                currentSize += fileSize;

            }

            if (currentChunk.Files.Count > 0)
                chunks.Add(currentChunk);


            //GENERIC chunks
            progress.Report(new Tuple<string, int>("Allocating generic chunks...\r\n", 0));
            //Create a LST chunk
            ChunkWriter LSTWriter = new ChunkWriter(ID++, DefaultCompression, ChunkType.Generic);


            //bunch duplicate files together
            //going to assume OrderBy is a stable sort
            fileList = new Queue<ISubfile>(
                GenericFiles.OrderBy(x => x.Source.Md5, new ByteArrayComparer()) //first sort all similar hashes together
                .OrderBy(x => Path.GetExtension(x.Name) ?? x.Name)); //then we order by file type, preserving duplicate file order


            total = fileList.Count;
            currentSize = 0;

            currentChunk = new ChunkWriter(ID++, DefaultCompression, ChunkType.Generic);

            while (fileList.Count > 0)
            {
                progress.Report(new Tuple<string, int>("", (int)(((total - fileList.Count) * 100) / total)));

                ISubfile file = fileList.Dequeue();

                if (file.Name.EndsWith(".lst"))
                {
                    LSTWriter.Files.Add(file);
                    continue;
                }

                if (currentChunk.Files.Any(x => Utility.CompareBytes(x.Source.Md5, file.Source.Md5)))
                {
                    //if the file is a dupe, add the file regardless
                    currentChunk.Files.Add(file);
                    continue;
                }

                if (file.Type == ArchiveFileType.XggAudio)
                {
                    //non-compressable data, assign it and any duplicates to a new chunk

                    ChunkWriter tempChunk = new ChunkWriter(ID++, ArchiveChunkCompression.Uncompressed, ChunkType.Generic);

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
                    continue;
                }

                ulong fileSize = file.Source.Size;

                //This takes longer but maximises space efficiency
                /*
                if (file.Type != ArchiveFileType.Raw)
                {
                    //need to calculate size based on encoded data
                    using (var encoder = file.GetEncoder())
                    {
                        encoder.Encode().Dispose();
                        fileSize = encoder.EncodedLength;
                    }
                }
                */

                if (currentSize + fileSize > ChunkSizeLimit)
                {
                    //cut off the chunk here
                    chunks.Add(currentChunk);

                    //create a new chunk
                    currentChunk = new ChunkWriter(ID++, DefaultCompression, ChunkType.Generic);
                    currentSize = 0;
                }

                currentChunk.Files.Add(file);
                currentSize += fileSize;

            }

            if (LSTWriter.Files.Count > 0)
                chunks.Add(LSTWriter);

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
            Xx3Encoder.texBank = new Xx2.TextureBank();

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
                                    "Allocating chunks...\r\n",
                                    0));

                List<ChunkWriter> allocatedChunks = AllocateChunks(progress);

                int i = 0;

                //foreach (ChunkWriter chunk in allocatedChunks)
                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Threads
                };

                uint fileCount = 0;

                Parallel.ForEach(allocatedChunks, options, (chunk) =>
                {
                    try
                    {
                        //Write the chunk

                        /*
                        using (MemoryStream compressedData = chunk.CompressData())
                            lock (ArchiveStream)
                            {
                                chunk.WriteChunkTable(chunkTableWriter, (ulong)ArchiveStream.Position, fileCount);

                                fileCount += (uint)chunk.WriteFileTable(fileTableWriter, fileCount);
                                
                                compressedData.CopyTo(ArchiveStream);

                                i++;
                            }

                        */

                        chunk.CompressHybrid(chunkTableWriter, fileTableWriter, ref fileCount, ArchiveStream);

                        i++;
                    }
                    catch
                    {
                        //Cancel the write process on error
                        progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + allocatedChunks.Count + "] Stopped writing chunk id:" + chunk.ID + "\r\n",
                                    100 * i / allocatedChunks.Count));

                        throw;
                    }

                    //Update progress
                    progress.Report(new Tuple<string, int>(
                                    "[" + i + " / " + allocatedChunks.Count + "] Written chunk id:" + chunk.ID + " (" + chunk.Files.Count + " files)\r\n",
                                    100 * i / allocatedChunks.Count));

                    //Compress memory
                    Utility.GCCompress();
                });

                int additionalChunks = 0;

                //write texture bank chunk
                progress.Report(new Tuple<string, int>("Writing texture bank...\r\n", 100));
                var textureBankWriter = new ChunkWriter((uint)(allocatedChunks.Count + (additionalChunks++)), DefaultCompression, ChunkType.Xx3);
                var textureBankData = new MemoryStream();
                Xx3Encoder.texBank.Write(new BinaryWriter(textureBankData));

                textureBankWriter.Files.Add(
                    new Subfile(
                        new MemorySource(textureBankData.ToArray()),
                        "TextureBank",
                        "_xx3"));
                    
                textureBankWriter.CompressHybrid(chunkTableWriter, fileTableWriter, ref fileCount, ArchiveStream);

                //Compress memory
                Utility.GCCompress();

                ulong chunkTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)allocatedChunks.Count + additionalChunks);

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

            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            progress.Report(new Tuple<string, int>("Finished.\n", 100));
        }


        protected class ChunkWriter
        {
            public uint ID { get; protected set; }

            public ChunkType Type { get; protected set; }

            public ArchiveChunkCompression Compression { get; protected set; }
            
            ulong UncompressedSize { get; set; }

            public ChunkWriter(uint id, ArchiveChunkCompression compression, ChunkType type)
            {
                ID = id;
                Compression = compression;
                Type = type;
            }

            public List<ISubfile> Files = new List<ISubfile>();

            public int WriteFileTable(BinaryWriter fileTableWriter, uint fileOffset)
            {
                ulong offset = 0;
                List<WriteReciept> reciepts = new List<WriteReciept>();

                int i = 0;

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


                    //check if it's a dupe
                    var reciept = reciepts.FirstOrDefault(x => Utility.CompareBytes(x.Md5, file.Source.Md5));

                    if (reciept != null)
                    {
                        //dupe
                        fileTableWriter.Write((uint)(fileOffset + reciept.Index));

                        fileTableWriter.Write(ID);
                        fileTableWriter.Write(reciept.Offset);
                        fileTableWriter.Write(reciept.Length);
                    }
                    else
                    {
                        //not a dupe
                        fileTableWriter.Write(ArchiveFileSource.CanonLinkID);

                        fileTableWriter.Write(ID);
                        fileTableWriter.Write(offset);

                        ulong size = file.Source.Size;

                        fileTableWriter.Write(size);

                        reciepts.Add(new WriteReciept
                        {
                            Md5 = file.Source.Md5,
                            Length = size,
                            Offset = offset,
                            Index = i
                        });
                        
                        offset += size;
                    }

                    i++;
                }

                return Files.Count;
            }

            protected MemoryStream CachedOutput;

            public void WriteChunkTable(BinaryWriter chunkTableWriter, ulong chunkFileOffset, uint fileOffset)
            {
                chunkTableWriter.Write(ID);

                chunkTableWriter.Write((byte)Type);

                chunkTableWriter.Write((byte)Compression);

                uint crc = Crc32CAlgorithm.Compute(CachedOutput.ToArray());

                chunkTableWriter.Write(crc);

                chunkTableWriter.Write(chunkFileOffset);
                chunkTableWriter.Write((ulong)CachedOutput.Length);
                chunkTableWriter.Write(UncompressedSize);

                chunkTableWriter.Write(fileOffset);
                chunkTableWriter.Write((uint)Files.Count);
            }

            public MemoryStream CompressData()
            {
                //generate the chunk
                CachedOutput = new MemoryStream();

                using (MemoryStream uncompressed = new MemoryStream())
                {
                    List<WriteReciept> reciepts = new List<WriteReciept>();

                    foreach (var file in Files)
                    {
                        //check if it's a dupe
                        var reciept = reciepts.FirstOrDefault(x => Utility.CompareBytes(x.Md5, file.Source.Md5));

                        if (reciept == null)
                        {
                            //not a dupe
                            reciepts.Add(new WriteReciept
                            {
                                Md5 = file.Source.Md5
                            });

                            using (IEncoder encoder = EncoderFactory.GetEncoder(file.Source, file.Type))
                            {
                                encoder.Encode().CopyTo(uncompressed);
                            }
                        }
                    }

                    UncompressedSize = (ulong)uncompressed.Length;

                    ulong iUncompressedSize = (ulong)Files.Sum(x => (long)x.Size);

                    uncompressed.Position = 0;

                    using (ICompressor compressor = CompressorFactory.GetCompressor(uncompressed, Compression))
                        compressor.WriteToStream(CachedOutput);
                }

                CachedOutput.Position = 0;

                return CachedOutput;
            }

            public void CompressHybrid(BinaryWriter chunkTableWriter, BinaryWriter fileTableWriter, ref uint fileOffset, Stream archiveStream)
            {
                ulong offset = 0;
                List<WriteReciept> reciepts = new List<WriteReciept>();

                int i = 0;

                //generate the chunk
                CachedOutput = new MemoryStream();

                using (MemoryStream uncompressed = new MemoryStream())
                {

                    lock (fileTableWriter)
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

                            //check if it's a dupe
                            var reciept = reciepts.FirstOrDefault(x => Utility.CompareBytes(x.Md5, file.Source.Md5));

                            if (reciept != null)
                            {
                                //dupe
                                fileTableWriter.Write((uint)(fileOffset + reciept.Index));

                                fileTableWriter.Write(ID);
                                fileTableWriter.Write(reciept.Offset);
                                fileTableWriter.Write(reciept.Length);
                            }
                            else
                            {
                                //not a dupe
                                fileTableWriter.Write(ArchiveFileSource.CanonLinkID);

                                fileTableWriter.Write(ID);
                                fileTableWriter.Write(offset);

                                long originalPosition = uncompressed.Position;


                                using (IEncoder encoder = EncoderFactory.GetEncoder(file.Source, file.Type))
                                {
                                    encoder.Encode().CopyTo(uncompressed);
                                }


                                ulong size = (ulong)(uncompressed.Position - originalPosition);//file.Source.Size;

                                fileTableWriter.Write(size);

                                reciepts.Add(new WriteReciept
                                {
                                    Md5 = file.Source.Md5,
                                    Length = size,
                                    Offset = offset,
                                    Index = i
                                });

                                offset += size;
                            }

                            i++;
                        }

                    UncompressedSize = (ulong)uncompressed.Length;

                    ulong iUncompressedSize = (ulong)Files.Sum(x => (long)x.Size);

                    uncompressed.Position = 0;

                    using (ICompressor compressor = CompressorFactory.GetCompressor(uncompressed, Compression))
                        compressor.WriteToStream(CachedOutput);
                }

                CachedOutput.Position = 0;

                lock (archiveStream)
                {
                    WriteChunkTable(chunkTableWriter, (ulong)archiveStream.Position, fileOffset);

                    fileOffset += (uint)Files.Count;

                    CachedOutput.CopyTo(archiveStream);
                }

                CachedOutput.Dispose();
            }
            
        }

        public class WriteReciept
        {
            public byte[] Md5;
            public ulong Offset;
            public ulong Length;
            public int Index;
        }
    }
}
