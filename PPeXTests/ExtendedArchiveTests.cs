using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Tests
{
    [TestClass()]
    public class ExtendedArchiveTests
    {
        public static byte[] TestData = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");
        public static byte[] TestData2 = Encoding.UTF8.GetBytes("orem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");
    public static byte[] TestHash;
        public static ExtendedArchive TestArchive;

        [DeploymentItem("X64")]
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            FileStream arc = new FileStream("test.ppx", FileMode.Create);
            var writer = new ExtendedArchiveWriter(arc, "test", true);
            
            using (var mem = new MemoryStream(TestData))
                TestHash = Utility.GetMd5(mem);

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

            writer.Files.Add(new Subfile(
                new MemorySource(TestData2),
                    "test3",
                    "t",
                    ArchiveFileType.Raw));

            writer.Write();

            arc.Close();

            TestArchive = new ExtendedArchive("test.ppx");
        }

        [TestMethod]
        public void ArchiveMetadataTest()
        {
            Assert.AreEqual("test", TestArchive.Title, "Archive title is incorrect.");

            Assert.AreEqual(3, TestArchive.Files.Count, "File count is incorrect.");

            Assert.AreEqual(1, TestArchive.Chunks.Count, "Chunk count is incorrect.");

            Assert.IsTrue(TestArchive.Files.Any(x => x.Name == "test1"), "Archive does not contain file \"test1\".");

            Assert.IsTrue(TestArchive.Files.Any(x => x.Name == "test2"), "Archive does not contain file \"test2\".");

            Assert.IsTrue(TestArchive.Files.Any(x => x.Name == "test3"), "Archive does not contain file \"test3\".");
        }

        [TestMethod]
        public void ChunkChecksumTest()
        {
            int failed = 0;

            foreach (var chunk in TestArchive.Chunks)
            {
                if (!chunk.VerifyChecksum())
                    failed++;
            }

            Assert.IsTrue(failed == 0, "Chunk checksum does not match data. (" + failed + " / " + TestArchive.Chunks.Count + " failed)");
        }

        [TestMethod]
        public void ChunkOffsetTest()
        {
            int failed = 0;

            foreach (var chunk in TestArchive.Chunks)
            foreach (var file in chunk.Files)
            {
                if ((file.Source as ArchiveFileSource).Offset + file.Size > chunk.UncompressedLength)
                    failed++;
            }

            Assert.IsTrue(failed == 0, "Chunk checksum does not match data. (" + failed + " / " + TestArchive.Chunks.Count + " failed)");
        }

        [TestMethod]
        public void FileHashTest()
        {
            int failed = 0;

            foreach (var file in TestArchive.Files)
            {
                if (!Utility.CompareBytes(file.Md5, TestHash))
                    failed++;
            }

            Assert.IsTrue(failed == 0, "File hash is not consistent. (" + failed + " / " + TestArchive.Files.Count + " failed)");
        }

        [TestMethod]
        public void FileDataTest()
        {
            int failed = 0;

            foreach (var file in TestArchive.Files)
            {
                using (MemoryStream mem = new MemoryStream())
                using (Stream decomp = file.GetStream())
                {
                    decomp.CopyTo(mem);

                    if (!Utility.CompareBytes(mem.ToArray(), TestData))
                        failed++;
                }
            }

            Assert.IsTrue(failed == 0, "File data is not consistent. (" + failed + " / " + TestArchive.Files.Count + " failed)");
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (File.Exists("test.ppx"))
                File.Delete("test.ppx");
        }
    }
}