using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using PPeX.External.CRC32;
using PPeX.Encoders;
using System.Collections.Concurrent;
using System.Threading;
using PPeX.Xx2;
using PPeX.Archives;
using PPeX.Archives.Writers;

namespace PPeX
{
    /// <summary>
    /// A writer for extended archives, to .ppx files.
    /// </summary>
    public class ExtendedArchiveWriter : IArchiveContainer
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

        internal BlockingCollection<IThreadWork> QueuedChunks;

        protected List<ChunkReceipt> CompletedChunks;

        public TextureBank TextureBank { get; protected set; }

        /// <summary>
        /// Creates a new extended archive writer.
        /// </summary>
        /// <param name="File">The stream of the .ppx file to be written to.</param>
        /// <param name="Name">The display name for the archive.</param>
        public ExtendedArchiveWriter(string Name, bool LeaveOpen = false)
        {
            this.Name = Name;
            DefaultCompression = ArchiveChunkCompression.Zstandard;
            TextureBank = new CompressedTextureBank(ArchiveChunkCompression.LZ4);

            ChunkSizeLimit = 16 * 1024 * 1024;
            Threads = 1;

            leaveOpen = LeaveOpen;
        }

        internal void AllocateBlocking(IEnumerable<ISubfile> FilesToAdd, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage, BlockingCollection<IThreadWork> chunks, uint startingID = 0)
        {
            uint ID = startingID;

            Queue<ISubfile> fileList;

            HybridChunkWriter currentChunk;

            double total;

            List<ISubfile> GenericFiles = new List<ISubfile>();
            List<ISubfile> Xx3Files = new List<ISubfile>();

            foreach (var file in FilesToAdd)
            {
                if (file.Type == ArchiveFileType.Xx3Mesh)
                {
                    Xx3Files.Add(file);
                }
                else
                {
                    GenericFiles.Add(file);
                }
            }

            //XX3 chunks
            ProgressStatus.Report("Allocating Xx3 chunks...\r\n");
            ProgressPercentage.Report(0);
            
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
            if (TextureBank.Count > 0)
            {
                ProgressStatus.Report("Writing texture bank...\r\n");
                var textureFiles = TextureBank.Select(texture => new Subfile(
                        new MemorySource(texture.Value),
                        texture.Key,
                        "_TextureBank"))
                    .OrderBy(x => x.Source.Md5, new ByteArrayComparer());

                var textureBankWriter = new HybridChunkWriter(ID++, DefaultCompression, ChunkType.Xx3, this);
                
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
            LinkedList<ISubfile> linkedSubfileList = new LinkedList<ISubfile>(
                GenericFiles
                .OrderBy(x => x.Name) //order by file name first
                .OrderBy(x => Path.GetExtension(x.Name) ?? x.Name)); //then we order by file type, preserving duplicate file order

            Dictionary<Md5Hash, LinkedListNode<ISubfile>> HashList = new Dictionary<Md5Hash, LinkedListNode<ISubfile>>();

            var node = linkedSubfileList.First;

            while (node.Next != null)
            {
                ISubfile file = node.Value;
                Md5Hash hash = file.Source.Md5;

                if (HashList.ContainsKey(hash))
                {
                    var nextNode = node.Next;

                    var originalNode = HashList[hash];

                    linkedSubfileList.Remove(node);
                    linkedSubfileList.AddAfter(originalNode, file);

                    node = nextNode;
                }
                else
                {
                    HashList.Add(hash, node);

                    node = node.Next;
                }
            }

            fileList = new Queue<ISubfile>(linkedSubfileList);


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

                if (file.Type == ArchiveFileType.OpusAudio)
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
            var context = (WriterThreadContext)threadContext;

            while (!QueuedChunks.IsCompleted)
            {
                IThreadWork item;
                bool result = QueuedChunks.TryTake(out item, 500);

                if (result)
                {
                    Stream output = item.GetData();

                    threadProgress.Report("Written chunk id:" + item.Receipt.ID + " (" + item.Receipt.FileReceipts.Count + " files)\r\n");

                    lock (context.ArchiveStream)
                    {
                        using (item)
                        using (output)
                        {
                            ulong chunkOffset = (ulong)context.ArchiveStream.Position;

                            output.CopyTo(context.ArchiveStream);

                            item.Receipt.FileOffset = chunkOffset;

                            CompletedChunks.Add(item.Receipt);
                        }
                    }

                    //Aggressive GC collections
                    Utility.GCCompress();
                }
            }
        }

        protected long WriteHeader(BinaryWriter dataWriter, bool writeNew)
        {
            //Write container header data
            dataWriter.Write(Encoding.ASCII.GetBytes(ExtendedArchive.Magic));

            dataWriter.Write(ExtendedArchive.Version);

            byte[] title = Encoding.Unicode.GetBytes(Name);

            dataWriter.Write((ushort)title.Length);
            dataWriter.Write(title);

            //Write individual file header + data
            long tableInfoOffset = dataWriter.BaseStream.Position;

            if (writeNew)
                //give a generous 1kb buffer for future edits
                dataWriter.Seek(16 + 1024, SeekOrigin.Current);

            return tableInfoOffset;
        }

        protected void WriteTables(long tableInfoOffset, BinaryWriter dataWriter)
        {
            //Write all metadata
            Stream ArchiveStream = dataWriter.BaseStream;

            using (BinaryWriter chunkTableWriter = new BinaryWriter(new MemoryStream()))
            using (BinaryWriter fileTableWriter = new BinaryWriter(new MemoryStream()))
            {
                uint fileOffset = 0;

                int chunkCout = CompletedChunks.Count;

                foreach (var finishedChunk in CompletedChunks)
                {
                    WriteChunkTable(chunkTableWriter, finishedChunk, fileOffset);

                    WriteFileTable(fileTableWriter, finishedChunk, ref fileOffset);
                }

                ulong chunkTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)CompletedChunks.Count);

                Stream chunkTableStream = chunkTableWriter.BaseStream;
                chunkTableStream.Position = 0;
                chunkTableStream.CopyTo(ArchiveStream);


                ulong fileTableOffset = (ulong)ArchiveStream.Position;

                dataWriter.Write((uint)CompletedChunks.Sum(x => x.FileReceipts.Count));

                Stream fileTableStream = fileTableWriter.BaseStream;
                fileTableStream.Position = 0;
                fileTableStream.CopyTo(ArchiveStream);


                long oldpos = ArchiveStream.Position;

                ArchiveStream.Position = tableInfoOffset;
                dataWriter.Write(chunkTableOffset);
                dataWriter.Write(fileTableOffset);

                ArchiveStream.Position = oldpos;
            }
        }

        protected void InitializeThreads(int threads, Stream ArchiveStream, IProgress<string> ProgressStatus)
        {
            TextureBank = new CompressedTextureBank(ArchiveChunkCompression.LZ4);

            QueuedChunks = new BlockingCollection<IThreadWork>(Threads);

            CompletedChunks = new List<ChunkReceipt>();
            threadProgress = ProgressStatus;

            WriterThreadContext ctx = new WriterThreadContext { ArchiveStream = ArchiveStream };

            threadObjects = new Thread[Threads];
            for (int i = 0; i < Threads; i++)
            {
                threadObjects[i] = new Thread(new ParameterizedThreadStart(CompressCallback));
                threadObjects[i].Start(ctx);
            }
        }

        protected void WaitForThreadCompletion()
        {
            foreach (Thread thread in threadObjects)
                thread.Join();

            //ManualResetEvent.WaitAll(threadCompleteEvents);
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        public void Write(string filename)
        {
            IProgress<string> progress1 = new Progress<string>();
            IProgress<int> progress2 = new Progress<int>();

            using (FileStream fs = new FileStream(filename, FileMode.Create))
                Write(fs, progress1, progress2);
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        public void Write(Stream ArchiveStream)
        {
            IProgress<string> progress1 = new Progress<string>();
            IProgress<int> progress2 = new Progress<int>();

            Write(ArchiveStream, progress1, progress2);
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        /// <param name="progress">The progress callback object.</param>
        public void Write(Stream ArchiveStream, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
            if (!ArchiveStream.CanSeek || !ArchiveStream.CanWrite)
                throw new ArgumentException("Stream must be seekable and able to be written to.", nameof(ArchiveStream));

            InitializeThreads(Threads, ArchiveStream, ProgressStatus);
            
            using (BinaryWriter dataWriter = new BinaryWriter(ArchiveStream, Encoding.ASCII, leaveOpen))
            {
                //Write header
                long tableInfoOffset = WriteHeader(dataWriter, true);

                ulong chunkOffset = (ulong)dataWriter.BaseStream.Position;


                ProgressStatus.Report("Allocating chunks...\r\n");
                ProgressPercentage.Report(0);

                AllocateBlocking(Files, ProgressStatus, ProgressPercentage, QueuedChunks);

                WaitForThreadCompletion();
                
                WriteTables(tableInfoOffset, dataWriter);
            }

            //Collect garbage and compress memory
            Utility.GCCompress();

            ProgressStatus.Report("Finished.\r\n");
            ProgressPercentage.Report(100);
        }

        public static void WriteChunkTable(BinaryWriter chunkTableWriter, ChunkReceipt receipt, uint fileOffset)
        {
            chunkTableWriter.Write(receipt.ID);

            chunkTableWriter.Write((byte)receipt.Type);

            chunkTableWriter.Write((byte)receipt.Compression);

            chunkTableWriter.Write(receipt.CRC);

            chunkTableWriter.Write(receipt.FileOffset);
            chunkTableWriter.Write(receipt.CompressedSize);
            chunkTableWriter.Write(receipt.UncompressedSize);

            //pp2 compatiblity
            chunkTableWriter.Write(fileOffset);
            chunkTableWriter.Write((uint)receipt.FileReceipts.Count);
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

        protected class WriterThreadContext
        {
            public Stream ArchiveStream;
        }
    }
}
