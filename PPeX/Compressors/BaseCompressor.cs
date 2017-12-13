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
        public abstract ArchiveChunkCompression Compression { get; }

        public abstract void Dispose();
        public abstract long WriteToStream(Stream input, Stream output);


        public virtual Stream GetStream(Stream input)
        {
            MemoryStream mem = new MemoryStream();

            WriteToStream(input, mem);

            mem.Position = 0;
            return mem;
        }
    }
}
