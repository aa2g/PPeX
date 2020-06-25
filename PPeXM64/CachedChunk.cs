using System;
using PPeX;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PPeX.Compressors;

namespace PPeXM64
{
    /// <summary>
    /// A cached version of a chunk to be kept in memory.
    /// </summary>
    public class CachedChunk
    {
        public uint ID => BaseChunk.ID;
        public CompressedCache BaseCache;
        public ExtendedArchiveChunk BaseChunk;
        public static readonly ArchiveChunkCompression RecompressionMethod = ArchiveChunkCompression.Zstandard;

        public CachedChunk(ExtendedArchiveChunk baseChunk, CompressedCache cache)
        {
            BaseChunk = baseChunk;
            BaseCache = cache;
        }

        public void Allocate()
        {
	        var filesToAllocate = BaseChunk.Files
		        .Where(x => !BaseCache.LoadedFiles.ContainsKey(x.RawSource.Md5))
		        .Select(x => (Md5Hash)x.RawSource.Md5)
		        .Distinct()
		        .ToArray();

	        if (filesToAllocate.Length == 0)
		        return;

	        List<Task> recompressionTasks = new List<Task>();

	        var memoryBuffer = MemoryPool<byte>.Shared.Rent((int)BaseChunk.UncompressedLength);

            BaseChunk.CopyToMemory(memoryBuffer.Memory);

	        foreach (var hash in filesToAllocate)
	        {
		        var subfile = BaseChunk.Files.First(x => x.RawSource.Md5 == hash);

                recompressionTasks.Add(Task.Run(() =>
                {
                    var cachedFile = BaseCache.LoadedFiles.GetOrAdd(hash, new CachedFile(BaseCache, hash, (long)subfile.Size));

                    using var zstdCompressor = new ZstdCompressor();

                    using var compressedMemoryBuffer =
	                    MemoryPool<byte>.Shared.Rent(ZstdCompressor.GetUpperCompressionBound((int)subfile.Size));

                    var uncompressedSource = memoryBuffer.Memory.Slice((int)subfile.RawSource.Offset, (int)subfile.RawSource.Size);

                    zstdCompressor.CompressData(uncompressedSource.Span, compressedMemoryBuffer.Memory.Span, 3, out int compressedSize);


                    //var pointer = BaseCache.Allocator.Allocate(compressedSize);
                    //using var pointerBuffer = pointer.GetReference();
                    Memory<byte> compressedBuffer = new byte[compressedSize];

                    compressedMemoryBuffer.Memory.Slice(0, compressedSize).CopyTo(compressedBuffer); //pointerBuffer.Memory

                    //cachedFile.CompressedData = pointer;
                    cachedFile.CompressedData = compressedBuffer;
                }));
	        }

	        Task.WhenAll(recompressionTasks).ContinueWith(t =>
	        {
		        memoryBuffer.Dispose();
	        });
        }
    }
}