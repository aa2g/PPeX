using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX;
using PPeXM64;
using System.IO;
using System.Text;
using System.Linq;

namespace PPeXTests
{
    [TestClass]
    public class CachedObjectTests
    {
        public static byte[] TestData = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");
        public static byte[] TestHash;
        public static ExtendedArchive TestArchive;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            FileStream arc = new FileStream("test.ppx", FileMode.Create);
            var writer = new ExtendedArchiveWriter(arc, "test", true);

            using (var mem = new MemoryStream(TestData))
                TestHash = PPeX.Utility.GetMd5(mem);

            writer.Files.Add(new Subfile(
                new MemorySource(TestData),
                    "test1",
                    "t",
                    ArchiveFileType.Raw));

            writer.Files.Add(new Subfile(
                new MemorySource(TestData),
                    "test2",
                    "t",
                    ArchiveFileType.Raw));

            writer.Write();

            arc.Close();

            TestArchive = new ExtendedArchive("test.ppx");
        }

        [TestMethod]
        public void AllocationTest()
        {
            CachedChunk chunk = new CachedChunk(TestArchive.Chunks[0]);

            chunk.Allocate();

            Assert.IsNotNull(chunk.Data);

            Assert.AreEqual((int)chunk.BaseChunk.UncompressedLength, (int)chunk.Data.Length);

            chunk.Deallocate();
        }

        [TestMethod]
        public void DeallocationTest()
        {
            CachedChunk chunk = new CachedChunk(TestArchive.Chunks[0]);

            chunk.Allocate();

            chunk.Deallocate();

            Assert.IsNull(chunk.Data);
        }

        [TestMethod]
        public void FileDataTest()
        {
            CachedChunk chunk = new CachedChunk(TestArchive.Chunks[0]);
            CachedFile file = new CachedFile(TestArchive.Files[0], chunk);

            using (MemoryStream mem = new MemoryStream())
            using (Stream decomp = file.GetStream())
            {
                decomp.CopyTo(mem);

                Assert.IsTrue(PPeX.Utility.CompareBytes(mem.ToArray(), TestData), "File data is not consistent.");
            }

            
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (File.Exists("test.ppx"))
                File.Delete("test.ppx");
        }
    }
}
