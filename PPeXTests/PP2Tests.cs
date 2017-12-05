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
        [TestMethod]
        public void DuplicateIndexTest()
        {
            ExtendedArchive TestArchive = TestCommon.GeneratePPX();

            Assert.AreEqual(8, TestArchive.RawFiles.Count(x => x.LinkID == ArchiveFileSource.CanonLinkID), "Incorrect amount of canon files.");

            foreach (var file in TestArchive.RawFiles)
            {
                if (file.LinkID != ArchiveFileSource.CanonLinkID)
                {
                    var index = TestArchive.RawFiles[(int)file.LinkID];

                    Assert.IsTrue(index.LinkID == ArchiveFileSource.CanonLinkID, "Link ID links to an archive that is a duplicate.");

                    Assert.IsTrue(Utility.CompareBytes(index.Md5, file.Md5), "Link ID links to a file that does not have a matching hash.");
                }
            }

            TestCommon.TeardownPPX(TestArchive);
        }

        [TestMethod]
        public void ReverseChunkIndexTest()
        {
            ExtendedArchive TestArchive = TestCommon.GeneratePPX();

            Assert.AreEqual(512 * 8 / 1024, TestArchive.Chunks.Count, "Incorrect amount of chunks.");

            foreach (var chunk in TestArchive.Chunks)
            {
                Assert.IsTrue(chunk.GlobalFileIndex + chunk.LocalFileCount <= TestArchive.Files.Count, "Chunk local file index + count exceeds global file count.");

                for (int i = (int)chunk.GlobalFileIndex; i < chunk.GlobalFileIndex + chunk.LocalFileCount; i++)
                {
                    Assert.IsTrue(TestArchive.RawFiles[i].ChunkID == chunk.ID, "Referenced file does not belong to this chunk.");
                }

                int index = 0;
                foreach (var file in TestArchive.RawFiles)
                {
                    Assert.IsFalse(TestArchive.RawFiles.Any(x => 
                        x.ChunkID == chunk.ID
                        && index < chunk.GlobalFileIndex
                        && index > chunk.GlobalFileIndex + chunk.LocalFileCount),
                        "Not all files in this chunk were indexed to this chunk.");

                    index++;
                }
            }

            TestCommon.TeardownPPX(TestArchive);
        }
    }
}
