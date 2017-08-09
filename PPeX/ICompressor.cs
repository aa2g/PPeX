﻿using System;
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

        uint UncompressedSize { get; }

        uint CompressedSize { get; }

        void WriteToStream(Stream stream);
    }

    public interface IDecompressor : IDisposable
    {
        ArchiveChunkCompression Compression { get; }

        Stream Decompress();
    }
}
