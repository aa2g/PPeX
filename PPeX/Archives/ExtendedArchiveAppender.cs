using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PPeX.Xx2;

namespace PPeX
{
    public class ExtendedArchiveAppender : IArchiveWriter
    {
        public string Title;

        public ExtendedArchive BaseArchive { get; protected set; }

        public List<ISubfile> FilesToAdd { get; protected set; }

        public List<ArchiveFileSource> FilesToRemove { get; protected set; }

        public ArchiveChunkCompression DefaultCompression { get; set; }

        public TextureBank TextureBank { get; protected set; }

        public ulong ChunkSizeLimit { get; protected set; }

        public int Threads { get; protected set; }

        internal BlockingCollection<HybridChunkWriter> QueuedChunks;

        internal List<ChunkReceipt> CompletedChunks;

        Thread[] threadObjects;
        IProgress<string> threadProgress;

        public ExtendedArchiveAppender(ExtendedArchive archive)
        {
            BaseArchive = archive;
            Title = BaseArchive.Title;
            FilesToAdd = new List<ISubfile>();
            FilesToRemove = new List<ArchiveFileSource>();
            DefaultCompression = ArchiveChunkCompression.Zstandard;
            TextureBank = new TextureBank();
            Threads = 1;
            ChunkSizeLimit = 16 * 1024 * 1024;
        }

        public ExtendedArchiveAppender(string filename) : this(new ExtendedArchive(filename))
        {
            
        }

        protected bool IsChunkUnedited(ExtendedArchiveChunk chunk)
        {
            return !FilesToRemove.Any(x => x.ChunkID == chunk.ID);
        }

        protected ChunkReceipt CopyChunk(ExtendedArchiveChunk chunk, out IList<ISubfile> AddedDupes)
        {
            List<FileReceipt> fileReciepts = new List<FileReceipt>();

            AddedDupes = new List<ISubfile>();

            foreach (var fileSource in chunk.Files.Select(x => x.RawSource))
            {
                FileReceipt reciept = new FileReceipt
                {
                    Offset = fileSource.Offset,
                    Length = fileSource.Size,
                    Md5 = fileSource.Md5,
                    Subfile = new ArchiveSubfile(fileSource)
                };

                fileReciepts.Add(reciept);

                IEnumerable<ISubfile> availableFiles = FilesToAdd.Where(x => x.Source.Md5 == fileSource.Md5);

                foreach (var dupeFile in availableFiles)
                {
                    if (!AddedDupes.Contains(dupeFile))
                    {
                        fileReciepts.Add(FileReceipt.CreateDuplicate(reciept, dupeFile));
                        AddedDupes.Add(dupeFile);
                    }
                }
            }

            return new ChunkReceipt
            {
                ID = chunk.ID,
                CRC = chunk.CRC32C,
                Type = chunk.Type,
                Compression = chunk.Compression,
                CompressedSize = chunk.CompressedLength,
                UncompressedSize = chunk.UncompressedLength,
                FileReceipts = fileReciepts
            };
        }

        protected ChunkReceipt CopyChunk(Stream stream, ExtendedArchiveChunk chunk, out IList<ISubfile> AddedDupes)
        {
            ulong offset = (ulong)stream.Position;

            using (Stream chunkStream = chunk.GetRawStream())
                chunkStream.CopyTo(stream);

            return CopyChunk(chunk, out AddedDupes);
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

        internal void AllocateBlocking(List<ISubfile> files, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage, BlockingCollection<HybridChunkWriter> chunks)
        {
            uint ID = BaseArchive.Chunks.Max(x => x.ID) + 1;

            HybridChunkWriter currentChunk;

            double total;

            //XX3 chunks
            ProgressStatus.Report("Allocating Xx3 chunks...\r\n");
            ProgressPercentage.Report(0);

            var fileList = new Queue<ISubfile>(files.Where(x => x.Type == ArchiveFileType.Xx3Mesh).OrderBy(x => x.Source.Md5, new ByteArrayComparer()));

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
                files.Where(x => x.Type != ArchiveFileType.Xx3Mesh).OrderBy(x => x.Source.Md5, new ByteArrayComparer()) //first sort all similar hashes together
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

        public void CompressCallback(object threadContext)
        {
            Stream ArchiveStream = threadContext as Stream;

            while (!QueuedChunks.IsCompleted)
            {
                bool result = QueuedChunks.TryTake(out HybridChunkWriter item, 1000);

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

        protected long WriteHeader(BinaryWriter dataWriter, bool writeNew)
        {
            //Write container header data
            dataWriter.Write(Encoding.ASCII.GetBytes(ExtendedArchive.Magic));

            dataWriter.Write(ExtendedArchive.Version);

            byte[] title = Encoding.Unicode.GetBytes(Title);

            dataWriter.Write((ushort)title.Length);
            dataWriter.Write(title);

            //Write individual file header + data
            long tableInfoOffset = dataWriter.BaseStream.Position;

            if (writeNew)
                //give a generous 1kb buffer for future edits
                dataWriter.Seek(16 + 1024, SeekOrigin.Current);

            return tableInfoOffset;
        }

        protected void internalWrite(Stream ArchiveStream, List<ISubfile> FilesAdding, long tableInfoOffset, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
            TextureBank = new TextureBank();

            QueuedChunks = new BlockingCollection<HybridChunkWriter>(Threads);

            threadProgress = ProgressStatus;

            threadObjects = new Thread[Threads];
            for (int i = 0; i < Threads; i++)
            {
                threadObjects[i] = new Thread(new ParameterizedThreadStart(CompressCallback));
                threadObjects[i].Start(ArchiveStream);
            }

            using (MemoryStream fileTableStream = new MemoryStream())
            using (BinaryWriter fileTableWriter = new BinaryWriter(fileTableStream))
            using (MemoryStream chunkTableStream = new MemoryStream())
            using (BinaryWriter chunkTableWriter = new BinaryWriter(chunkTableStream))
            using (BinaryWriter dataWriter = new BinaryWriter(ArchiveStream, Encoding.ASCII))
            {
                ulong chunkOffset = (ulong)dataWriter.BaseStream.Position;

                ProgressStatus.Report("Allocating chunks...\r\n");
                ProgressPercentage.Report(0);

                AllocateBlocking(FilesAdding, ProgressStatus, ProgressPercentage, QueuedChunks);

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

        public void Write(IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
            CompletedChunks = new List<ChunkReceipt>();


            List<ISubfile> FilesAdding = new List<ISubfile>(FilesToAdd);

            using (FileStream fs = new FileStream(BaseArchive.Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (BinaryWriter dataWriter = new BinaryWriter(fs))
            {
                long tableInfoOffset = WriteHeader(dataWriter, false);

                foreach (var chunk in BaseArchive.Chunks)
                {
                    if (IsChunkUnedited(chunk))
                    {
                        var chunkReciept = CopyChunk(chunk, out IList<ISubfile> AddedDupes);

                        CompletedChunks.Add(chunkReciept);

                        foreach (var dupe in AddedDupes)
                            FilesAdding.Remove(dupe);
                    }
                    else
                    {
                        FilesAdding.AddRange(chunk.Files.Where(x => !FilesToRemove.Contains(x.Source)));
                    }
                }

                ulong chunkOffset = BaseArchive.Chunks.Max(x => x.Offset + x.CompressedLength + 1);

                fs.Position = (long)chunkOffset;

                internalWrite(fs, FilesAdding, tableInfoOffset, ProgressStatus, ProgressPercentage);
            }
        }

        public void Write(string filename, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
#warning need to implement
            throw new NotImplementedException();
        }
    }
}
