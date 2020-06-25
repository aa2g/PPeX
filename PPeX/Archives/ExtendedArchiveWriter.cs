using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using PPeX.Common;
using PPeX.Compressors;
using PPeX.Encoders;
using PPeX.External.CRC32;

namespace PPeX
{
    /// <summary>
    /// A writer for extended archives, to .ppx files.
    /// </summary>
    public class ExtendedArchiveWriter
    {
        /// <summary>
        /// The display name of the archive.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of files that the archive contains.
        /// </summary>
        public List<ISubfile> Files { get; protected set; } = new List<ISubfile>();

        public Dictionary<ArchiveFileType, ArchiveFileType> EncodingConversions { get; protected set; } = new Dictionary<ArchiveFileType, ArchiveFileType>(Core.Settings.DefaultEncodingConversions);

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

        protected BlockingCollection<QueuedChunk> QueuedChunks;

        protected AsyncCollection<FinishedChunk> ReadyChunks;

        protected List<ChunkReceipt> CompletedChunks;

        /// <summary>
        /// Creates a new extended archive writer.
        /// </summary>
        /// <param name="File">The stream of the .ppx file to be written to.</param>
        /// <param name="Name">The display name for the archive.</param>
        public ExtendedArchiveWriter(string Name, bool LeaveOpen = false)
        {
            this.Name = Name;
            DefaultCompression = ArchiveChunkCompression.Zstandard;

            ChunkSizeLimit = 16 * 1024 * 1024;
            Threads = 1;

            leaveOpen = LeaveOpen;
        }

        protected async Task GenerateHashes(IList<ISubfile> files, IProgress<int> progressPercentage)
        {
            Dictionary<string, List<PPSource>> ppSubfiles = new Dictionary<string, List<PPSource>>();

            foreach (var file in files)
            {
	            if (file.Source is PPSource ppSource)
	            {
		            if (!ppSubfiles.TryGetValue(ppSource.Subfile.ppPath, out var subfileList))
		            {
			            subfileList = new List<PPSource>();
			            ppSubfiles[ppSource.Subfile.ppPath] = subfileList;
		            }

                    subfileList.Add(ppSource);
	            }
            }


            int i = 0;
            double total = ppSubfiles.Sum(x => x.Value.Count);

            foreach (var ppFile in ppSubfiles)
            {
                await using var fileStream = new FileStream(ppFile.Key, FileMode.Open, FileAccess.Read);

                foreach (var file in ppFile.Value)
                {
	                using var rentedMemory = MemoryPool<byte>.Shared.Rent((int)file.Subfile.size);

	                await using var substream = file.Subfile.CreateReadStream(fileStream);

	                int totalRead = 0;
	                int read = -1;
	                while (read != 0)
	                {
		                read = await substream.ReadAsync(rentedMemory.Memory.Slice(totalRead, (int)file.Subfile.size - totalRead));
		                totalRead += read;
	                }

					file.Md5 = Utility.GetMd5(rentedMemory.Memory.Span.Slice(0, (int)file.Subfile.size));

	                i++;

	                progressPercentage.Report((int)(i * 100 / total));
                }
            }
        }

        protected async Task AllocateBlocks(IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage, uint startingID = 0)
        {
            uint ID = startingID;

            ProgressStatus.Report("First pass hash caching...\r\n");

            await GenerateHashes(Files, ProgressPercentage);


            ProgressStatus.Report("Second pass writing...\r\n");

            ProgressStatus.Report("Allocating chunks...\r\n");
            ProgressPercentage.Report(0);

            //Create a LST chunk
            var lstChunk = new QueuedChunk(new List<ISubfile>(), ID++, DefaultCompression, 23);

            //bunch duplicate files together
            //going to assume OrderBy is a stable sort
            LinkedList<ISubfile> linkedSubfileList = new LinkedList<ISubfile>(
                Files
                .OrderBy(x => x.Name) //order by file name first
                .OrderBy(x => Path.GetExtension(x.Name) ?? x.Name)); //then we order by file type, preserving duplicate file order

            Dictionary<Md5Hash, LinkedListNode<ISubfile>> HashList = new Dictionary<Md5Hash, LinkedListNode<ISubfile>>();

            var node = linkedSubfileList.First;

            while (node?.Next != null)
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

            var fileList = new Queue<ISubfile>(linkedSubfileList);

            ulong accumulatedSize = 0;

            var currentChunk = new QueuedChunk(new List<ISubfile>(), ID++, DefaultCompression, 23);
            Md5Hash? previousHash = null;

            while (fileList.Count > 0)
            {
                ISubfile file = fileList.Dequeue();

                if (previousHash.HasValue && previousHash.Value == file.Source.Md5)
                {
                    currentChunk.Subfiles.Add(file);
                    continue;
                }

                previousHash = file.Source.Md5;

                accumulatedSize += file.Source.Size;

                if (file.Name.EndsWith(".lst"))
                {
                    lstChunk.Subfiles.Add(file);
                    continue;
                }

                if (file.Type == ArchiveFileType.WaveAudio || file.Type == ArchiveFileType.OpusAudio)
                {
                    //non-compressable data, assign it and any duplicates to a new chunk

                    List<ISubfile> opusFiles = new List<ISubfile>();

                    opusFiles.Add(file);

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

                        opusFiles.Add(fileList.Dequeue());
                    }

                    var tempChunk = new QueuedChunk(opusFiles, ID++, ArchiveChunkCompression.Uncompressed, 0);
                    QueuedChunks.Add(tempChunk);

                    continue;
                }

                if (file.Size + accumulatedSize > ChunkSizeLimit)
                {
                    //cut off the chunk here
                    QueuedChunks.Add(currentChunk);

                    accumulatedSize = 0;

                    //create a new chunk
                    currentChunk = new QueuedChunk(new List<ISubfile>(), ID++, DefaultCompression, 23);
                }

                currentChunk.Subfiles.Add(file);
            }

