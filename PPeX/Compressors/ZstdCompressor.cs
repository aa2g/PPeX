using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Compressors
{
    public class ZstdCompressor : BaseCompressor, IDisposable
    {
        public override ArchiveFileCompression Compression => ArchiveFileCompression.Zstandard;

        protected ZstdNet.Compressor _compressor;

        public ZstdCompressor(Stream stream, int CompressionLevel) : base(stream, (uint)stream.Length)
        {
            _compressor = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(CompressionLevel));
        }

        public override uint CompressedSize { get; protected set; }

        public override void WriteToStream(Stream stream)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                BaseStream.CopyTo(mem);

                using (MemoryStream output = new MemoryStream(_compressor.Wrap(mem.ToArray())))
                {
                    output.CopyTo(stream);
                    CompressedSize = (uint)output.Length;
                }
            }
        }

        public override void Dispose()
        {
            ((IDisposable)_compressor).Dispose();
            base.Dispose();
        }
    }

    public class ZstdDecompressor : IDecompressor
    {
        public ArchiveFileCompression Compression => ArchiveFileCompression.Zstandard;

        protected ZstdNet.Decompressor _decompressor;

        public Stream BaseStream { get; protected set; }

        public ZstdDecompressor(Stream stream)
        {
            _decompressor = new ZstdNet.Decompressor();
            BaseStream = stream;
        }

        public Stream Decompress()
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                BaseStream.CopyTo(buffer);
                return new MemoryStream(_decompressor.Unwrap(buffer.ToArray()));
            }
        }

        public void Dispose()
        {
            ((IDisposable)_decompressor).Dispose();
            ((IDisposable)BaseStream).Dispose();
        }
    }
}
