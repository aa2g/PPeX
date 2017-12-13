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
        protected int blockSize;

        public Lz4Compressor(bool HighCompression, int BlockSize = 4 * 1024 * 1024)
        {
            highCompression = HighCompression;
            blockSize = BlockSize;
        }

        public override long WriteToStream(Stream input, Stream stream)
        {
            long oldPos = stream.Position;

            var flags = LZ4.LZ4StreamFlags.IsolateInnerStream;

            if (highCompression)
                flags |= LZ4.LZ4StreamFlags.HighCompression;

            using (var lz4 = new LZ4.LZ4Stream(stream, LZ4.LZ4StreamMode.Compress, flags, blockSize))
            {
                input.CopyTo(lz4);
                lz4.Close();
            }

            return stream.Position - oldPos;
        }

        public override void Dispose()
        {
            //nothing to dispose
        }
    }

    public class Lz4Decompressor : IDecompressor
    {
        public ArchiveChunkCompression Compression => ArchiveChunkCompression.LZ4;

        public Stream Decompress(Stream input)
        {
            MemoryStream buffer = new MemoryStream();

            using (var lz4 = new LZ4.LZ4Stream(input, LZ4.LZ4StreamMode.Decompress))
                lz4.CopyTo(buffer);

            buffer.Position = 0;

            return buffer;
        }

        public void Dispose()
        {
            //nothing to dispose
        }
    }
}
