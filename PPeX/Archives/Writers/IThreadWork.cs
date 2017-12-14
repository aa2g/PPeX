using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PPeX.Archives.Writers
{
    public interface IThreadWork : IDisposable
    {
        Stream GetData(IEnumerable<ICompressor> compressors);

        ChunkReceipt Receipt { get; }
    }
}
