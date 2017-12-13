using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Compressors
{
    public static class CompressorFactory
    {
        public static ICompressor GetCompressor(ArchiveChunkCompression compression)
        {
            switch (compression)
            {
                case ArchiveChunkCompression.Zstandard:
                    return new ZstdCompressor(Core.Settings.ZstdCompressionLevel);
                case ArchiveChunkCompression.LZ4:
                    return new Lz4Compressor(false);
                case ArchiveChunkCompression.Uncompressed:
                    return new PassthroughCompressor();
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }

        public static IDecompressor GetDecompressor(ArchiveChunkCompression compression)
        {
            switch (compression)
            {
                case ArchiveChunkCompression.Zstandard:
                    return new ZstdDecompressor();
                case ArchiveChunkCompression.LZ4:
                    return new Lz4Decompressor();
                case ArchiveChunkCompression.Uncompressed:
                    return new PassthroughCompressor();
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }
    }
}
