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
        public override ArchiveChunkCompression Compression => ArchiveChunkCompression.Uncompressed;

        public override long WriteToStream(Stream input, Stream output)
        {
            long outputpos = output.Position;
            input.CopyTo(output);

            return output.Position - outputpos;
        }

        public Stream Decompress(Stream input)
        {
            return input;
        }

        public override void Dispose()
        {
            //nothing to dispose
        }
    }
}
