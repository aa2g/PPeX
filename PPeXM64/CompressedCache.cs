using PPeX;
using PPeX.Xx2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace PPeXM64
{
    public enum TrimMethod
    {
        GCCompactOnly,
        ZeroAccesses,
        OneAccess,
        All
    }

    public class CompressedCache
    {
        public Dictionary<FileEntry, CachedFile> LoadedFiles = new Dictionary<FileEntry, CachedFile>();

        public List<CachedChunk> TotalChunks = new List<CachedChunk>();
        public List<CachedFile> TotalFiles = new List<CachedFile>();

        public List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();

        public CompressedTextureBank UniversalTexBank;

        public CompressedCache(IEnumerable<ExtendedArchive> Archives) : this(Archives, new Progress<string>())
        {

        }

        protected void LoadTextures(Xx3Provider provider)
        {
            var chunks = provider.TextureFiles
                .Select(x => (x.Value.Source as ArchiveFileSource).Chunk)
                .Distinct();

            foreach (var chunk in chunks)
            {
                CachedChunk cachedChunk = new CachedChunk(chunk, this);

                cachedChunk.Allocate();

                foreach (var file in cachedChunk.Files)
                {
                    UniversalTexBank.AddRaw(file.Name, file.CompressedData);
                }

                cachedChunk.Deallocate();
            }
        }

        public CompressedCache(IEnumerable<ExtendedArchive> Archives, IProgress<string> Status)
        {
            UniversalTexBank = new CompressedTextureBank(CachedChunk.RecompressionMethod);

            foreach (ExtendedArchive archive in Archives)
            {
                LoadTextures(archive.Xx3Provider);

                foreach (var chunk in archive.Chunks)
                {
                    var cachedChunk = new CachedChunk(chunk, this);

                    TotalChunks.Add(cachedChunk);

                    foreach (var file in cachedChunk.Files)
                    {
                        TotalFiles.Add(file);

                        string name = PPeX.Encoders.EncoderFactory.GetEncoder(System.IO.Stream.Null, archive, file.Type).NameTransform(file.Name);

                        LoadedFiles[new FileEntry(file.ArchiveName, name)] = file;
                    }
                }

                Status.Report("Loaded \"" + archive.Title + "\" (" + archive.Files.Count + " files)");

                LoadedArchives.Add(archive);
            }
        }

        public long AllocatedMemorySize => TotalFiles.Where(x => x.Allocated).Sum(x => x.CompressedData.LongLength);

        public object LoadLock = new object();

        /// <summary>
        /// Trims allocated memory using a set method.
        /// </summary>
        /// <param name="Method">The method to determine what to trim.</param>
        public void Trim(TrimMethod Method)
        {
            int freed = 0;
            int freedSize = 0;

            lock (LoadLock)
            {
                if (Method == TrimMethod.ZeroAccesses)
                {
                    foreach (var file in TotalFiles)
                    {
                        if (file.Allocated)
                            if (file.Accesses == 0)
                            {
                                freed++;
                                freedSize += file.CompressedData.Length;

                                file.Deallocate();
                            }
                    }
                }
                else if (Method == TrimMethod.OneAccess)
                {
                    foreach (var file in TotalFiles)
                    {
                        if (file.Allocated)
                            if (file.Accesses == 0 | file.Accesses == 1)
                            {
                                freed++;
                                freedSize += file.CompressedData.Length;

                                file.Deallocate();
                            }
                    }
                }
                else if (Method == TrimMethod.All)
                {
                    foreach (var file in TotalFiles)
                    {
                        file.Deallocate();
                    }
                }

                Console.WriteLine("Freed " + freed + " file(s) (" + Utility.GetBytesReadable(freedSize) + ")");
            }

            //Collect garbage and compact the heap
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        /// <summary>
        /// Trims memory using a generation-based prioritizer.
        /// </summary>
        /// <param name="MaxSize">The maximum allowed size of cached data.</param>
        public void Trim(long MaxSize)
        {
            lock (LoadLock)
            {
                long loadedDiff = AllocatedMemorySize;
                
                if (loadedDiff > MaxSize)
                {
                    Trim(TrimMethod.ZeroAccesses);

                    if (loadedDiff > MaxSize)
                    {
                        Trim(TrimMethod.OneAccess);

                        if (loadedDiff > MaxSize)
                        {
                            IOrderedEnumerable<CachedFile> sortedFiles = TotalFiles.Where(x => x.Allocated).OrderBy(x => x.Accesses);

                            long accumulatedSize = 0;

                            IEnumerable<CachedFile> removedFiles = sortedFiles.TakeWhile(x => (accumulatedSize += x.CompressedData.Length) < (loadedDiff - MaxSize));

                            foreach (var file in removedFiles)
                                file.Deallocate();

                            Console.WriteLine("Freed " + removedFiles.Count() + " files(s) (" + Utility.GetBytesReadable(accumulatedSize) + ")");
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("Freed 0 files(s) (0 B)");
                }
            }

            //Collect garbage and compact the heap
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
    }
}
