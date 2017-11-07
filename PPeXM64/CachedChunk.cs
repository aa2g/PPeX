using PPeX;
using PPeX.Compressors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXM64
{
    /// <summary>
    /// A cached version of a chunk to be kept in memory.
    /// </summary>
    public class CachedChunk
    {
        public uint ID => BaseChunk.ID;
        public ExtendedArchiveChunk BaseChunk;
        public ArchiveChunkCompression RecompressionMethod = ArchiveChunkCompression.LZ4;

        public List<CachedFile> Files = new List<CachedFile>();

        public CachedChunk(ExtendedArchiveChunk baseChunk)
        {
            BaseChunk = baseChunk;

            foreach (var file in baseChunk.Files)
            {
                Files.Add(new CachedFile(file.Source as ArchiveFileSource, this));
            }
        }

        public void Allocate()
        {
            using (Stream fstream = BaseChunk.GetStream())
            {
                while (fstream.Position < fstream.Length)
                {
                    var possibleFiles = Files.Where(x => (long)x.Source.Offset == fstream.Position).ToList();

                    int length = possibleFiles[0].Length;

                    byte[] uncompressed = new byte[length];

                    fstream.Read(uncompressed, 0, length);

                    //We don't want to recompress data that's already cached
                    if (possibleFiles.Any(x => !x.Allocated))
                    {
                        using (MemoryStream uncomp = new MemoryStream(uncompressed))
                        using (ICompressor compressor = CompressorFactory.GetCompressor(uncomp, RecompressionMethod))
                        using (MemoryStream compressed = new MemoryStream())
                        {
                            compressor.WriteToStream(compressed);

                            foreach (var file in possibleFiles)
                            {
                                file.CompressedData = compressed.ToArray();
                                file.Compression = RecompressionMethod;
                            }
                        }
                    }
                }
            }
        }

        public void Deallocate()
        {
            foreach (var file in Files)
            {
                file.Deallocate();
            }
        }
    }
}
