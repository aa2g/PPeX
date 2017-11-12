using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Xx2;

namespace PPeX
{
    public class GenericProvider
    {
        public static IEnumerable<ArchiveSubfile> Load(IList<ExtendedArchiveChunk> Chunks)
        {
            List<ExtendedArchiveChunk> genericChunks = Chunks.Where(x => x.Type == ChunkType.Generic).ToList();

            return genericChunks.SelectMany(x => x.Files);
        }
    }
}
