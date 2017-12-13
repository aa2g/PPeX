using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public interface ICompressor : IDisposable
    {
        ArchiveChunkCompression Compression { get; }

        Stream GetStream(Stream input);

        long WriteToStream(Stream input, Stream output);
    }

    public interface IDecompressor : IDisposable
    {
        ArchiveChunkCompression Compression { get; }

        Stream Decompress(Stream input);
    }
}
