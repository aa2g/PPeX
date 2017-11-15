using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using Crc32C;
using PPeX.Encoders;
using System.Collections.Concurrent;
using System.Threading;
using PPeX.Xx2;

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

        protected BlockingCollection<HybridChunkWriter> QueuedChunks;

        protected List<ChunkReceipt> CompletedChunks;

        public TextureBank TextureBank = new TextureBank();

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
            Threads = 1;

            leaveOpen = LeaveOpen;
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        public void Write()
        {
            IProgress<string> progress1 = new Progress<string>();
            IProgress<int> progress2 = new Progress<int>();

            Write(progress1, progress2);
        }

        protected void AllocateBlocking(IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage, BlockingCollection<HybridChunkWriter> chunks)
        {
            uint ID = 0;

            Queue<ISubfile> fileList;

            HybridChunkWriter currentChunk;

            double total;

            List<ISubfile> GenericFiles = new List<ISubfile>(Files);

            //XX3 chunks
            ProgressStatus.Report("Allocating Xx3 chunks...\r\n");
            ProgressPercentage.Report(0);

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

            currentChunk = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Xx3, this);

            while (fileList.Count > 0)
            {
                ProgressPercentage.Report((int)(((total - fileList.Count) * 100) / total));

                ISubfile file = fileList.Dequeue();

                if (file.Source.Size == 0)
                    continue;

                if (!currentChunk.TryAddFile(file, ChunkSizeLimit))
                {
                    //cut off the chunk here
                    chunks.Add(currentChunk);

                    //create a new chunk
                    currentChunk = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Xx3, this);
                    currentChunk.AddFile(file);
                }
            }

            if (currentChunk.ContainsFiles)
                chunks.Add(currentChunk);


            //write texture bank chunk
            if (TextureBank.Textures.Count > 0)
            {
                ProgressStatus.Report("Writing texture bank...\r\n");
                var textureFiles = TextureBank.Textures.Select(texture => new Subfile(
                        new MemorySource(texture.Value),
                        texture.Key,
                        "_TextureBank"))
                    .OrderBy(x => x.Source.Md5, new ByteArrayComparer());

                var textureBankWriter = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Xx3, this);

                int i = 0;
                foreach (var file in textureFiles)
                {
                    if (!textureBankWriter.TryAddFile(file, ChunkSizeLimit))
                    {
                        chunks.Add(textureBankWriter);

                        textureBankWriter = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Xx3, this);
                        textureBankWriter.AddFile(file);
                    }
                }

                chunks.Add(textureBankWriter);
            }


            //GENERIC chunks
            ProgressStatus.Report("Allocating generic chunks...\r\n");
            ProgressPercentage.Report(0);

            //Create a LST chunk
            HybridChunkWriter LSTWriter = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Generic, this);


            //bunch duplicate files together
            //going to assume OrderBy is a stable sort
            fileList = new Queue<ISubfile>(
                GenericFiles.OrderBy(x => x.Source.Md5, new ByteArrayComparer()) //first sort all similar hashes together
                .OrderBy(x => Path.GetExtension(x.Name) ?? x.Name)); //then we order by file type, preserving duplicate file order


            total = fileList.Count;

            currentChunk = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Generic, this);

            while (fileList.Count > 0)
            {
                ProgressPercentage.Report((int)(((total - fileList.Count) * 100) / total));

                ISubfile file = fileList.Dequeue();

                if (file.Name.EndsWith(".lst"))
                {
                    LSTWriter.AddFile(file);
                    continue;
                }

                if (file.Type == ArchiveFileType.XggAudio)
                {
                    //non-compressable data, assign it and any duplicates to a new chunk

                    HybridChunkWriter tempChunk = new HybridChunkWriter(ID++, ArchiveChunkCompression.Uncompressed, ChunkType.Generic, this);

                    tempChunk.AddFile(file);

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

                        tempChunk.AddFile(fileList.Dequeue());
                    }

                    chunks.Add(tempChunk);
                    continue;
                }

                if (!currentChunk.TryAddFile(file, ChunkSizeLimit))
                {
                    //cut off the chunk here
                    chunks.Add(currentChunk);

                    //create a new chunk
                    currentChunk = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Generic, this);
                    currentChunk.AddFile(file);
                }
            }

            if (currentChunk.ContainsFiles)
                chunks.Add(currentChunk);

            if (LSTWriter.ContainsFiles)
                chunks.Add(LSTWriter);

            chunks.CompleteAdding();
        }
        
        Thread[] threadObjects;
        IProgress<string> threadProgress;

        public void CompressCallback(object threadContext)
        {
            while (!QueuedChunks.IsCompleted)
            {
                HybridChunkWriter item;
                bool result = QueuedChunks.TryTake(out item, 1000);

                if (result)
                {
                    item.Compress();

                    threadProgress.Report("Written chunk id:" + item.ID + " (" + item.Receipt.FileReceipts.Count + " files)\r\n");

                    lock (ArchiveStream)
                    {
                        using (item)
                        {
                            item.CompressedStream.CopyTo(ArchiveStream);

                            CompletedChunks.Add(item.Receipt);
                        }
                    }

                    //Aggressive GC collections
                    Utility.GCCompress();
                }
            }
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        /// <param name="progress">The progress callback object.</param>
        public void Write(IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
            TextureBank = new TextureBank();

            QueuedChunks = new BlockingCollection<HybridChunkWriter>(Threads);

            CompletedChunks = new List<ChunkReceipt>();
            threadProgress = ProgressStatus;

            threadObjects = new Thread[Threads];
            for (int i = 0; i < Threads; i++)
            {
                threadObjects[i] = new Thread(new ParameterizedThreadStart(CompressCallback));
                threadObjects[i].Start(null);
            }

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

                //give a generous 1kb buffer for future edits
                dataWriter.Seek(1024, SeekOrigin.Current);

                ulong chunkOffset = (ulong)dataWriter.BaseStream.Position;


                ProgressStatus.Report("Allocating chunks...\r\n");
                ProgressPercentage.Report(0);

                AllocateBlocking(ProgressStatus, ProgressPercentage, QueuedChunks);

                //wait for threads
                foreach (Thread thread in threadObjects)
                    thread.Join();

                //ManualResetEvent.WaitAll(threadCompleteEvents);

                //Write all metadata
                ulong currentChunkOffset = chunkOffset;
                uint fileOffset = 0;

                foreach (var finishedChunk in CompletedChunks)
                {
                    WriteChunkTable(chunkTableWriter, finishedChunk, ref currentChunkOffset, fileOffset);

                    WriteFileTable(fileTableWriter, finishedChunk, ref fileOffset);
                }

                //Compress memory
                Utility.GCCompress();

                ulong chunkTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)CompletedChunks.Count);

                chunkTableStream.Position = 0;
                chunkTableStream.CopyTo(ArchiveStream);


                ulong fileTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)CompletedChunks.Sum(x => x.FileReceipts.Count));

                fileTableStream.Position = 0;
                fileTableStream.CopyTo(ArchiveStream);


                ArchiveStream.Position = tableInfoOffset;
                dataWriter.Write(chunkTableOffset);
                dataWriter.Write(fileTableOffset);
            }

            Utility.GCCompress();

            ProgressStatus.Report("Finished.\r\n");
            ProgressPercentage.Report(100);
        }

        public static void WriteChunkTable(BinaryWriter chunkTableWriter, ChunkReceipt receipt, ref ulong chunkOffset, uint fileOffset)
        {
            chunkTableWriter.Write(receipt.ID);

            chunkTableWriter.Write((byte)receipt.Type);

            chunkTableWriter.Write((byte)receipt.Compression);

            chunkTableWriter.Write(receipt.CRC);

            chunkTableWriter.Write(chunkOffset);
            chunkTableWriter.Write(receipt.CompressedSize);
            chunkTableWriter.Write(receipt.UncompressedSize);

            //pp2 compatiblity
            chunkTableWriter.Write(fileOffset);
            chunkTableWriter.Write((uint)receipt.FileReceipts.Count);

            chunkOffset += receipt.CompressedSize;
        }

        public static void WriteFileTable(BinaryWriter fileTableWriter, ChunkReceipt chunkReceipt, ref uint fileOffset)
        {
            Dictionary<Md5Hash, int> checkedHashes = new Dictionary<Md5Hash, int>();

            int index = 0;
            foreach (var receipt in chunkReceipt.FileReceipts)
            {
                //write each file metadata
                byte[] archive_name = Encoding.Unicode.GetBytes(receipt.Subfile.ArchiveName);

                fileTableWriter.Write((ushort)archive_name.Length);
                fileTableWriter.Write(archive_name);


                byte[] file_name = Encoding.Unicode.GetBytes(receipt.Subfile.Name);

                fileTableWriter.Write((ushort)file_name.Length);
                fileTableWriter.Write(file_name);

                fileTableWriter.Write((ushort)receipt.Subfile.Type);
                fileTableWriter.Write(receipt.Md5);


                //pp2 compatiblity
                //check if it's a dupe
                int previousIndex;
                if (checkedHashes.TryGetValue(receipt.Md5, out previousIndex))
                {
                    //dupe
                    fileTableWriter.Write((uint)(fileOffset + previousIndex));
                }
                else
                {
                    //not a dupe
                    fileTableWriter.Write(ArchiveFileSource.CanonLinkID);

                    checkedHashes.Add(receipt.Md5, index);
                }
                    


                fileTableWriter.Write(chunkReceipt.ID);
                fileTableWriter.Write(receipt.Offset);
                fileTableWriter.Write(receipt.Length);

                index++;
            }

            fileOffset += (uint)chunkReceipt.FileReceipts.Count;
        }


        protected class HybridChunkWriter : IDisposable
        {
            public uint ID { get; protected set; }

            public ChunkType Type { get; protected set; }

            public ArchiveChunkCompression Compression { get; protected set; }

            ulong UncompressedSize { get; set; }

            protected ExtendedArchiveWriter writer;

            public HybridChunkWriter(uint id, ArchiveChunkCompression compression, ChunkType type, ExtendedArchiveWriter writer)
            {
                ID = id;
                Compression = compression;
                Type = type;
                this.writer = writer;
            }

            protected MemoryStream UncompressedStream = new MemoryStream();

            public bool IsReady => CompressedStream != null;

            public bool ContainsFiles => fileReceipts.Count > 0;

            public MemoryStream CompressedStream { get; protected set; }

            protected List<FileReceipt> fileReceipts = new List<FileReceipt>();

            public ChunkReceipt Receipt { get; protected set; }

            public void AddFile(ISubfile file)
            {
                Md5Hash hash = file.Source.Md5;

                if (fileReceipts.Any(x => x.Md5 == hash))
                {
                    FileReceipt original = fileReceipts.First(x => x.Md5 == hash);

                    FileReceipt duplicate = FileReceipt.CreateDuplicate(original, file);

                    fileReceipts.Add(duplicate);

                    return;
                }

                using (IEncoder encoder = EncoderFactory.GetEncoder(file.Source, writer, file.Type))
                using (var encoded = encoder.Encode())
                {
                    FileReceipt receipt = new FileReceipt
                    {
                        Md5 = hash,
                        Length = (ulong)encoded.Length,
                        Offset = (ulong)UncompressedStream.Position,
                        Subfile = file
                    };

                    fileReceipts.Add(receipt);

                    encoded.CopyTo(UncompressedStream);
                }
            }

            public bool TryAddFile(ISubfile file, ulong maxChunkSize)
            {
                Md5Hash hash = file.Source.Md5;

                if (fileReceipts.Any(x => x.Md5 == hash))
                {
                    FileReceipt original = fileReceipts.First(x => x.Md5 == hash);

                    FileReceipt duplicate = FileReceipt.CreateDuplicate(original, file);

                    fileReceipts.Add(duplicate);

                    return true;
                }

                using (IEncoder encoder = EncoderFactory.GetEncoder(file.Source, writer, file.Type))
                using (var encoded = encoder.Encode())
                {
                    if (UncompressedStream.Length == 0 ||
                        (ulong)(encoder.EncodedLength + UncompressedStream.Length) <= maxChunkSize)
                    {
                        FileReceipt receipt = new FileReceipt
                        {
                            Md5 = hash,
                            Length = encoder.EncodedLength,
                            Offset = (ulong)UncompressedStream.Position,
                            Subfile = file
                        };

                        fileReceipts.Add(receipt);

                        encoded.CopyTo(UncompressedStream);

                        return true;
                    }
                }
                return false;
            }

            public void Compress()
            {
                UncompressedStream.Position = 0;

                using (UncompressedStream)
                using (ICompressor compressor = CompressorFactory.GetCompressor(UncompressedStream, Compression))
                {
                    CompressedStream = new MemoryStream();

                    compressor.WriteToStream(CompressedStream);

                    uint crc = Crc32CAlgorithm.Compute(CompressedStream.ToArray());

                    Receipt = new ChunkReceipt
                    {
                        ID = this.ID,
                        Compression = this.Compression,
                        Type = this.Type,
                        CRC = crc,
                        UncompressedSize = (ulong)UncompressedStream.Length,
                        CompressedSize = (ulong)CompressedStream.Length,
                        FileReceipts = fileReceipts
                    };

                    CompressedStream.Position = 0;
                }

                //FinishedWriters.Enqueue(this);
            }

            public void Dispose()
            {
                if (UncompressedStream != null)
                    UncompressedStream.Dispose();

                if (CompressedStream != null)
                    CompressedStream.Dispose();
            }
        }

        public class FileReceipt
        {
            public ISubfile Subfile;

            public Md5Hash Md5;
            public ulong Offset;
            public ulong Length;
            //public int Index;

            public static FileReceipt CreateDuplicate(FileReceipt original, ISubfile subfile)
            {
                FileReceipt receipt = original.MemberwiseClone() as FileReceipt;

                receipt.Subfile = subfile;

                return receipt;
            }
        }

        public class ChunkReceipt
        {
            public uint ID;
            public ChunkType Type;
            public ArchiveChunkCompression Compression;
            public uint CRC;
            public ulong CompressedSize;
            public ulong UncompressedSize;

            public ICollection<FileReceipt> FileReceipts;
        }
    }
}
