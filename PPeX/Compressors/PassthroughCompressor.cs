using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Compressors
{
    public class PassthroughCompressor : BaseCompressor, IDecompressor
    {
        public PassthroughCompressor(Stream stream) : base(stream, (uint)stream.Length)
        {
            CompressedSize = (uint)BaseStream.Length;
        }

        public override uint CompressedSize { get; protected set; }

        public Stream Decompress()
        {
            return BaseStream;
        }

        public override void WriteToStream(Stream stream)
        {
            BaseStream.CopyTo(stream);
        }
    }
}
