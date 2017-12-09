using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Archives.Writers
{
    public class HybridEncoder : IThreadWork
    {
        internal HybridChunkWriter chunkWriter;
        protected IEnumerable<ISubfile> files;

        public HybridEncoder(uint ID, IEnumerable<ISubfile> files, IArchiveContainer writer)
        {
            chunkWriter = new HybridChunkWriter(ID, ArchiveChunkCompression.Uncompressed, ChunkType.Generic, writer);

            this.files = files;
        }

        public ChunkReceipt Receipt => chunkWriter.Receipt;

        public void Dispose()
        {
            chunkWriter.Dispose();
        }

        public Stream GetData()
        {
            foreach (ISubfile file in files)
                chunkWriter.AddFile(file);

            chunkWriter.Compress();
            return chunkWriter.CompressedStream;
        }
    }
}
