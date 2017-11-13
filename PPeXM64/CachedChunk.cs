using PPeX;
using PPeX.Compressors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PPeXM64
{
    /// <summary>
    /// A cached version of a chunk to be kept in memory.
    /// </summary>
    public class CachedChunk
    {
        public uint ID => BaseChunk.ID;
        public CompressedCache BaseCache;
        public ExtendedArchiveChunk BaseChunk;
        public ArchiveChunkCompression RecompressionMethod = ArchiveChunkCompression.LZ4;

        public List<CachedFile> Files = new List<CachedFile>();

        public CachedChunk(ExtendedArchiveChunk baseChunk, CompressedCache cache)
        {
            BaseChunk = baseChunk;
            BaseCache = cache;

            foreach (var file in baseChunk.Files)
            {
                Files.Add(new CachedFile(file.Source as ArchiveFileSource, cache, this));
            }
        }

        public static readonly int RecompressionThreads = 4;

        protected Thread[] threads;
        protected BlockingCollection<Tuple<IEnumerable<CachedFile>, byte[]>> QueuedFileSets;

        protected void RecompressionCallback()
        {
            while (!QueuedFileSets.IsCompleted)
            {
                Tuple<IEnumerable<CachedFile>, byte[]> item;
                bool result = QueuedFileSets.TryTake(out item, 50);

                if (result)
                {
                    var possibleFiles = item.Item1;
                    var uncompressed = item.Item2;

                    //We don't want to recompress data that's already cached
                    if (possibleFiles.Any(x => !x.Allocated))
                    {
                        using (MemoryStream uncomp = new MemoryStream(uncompressed))
                        using (ICompressor compressor = CompressorFactory.GetCompressor(uncomp, RecompressionMethod))
                        using (MemoryStream compressed = new MemoryStream())
                        {
                            compressor.WriteToStream(compressed);

                            byte[] compressedArray = compressed.ToArray();

                            foreach (var file in possibleFiles)
                            {
                                file.CompressedData = compressedArray;
                                file.Compression = RecompressionMethod;
                            }
                        }
                    }
                }
            }
        }

        public void Allocate()
        {
            QueuedFileSets = new BlockingCollection<Tuple<IEnumerable<CachedFile>, byte[]>>();
            threads = new Thread[RecompressionThreads];

            for (int i = 0; i < RecompressionThreads; i++)
            {
                threads[i] = new Thread(new ThreadStart(RecompressionCallback));
                threads[i].Start();
            }

            using (Stream fstream = BaseChunk.GetStream())
            {
                while (fstream.Position < fstream.Length)
                {
                    var possibleFiles = Files.Where(x => (long)x.Source.Offset == fstream.Position).ToList();

                    int length = possibleFiles[0].Length;

                    byte[] uncompressed = new byte[length];

                    fstream.Read(uncompressed, 0, length);

                    QueuedFileSets.Add(new Tuple<IEnumerable<CachedFile>, byte[]>(possibleFiles, uncompressed));
                }

                QueuedFileSets.CompleteAdding();
            }

            //wait for threads
            for (int i = 0; i < RecompressionThreads; i++)
                threads[i].Join();
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
