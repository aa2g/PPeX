using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Compressors
{
    public class Lz4Compressor : BaseCompressor
    {
        public override ArchiveChunkCompression Compression => ArchiveChunkCompression.LZ4;

        protected bool highCompression;

        public static int BlockSize = 4 * 1024 * 1024;

        public Lz4Compressor(Stream stream, bool HighCompression) : base(stream, (uint)stream.Length)
        {
            highCompression = HighCompression;
        }

        public override uint CompressedSize { get; protected set; }

        public override void WriteToStream(Stream stream)
        {
            long oldPos = stream.Position;

            var flags = LZ4.LZ4StreamFlags.IsolateInnerStream;

            if (highCompression)
                flags |= LZ4.LZ4StreamFlags.HighCompression;

            using (var lz4 = new LZ4.LZ4Stream(stream, LZ4.LZ4StreamMode.Compress, flags, BlockSize))
            {
                BaseStream.CopyTo(lz4);
                lz4.Close();
            }

            CompressedSize = (uint)(stream.Position - oldPos);
        }
    }

    public class Lz4Decompressor : IDecompressor
    {
        public ArchiveChunkCompression Compression => ArchiveChunkCompression.LZ4;

        public Stream BaseStream { get; protected set; }

        public Lz4Decompressor(Stream stream)
        {
            BaseStream = stream;
        }

        public Stream Decompress()
        {
            MemoryStream buffer = new MemoryStream();

            using (var lz4 = new LZ4.LZ4Stream(BaseStream, LZ4.LZ4StreamMode.Decompress))
                lz4.CopyTo(buffer);

            return buffer;
        }

        public void Dispose()
        {
            ((IDisposable)BaseStream).Dispose();
        }
    }
}
