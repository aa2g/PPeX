using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PPeX.Xx2;
using PPeX.Archives;

namespace PPeX
{
    public class ExtendedArchiveAppender : ExtendedArchiveWriter
    {
        public ExtendedArchive BaseArchive { get; protected set; }

        public List<ISubfile> FilesToAdd { get; protected set; }

        public List<ArchiveFileSource> FilesToRemove { get; protected set; }

        Thread[] threadObjects;
        IProgress<string> threadProgress;

        public ExtendedArchiveAppender(ExtendedArchive archive) : base(archive.Title)
        {
            BaseArchive = archive;
            FilesToAdd = new List<ISubfile>();
            FilesToRemove = new List<ArchiveFileSource>();
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
                CRC = chunk.CRC32,
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

        public void Write(IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
            List<ISubfile> FilesAdding = new List<ISubfile>(FilesToAdd);

            using (FileStream fs = new FileStream(BaseArchive.Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (BinaryWriter dataWriter = new BinaryWriter(fs))
            {
                InitializeThreads(Threads, fs, ProgressStatus);

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

                AllocateBlocking(FilesAdding, ProgressStatus, ProgressPercentage, QueuedChunks);

                WaitForThreadCompletion();

                using (BinaryWriter fileTableWriter = new BinaryWriter(new MemoryStream()))
                using (BinaryWriter chunkTableWriter = new BinaryWriter(new MemoryStream()))
                {
                    WriteTables(tableInfoOffset, chunkTableWriter, fileTableWriter, dataWriter);
                }

                Utility.GCCompress();

                ProgressStatus.Report("Finished.\r\n");
                ProgressPercentage.Report(100);
            }
        }

        public void Write()
        {
            Write(new Progress<string>(), new Progress<int>());
        }

        public void Write(string filename, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
#warning need to implement
            throw new NotImplementedException();
        }
    }
}
