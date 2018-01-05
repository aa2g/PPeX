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

        public bool WastedSpaceExists
        {
            get
            {
                var orderedChunks = BaseArchive.Chunks.OrderBy(x => x.Offset);
                ulong expectedOffset = orderedChunks.First().Offset;

                foreach (var chunk in orderedChunks)
                {
                    if (chunk.Offset != expectedOffset)
                        return true;

                    expectedOffset += chunk.CompressedLength;
                }

                return false;
            }
        }

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

        protected void ResetAppender()
        {
            BaseArchive = new ExtendedArchive(BaseArchive.Filename);
            FilesToAdd.Clear();
            FilesToRemove.Clear();
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
                    Subfile = new ArchiveSubfile(fileSource),
                    InternalName = fileSource.Name,
                    EmulatedName = fileSource.EmulatedName
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
                InitializeThreads(Threads, fs, DefaultCompression, ProgressStatus);

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

                AllocateBlocking(FilesAdding, ProgressStatus, ProgressPercentage, QueuedChunks, BaseArchive.Chunks.Select(x => x.ID).Max() + 1);

                WaitForThreadCompletion();

                WriteTables(tableInfoOffset, dataWriter);

                FinalizeThreads();

                Utility.GCCompress();

                ProgressStatus.Report("Finished.\r\n");
                ProgressPercentage.Report(100);
            }

            ResetAppender();
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

        public void Defragment()
        {
            CompletedChunks = new List<ChunkReceipt>();

            using (FileStream fs = new FileStream(BaseArchive.Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (BinaryReader dataReader = new BinaryReader(fs))
            using (BinaryWriter dataWriter = new BinaryWriter(fs))
            {
                var orderedChunks = BaseArchive.Chunks.OrderBy(x => x.Offset);
                ulong expectedOffset = orderedChunks.First().Offset;

                foreach (var chunk in orderedChunks)
                {
                    ChunkReceipt reciept = CopyChunk(chunk, out IList<ISubfile> AddedDupes);

                    if (chunk.Offset != expectedOffset)
                    {
                        fs.Position = (long)chunk.Offset;

                        using (MemoryStream buffer = new MemoryStream(dataReader.ReadBytes((int)chunk.CompressedLength)))
                        {
                            fs.Position = (long)expectedOffset;
                            buffer.CopyTo(fs);
                        }

                        reciept.FileOffset = expectedOffset;
                    }

                    CompletedChunks.Add(reciept);

                    expectedOffset += chunk.CompressedLength;
                }

                WriteTables((long)BaseArchive.TableInfoOffset, dataWriter);
                
                fs.SetLength(fs.Position);
            }

            ResetAppender();
        }
    }
}
