using System;
using System.Buffers;
using PPeX;

namespace PPeXM64
{
    public class CachedFile
    {
        public CompressedCache Cache { get; protected set; }

        public ArchiveChunkCompression Compression { get; set; }

        //public PoolPointer CompressedData { get; set; }

        public Memory<byte>? CompressedData { get; set; }

        public Md5Hash Md5 { get; }

        public bool Ready => CompressedData != null;

        public int Accesses { get; protected set; }

        public long UncompressedSize { get; protected set; }

        public long Size => CompressedData?.Length ?? 0;

        public CachedFile(CompressedCache cache, Md5Hash md5, long uncompressedSize)
        {
            Cache = cache;
            Md5 = md5;
            UncompressedSize = uncompressedSize;

            Accesses = 0;
        }

        public void Deallocate()
        {
            //CompressedData.Release();
            CompressedData = null;

            Accesses = 0;
        }

        public IMemoryOwner<byte> GetMemory()
        {
	        //return CompressedData.GetReference();
            return new DummyMemoryOwner(CompressedData.Value);
        }

        private class DummyMemoryOwner : IMemoryOwner<byte>
        {
	        public Memory<byte> Memory { get; }

	        public DummyMemoryOwner(Memory<byte> memory)
	        {
		        Memory = memory;
	        }

	        public void Dispose() { }
        }
    }
}