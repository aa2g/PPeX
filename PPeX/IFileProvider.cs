using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public interface IFileProvider
    {
        void Initialize(ICollection<ExtendedArchiveChunk> Chunks);

        ICollection<ISubfile> Files { get; }
    }
}
