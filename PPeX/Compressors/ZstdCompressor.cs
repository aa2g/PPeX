using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Compressors
{
    public class ZstdCompressor : BaseCompressor
    {
        public override ArchiveChunkCompression Compression => ArchiveChunkCompression.Zstandard;

        protected ZstdNet.Compressor _compressor;

        public ZstdCompressor(int CompressionLevel)
        {
            _compressor = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(CompressionLevel));
        }

        public override long WriteToStream(Stream input, Stream output)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                input.CopyTo(mem);

                byte[] buffer = _compressor.Wrap(mem.ToArray());

                output.Write(buffer, 0, buffer.Length);

                return output.Length;
            }
        }

        public override void Dispose()
        {
            ((IDisposable)_compressor).Dispose();
        }
    }

    public class ZstdDecompressor : IDecompressor
    {
        public ArchiveChunkCompression Compression => ArchiveChunkCompression.Zstandard;

        protected ZstdNet.Decompressor _decompressor;

        public ZstdDecompressor()
        {
            _decompressor = new ZstdNet.Decompressor();
        }

        public Stream Decompress(Stream input)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                input.CopyTo(buffer);
                return new MemoryStream(_decompressor.Unwrap(buffer.ToArray()));
            }
        }

        public void Dispose()
        {
            ((IDisposable)_decompressor).Dispose();
        }
    }
}
