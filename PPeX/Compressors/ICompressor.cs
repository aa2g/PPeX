using System;

namespace PPeX
{
    public interface ICompressor : IDisposable
    {
        ArchiveChunkCompression Compression { get; }

        void CompressData(ReadOnlySpan<byte> input, Span<byte> destinationBuffer, int compressionLevel, out int actualSize);
    }

    public interface IDecompressor : IDisposable
    {
        ArchiveChunkCompression Compression { get; }

        void DecompressData(ReadOnlySpan<byte> input, Span<byte> destinationBuffer, out int actualSize);
        Memory<byte> DecompressData(Span<byte> input);
    }
}