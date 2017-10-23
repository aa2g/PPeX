using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXTests
{
    [TestClass]
    public class PP2Tests
    {
        public static ExtendedArchive TestArchive;
        public static byte[][] TestData = new byte[8][];

        [DeploymentItem("X64")]
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            //generate random data
            Random random = new Random();

            for (int i = 0; i < 8; i++)
            {
                TestData[i] = new byte[512];
                random.NextBytes(TestData[i]);
            }

            //create archive
            FileStream arc = new FileStream("test_pp2.ppx", FileMode.Create);
            var writer = new ExtendedArchiveWriter(arc, "test_pp2", true);

            writer.ChunkSizeLimit = 1024;

            //add files + duplicates
            for (int i = 0; i < 8; i++)
            {
                for (int d = 0; d < 4; d++)
                {
                    ISubfile file = new Subfile(new MemorySource(TestData[i]), "test" + i.ToString() + d.ToString(), "test", ArchiveFileType.Raw);

                    writer.Files.Add(file);
                }
            }

            writer.Write();

            arc.Close();

            TestArchive = new ExtendedArchive("test_pp2.ppx");
        }

        [TestMethod]
        public void DuplicateIndexTest()
        {
            Assert.AreEqual(8, TestArchive.Files.Count(x => x.LinkID == ArchiveFileSource.CanonLinkID), "Incorrect amount of canon files.");

            foreach (var file in TestArchive.Files)
            {
                if (file.LinkID != ArchiveFileSource.CanonLinkID)
                {
                    var index = TestArchive.Files[(int)file.LinkID];

                    Assert.IsTrue(index.LinkID == ArchiveFileSource.CanonLinkID, "Link ID links to an archive that is a duplicate.");

                    Assert.IsTrue(Utility.CompareBytes(index.Md5, file.Md5), "Link ID links to a file that does not have a matching hash.");
                }
            }
        }

        [TestMethod]
        public void ReverseChunkIndexTest()
        {
            Assert.AreEqual(512 * 8 / 1024, TestArchive.Chunks.Count, "Incorrect amount of chunks.");

            foreach (var chunk in TestArchive.Chunks)
            {
                Assert.IsTrue(chunk.GlobalFileIndex + chunk.LocalFileCount <= TestArchive.Files.Count, "Chunk local file index + count exceeds global file count.");

                for (int i = (int)chunk.GlobalFileIndex; i < chunk.GlobalFileIndex + chunk.LocalFileCount; i++)
                {
                    Assert.IsTrue(TestArchive.Files[i].ChunkID == chunk.ID, "Referenced file does not belong to this chunk.");
                }

                int index = 0;
                foreach (var file in TestArchive.Files)
                {
                    Assert.IsFalse(TestArchive.Files.Any(x => 
                        x.ChunkID == chunk.ID
                        && index < chunk.GlobalFileIndex
                        && index > chunk.GlobalFileIndex + chunk.LocalFileCount),
                        "Not all files in this chunk were indexed to this chunk.");

                    index++;
                }
            }
        }
    }
}
