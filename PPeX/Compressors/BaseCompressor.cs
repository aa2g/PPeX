using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Compressors
{
    public abstract class BaseCompressor : ICompressor
    {
        public Stream BaseStream { get; protected set; }

        public BaseCompressor(Stream baseStream, uint UncompressedSize)
        {
            this.BaseStream = baseStream;
            this.UncompressedSize = UncompressedSize;
        }

        public abstract uint CompressedSize { get; protected set; }

        public uint UncompressedSize { get; protected set; }

        public abstract ArchiveChunkCompression Compression { get; }

        public abstract void WriteToStream(Stream stream);

        public virtual void Dispose()
        {
            ((IDisposable)BaseStream).Dispose();
        }
    }

    public static class CompressorFactory
    {
        public static ICompressor GetCompressor(Stream stream, ArchiveChunkCompression compression)
        {
            switch (compression)
            {
                case ArchiveChunkCompression.Zstandard:
                    return new ZstdCompressor(stream, 8);
                case ArchiveChunkCompression.LZ4:
                    return new Lz4Compressor(stream, true);
                case ArchiveChunkCompression.Uncompressed:
                    return new PassthroughCompressor(stream);
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }

        public static IDecompressor GetDecompressor(Stream stream, ArchiveChunkCompression compression)
        {
            switch (compression)
            {
                case ArchiveChunkCompression.Zstandard:
                    return new ZstdDecompressor(stream);
                case ArchiveChunkCompression.LZ4:
                    return new Lz4Decompressor(stream);
                case ArchiveChunkCompression.Uncompressed:
                    return new PassthroughCompressor(stream);
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }
    }
}