            if (currentChunk.Subfiles.Count > 0)
	            QueuedChunks.Add(currentChunk);

            if (lstChunk.Subfiles.Count > 0)
	            QueuedChunks.Add(lstChunk);

            QueuedChunks.CompleteAdding();
        }
        
        Thread[] threadObjects;
        private TaskCompletionSource<object>[] threadCompletionSources;
        IProgress<string> threadProgress;

        public void CompressCallback(int id)
        {
            using ZstdCompressor compressor = new ZstdCompressor();
            var completionSource = threadCompletionSources[id];

            try
            {
	            while (!QueuedChunks.IsCompleted)
	            {
		            if (QueuedChunks.TryTake(out var queuedChunk, 500))
		            {
			            var totalUncompressed = queuedChunk.Subfiles.Sum(x => (int)x.Size);
			            var upperBound = ZstdCompressor.GetUpperCompressionBound(totalUncompressed);

			            var uncompressedBuffer = MemoryPool<byte>.Shared.Rent(totalUncompressed);
			            int currentBufferIndex = 0;

			            List<FileReceipt> fileReceipts = new List<FileReceipt>();

			            foreach (var subfile in queuedChunk.Subfiles)
			            {
				            FileReceipt receipt;

				            if ((receipt = fileReceipts.Find(x => x.Md5 == subfile.Source.Md5)) != null)
				            {
					            receipt = FileReceipt.CreateDuplicate(receipt, subfile);

					            receipt.Filename = subfile.Name;
					            receipt.EmulatedName = subfile.Name;
					            receipt.ArchiveName = subfile.ArchiveName;
				            }
				            else if (subfile.RequestedConversion != null)
				            {
					            if (subfile.RequestedConversion.TargetEncoding != ArchiveFileType.OpusAudio)
						            throw new NotImplementedException("Only supports opus encoding at this time");

					            using var opusEncoder = new OpusEncoder();

					            using var inputStream = subfile.GetStream();
					            using var bufferStream = new MemorySpanStream(uncompressedBuffer.Memory.Slice(currentBufferIndex));

					            opusEncoder.Encode(inputStream, bufferStream);

					            receipt = new FileReceipt
					            {
						            Md5 = Utility.GetMd5(bufferStream.SliceToCurrentPosition().Span),
						            Length = (ulong)bufferStream.Position,
						            Offset = (ulong)0,
						            Filename = opusEncoder.RealNameTransform(subfile.Name),
						            EmulatedName = subfile.Name,
						            Encoding = ArchiveFileType.OpusAudio,
						            ArchiveName = subfile.ArchiveName,
						            Subfile = subfile
					            };

					            currentBufferIndex += (int)receipt.Length;
				            }
				            else
				            {
					            using var inputStream = subfile.GetStream();

					            int totalRead = 0;

					            while (totalRead < (int)subfile.Size)
					            {
						            int read = inputStream.Read(
							            uncompressedBuffer.Memory.Span.Slice(currentBufferIndex,
								            (int)subfile.Size - totalRead));

						            totalRead += read;
					            }

					            receipt = new FileReceipt
					            {
						            Md5 = subfile.Source.Md5,
						            Length = subfile.Size,
						            Offset = (ulong)currentBufferIndex,
						            Filename = subfile.Name,
						            EmulatedName = subfile.Name,
						            Encoding = subfile.Type,
						            ArchiveName = subfile.ArchiveName,
						            Subfile = subfile
					            };

					            currentBufferIndex += (int)receipt.Length;
				            }

				            fileReceipts.Add(receipt);
			            }

			            Memory<byte> uncompressedSpan = uncompressedBuffer.Memory.Slice(0, currentBufferIndex);

			            IMemoryOwner<byte> compressedBuffer; 
			            Memory<byte> compressedMemory;

			            if (queuedChunk.Compression == ArchiveChunkCompression.Zstandard)
			            {
				            compressedBuffer = MemoryPool<byte>.Shared.Rent(upperBound);

				            compressor.CompressData(uncompressedSpan.Span,
					            compressedBuffer.Memory.Span, queuedChunk.CompressionLevel, out int compressedSize);

				            compressedMemory = compressedBuffer.Memory.Slice(0, compressedSize);

                            uncompressedBuffer.Dispose();
			            }
			            else
			            {
				            compressedBuffer = uncompressedBuffer;
				            compressedMemory = uncompressedSpan;
			            }

                        uint crc = CRC32.Compute(compressedMemory.Span);

			            var chunkReceipt = new ChunkReceipt
			            {
				            ID = queuedChunk.ID,
				            Compression = queuedChunk.Compression,
				            CRC = crc,
				            UncompressedSize = (ulong)uncompressedSpan.Length,
				            CompressedSize = (ulong)compressedMemory.Length,
				            FileReceipts = fileReceipts
			            };

			            ReadyChunks.Add(new FinishedChunk
			            {
				            UnderlyingBuffer = compressedBuffer,
				            Data = compressedMemory,
				            Receipt = chunkReceipt
			            });

			            threadProgress.Report(
				            $"Compressed chunk id:{queuedChunk.ID} ({fileReceipts.Count} files) ({Utility.GetBytesReadable((long)chunkReceipt.CompressedSize)} - {(double)chunkReceipt.CompressedSize / chunkReceipt.UncompressedSize:P} ratio)\r\n");
		            }
                    else
					{
                        Thread.Sleep(50);
					}
	            }

	            completionSource.SetResult(null);
            }
            catch (Exception ex)
            {
	            completionSource.SetException(ex);
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
                foreach (var finishedChunk in CompletedChunks)
                {
                    WriteChunkTable(chunkTableWriter, finishedChunk);

                    WriteFileTable(fileTableWriter, finishedChunk);
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

        protected void InitializeThreads(IProgress<string> ProgressStatus)
        {
            QueuedChunks = new BlockingCollection<QueuedChunk>();
            ReadyChunks = new AsyncCollection<FinishedChunk>();

            CompletedChunks = new List<ChunkReceipt>();
            threadProgress = ProgressStatus;
            
            threadObjects = new Thread[Threads];
            threadCompletionSources = new TaskCompletionSource<object>[Threads];

            for (int i = 0; i < Threads; i++)
            {
                int capturedI = i;
                threadObjects[i] = new Thread(() => CompressCallback(capturedI));
                threadCompletionSources[i] = new TaskCompletionSource<object>();

                threadObjects[i].Start();
            }
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        public async Task WriteAsync(string filename)
        {
            IProgress<string> progress1 = new Progress<string>();
            IProgress<int> progress2 = new Progress<int>();

            await using FileStream fs = new FileStream(filename, FileMode.Create);
            await WriteAsync(fs, progress1, progress2);
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        public async Task WriteAsync(Stream ArchiveStream)
        {
            IProgress<string> progress1 = new Progress<string>();
            IProgress<int> progress2 = new Progress<int>();

            await WriteAsync(ArchiveStream, progress1, progress2);
        }

        /// <summary>
        /// Writes the archive to the .ppx file.
        /// </summary>
        /// <param name="progress">The progress callback object.</param>
        public async Task WriteAsync(Stream ArchiveStream, IProgress<string> ProgressStatus, IProgress<int> ProgressPercentage)
        {
            if (!ArchiveStream.CanSeek || !ArchiveStream.CanWrite)
                throw new ArgumentException("Stream must be seekable and able to be written to.", nameof(ArchiveStream));

            InitializeThreads(ProgressStatus);
            
            using (BinaryWriter dataWriter = new BinaryWriter(ArchiveStream, Encoding.ASCII, leaveOpen))
            {
                //Write header
                long tableInfoOffset = WriteHeader(dataWriter, true);

                ulong chunkOffset = (ulong)dataWriter.BaseStream.Position;

                var writeTask = Task.Factory.StartNew(async () =>
                {
	                int totalFiles = Files.Count;
	                int completedFiles = 0;

	                while (await ReadyChunks.OutputAvailableAsync().ConfigureAwait(false))
	                {
		                var chunk = await ReadyChunks.TakeAsync().ConfigureAwait(false);

		                chunk.Receipt.FileOffset = (ulong)ArchiveStream.Position;

		                await ArchiveStream.WriteAsync(chunk.Data).ConfigureAwait(false);

                        CompletedChunks.Add(chunk.Receipt);

                        chunk.Free();

                        completedFiles += chunk.Receipt.FileReceipts.Count;
                        ProgressPercentage.Report(completedFiles * 100 / totalFiles);
	                }
                }, TaskCreationOptions.LongRunning);

                ProgressStatus.Report("Allocating chunks...\r\n");
                ProgressPercentage.Report(0);

                await AllocateBlocks(ProgressStatus, ProgressPercentage);

                await Task.WhenAll(threadCompletionSources.Select(x => x.Task));

                ReadyChunks.CompleteAdding();

                await writeTask;

                WriteTables(tableInfoOffset, dataWriter);
            }

            //Collect garbage and compress memory
            Utility.GCCompress();

            ProgressStatus.Report("Finished.\r\n");
            ProgressPercentage.Report(100);
        }

        protected static void WriteChunkTable(BinaryWriter chunkTableWriter, ChunkReceipt receipt)
        {
            chunkTableWriter.Write(receipt.ID);

            chunkTableWriter.Write((byte)receipt.Compression);

            chunkTableWriter.Write(receipt.CRC);

            chunkTableWriter.Write(receipt.FileOffset);
            chunkTableWriter.Write(receipt.CompressedSize);
            chunkTableWriter.Write(receipt.UncompressedSize);
        }

        protected static void WriteFileTable(BinaryWriter fileTableWriter, ChunkReceipt chunkReceipt)
        {
            int index = 0;
            foreach (var receipt in chunkReceipt.FileReceipts)
            {
                //write each file metadata
                byte[] archive_name = Encoding.Unicode.GetBytes(receipt.Subfile.ArchiveName);

                fileTableWriter.Write((ushort)archive_name.Length);
                fileTableWriter.Write(archive_name);


                byte[] file_name = Encoding.Unicode.GetBytes(receipt.Filename);

                fileTableWriter.Write((ushort)file_name.Length);
                fileTableWriter.Write(file_name);


                byte[] emulated_file_name = Encoding.Unicode.GetBytes(receipt.EmulatedName);

                fileTableWriter.Write((ushort)emulated_file_name.Length);
                fileTableWriter.Write(emulated_file_name);



                fileTableWriter.Write((ushort)receipt.Encoding);
                fileTableWriter.Write(receipt.Md5);


                fileTableWriter.Write(chunkReceipt.ID);
                fileTableWriter.Write(receipt.Offset);
                fileTableWriter.Write(receipt.Length);

                index++;
            }
        }

        protected class QueuedChunk
        {
            public IList<ISubfile> Subfiles;
            public uint ID;
            public ArchiveChunkCompression Compression;
            public int CompressionLevel;

            public QueuedChunk(IList<ISubfile> subfiles, uint id, ArchiveChunkCompression compression, int compressionLevel)
            {
	            Subfiles = subfiles;
	            ID = id;
	            Compression = compression;
	            CompressionLevel = compressionLevel;
            }
        }

        protected class FinishedChunk
        {
            public IMemoryOwner<byte> UnderlyingBuffer;
            public Memory<byte> Data;
            public ChunkReceipt Receipt;

            public void Free()
            {
                UnderlyingBuffer.Dispose();
            }
        }

        protected class FileReceipt
        {
	        public ISubfile Subfile;

	        public Md5Hash Md5;
	        public ulong Offset;
	        public ulong Length;

	        public string ArchiveName;

	        public string Filename;
	        public string EmulatedName;

	        public ArchiveFileType Encoding;

	        public static FileReceipt CreateDuplicate(FileReceipt original, ISubfile subfile)
	        {
		        FileReceipt receipt = original.MemberwiseClone() as FileReceipt;

		        receipt.Subfile = subfile;

		        return receipt;
	        }
        }

        protected class ChunkReceipt
        {
	        public uint ID;
	        public ArchiveChunkCompression Compression;
	        public uint CRC;
	        public ulong FileOffset;
	        public ulong CompressedSize;
	        public ulong UncompressedSize;

	        public ICollection<FileReceipt> FileReceipts;
        }
    }
}