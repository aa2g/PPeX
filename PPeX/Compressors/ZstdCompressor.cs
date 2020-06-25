using System;

namespace PPeX.Compressors
{
    public class ZstdCompressor : ICompressor
    {
        public ArchiveChunkCompression Compression => ArchiveChunkCompression.Zstandard;

        protected External.Zstandard.ZstdCompressor _compressor;

        public ZstdCompressor()
        {
	        _compressor = new External.Zstandard.ZstdCompressor();
        }

        public void CompressData(ReadOnlySpan<byte> input, Span<byte> destinationBuffer, int compressionLevel, out int actualSize)
        {
	        actualSize = _compressor.Wrap(input, destinationBuffer, compressionLevel);
        }

        public void Dispose()
        {
	        _compressor?.Dispose();
	        _compressor = null;
        }

        ~ZstdCompressor()
        {
	        Dispose();
        }

        public static int GetUpperCompressionBound(int uncompressedSize)
        {
	        return External.Zstandard.ZstdCompressor.GetCompressBound(uncompressedSize);
        }
    }

    public class ZstdDecompressor : IDecompressor
    {
        public ArchiveChunkCompression Compression => ArchiveChunkCompression.Zstandard;

        protected External.Zstandard.ZstdDecompressor _decompressor;

        public ZstdDecompressor()
        {
	        _decompressor = new External.Zstandard.ZstdDecompressor();
        }

        public void DecompressData(ReadOnlySpan<byte> input, Span<byte> destinationBuffer, out int actualSize)
        {
	        actualSize = _decompressor.Unwrap(input, destinationBuffer);
        }

        public Memory<byte> DecompressData(Span<byte> input)
        {
	        return _decompressor.Unwrap(input);
        }

        public void Dispose()
        {
	        _decompressor?.Dispose();
	        _decompressor = null;
        }

        ~ZstdDecompressor()
        {
            Dispose();
        }
    }
}