using PPeX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

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
        public ConcurrentDictionary<Md5Hash, CachedFile> LoadedFiles = new ConcurrentDictionary<Md5Hash, CachedFile>();
        public Dictionary<FileEntry, Md5Hash> ReferenceMd5Sums = new Dictionary<FileEntry, Md5Hash>();
        public Dictionary<FileEntry, CachedChunk> LoadedFileReferences = new Dictionary<FileEntry, CachedChunk>();

        public List<CachedChunk> TotalChunks = new List<CachedChunk>();

        public List<ExtendedArchive> LoadedArchives = new List<ExtendedArchive>();

        public Allocator Allocator { get; }

        public CompressedCache(IEnumerable<ExtendedArchive> Archives, long memoryPoolSize) : this(Archives, new Progress<string>(), memoryPoolSize)
        {

        }

        public CompressedCache(IEnumerable<ExtendedArchive> Archives, IProgress<string> Status, long memoryPoolSize)
        {
            Allocator = new Allocator(memoryPoolSize);

            foreach (ExtendedArchive archive in Archives)
            {
                foreach (var chunk in archive.Chunks)
                {
                    var cachedChunk = new CachedChunk(chunk, this);

                    TotalChunks.Add(cachedChunk);

                    foreach (var file in chunk.Files)
                    {
	                    var entry = new FileEntry(file.ArchiveName, file.EmulatedName);



                        ReferenceMd5Sums[entry] = file.RawSource.Md5;
	                    LoadedFileReferences[entry] = cachedChunk;
                    }
                }

                Status.Report("Loaded \"" + archive.Title + "\" (" + archive.Files.Count + " files)");

                LoadedArchives.Add(archive);
            }
        }

        public long AllocatedMemorySize => Allocator.GetTotalAllocatedSize();

        public object LoadLock = new object();

        /// <summary>
        /// Trims allocated memory using a set method.
        /// </summary>
        /// <param name="Method">The method to determine what to trim.</param>
        public void Trim(TrimMethod Method)
        {
            int freed = 0;
            long freedSize = 0;

            lock (LoadLock)
            {
	            foreach (var fileKv in LoadedFiles)
	            {
		            if (fileKv.Value.Ready && (
			            (Method == TrimMethod.ZeroAccesses && fileKv.Value.Accesses == 0)
                        || (Method == TrimMethod.OneAccess && (fileKv.Value.Accesses == 0 || fileKv.Value.Accesses == 1))
                        || Method == TrimMethod.All
                        ))
		            {
			            freed++;
			            freedSize += fileKv.Value.Size;

			            fileKv.Value.Deallocate();

			            LoadedFiles.Remove(fileKv.Key, out _);
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
                            IOrderedEnumerable<CachedFile> sortedFiles = LoadedFiles.Values.Where(x => x.Ready).OrderBy(x => x.Accesses);

                            long accumulatedSize = 0;

                            IEnumerable<CachedFile> removedFiles = sortedFiles
	                            .TakeWhile(x => (accumulatedSize += x.Size) < (loadedDiff - MaxSize))
	                            .ToArray();

                            foreach (var file in removedFiles)
                            {
	                            file.Deallocate();
	                            LoadedFiles.Remove(file.Md5, out _);
                            }

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
