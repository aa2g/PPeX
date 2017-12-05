using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using PPeX;
using System.Text;
using System.Linq;

namespace PPeXTests
{
    [TestClass]
    public class ExtendedArchiveAppenderTests
    {
        public static byte[] TestData = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");

        [TestMethod]
        public void AppendMetadataTest()
        {
            var ppx = TestCommon.GeneratePPX();

            ExtendedArchiveAppender appender = new ExtendedArchiveAppender(ppx);

            appender.Name = "test-appended";

            var file1 = new Subfile(
                new MemorySource(TestData),
                    "test3-2",
                    "t",
                    ArchiveFileType.Raw);

            var file2 = new Subfile(
                new MemorySource(Encoding.UTF8.GetBytes("short data")),
                    "test4-2",
                    "t",
                    ArchiveFileType.Raw);

            var file3 = appender.BaseArchive.RawFiles.Find(x => x.Name == "test00");

            appender.FilesToAdd.Add(file1);
            appender.FilesToAdd.Add(file2);

            appender.FilesToRemove.Add(file3);

            appender.Write();


            ppx = new ExtendedArchive(ppx.Filename);

            Assert.AreEqual("test-appended", ppx.Title);

            Assert.AreEqual(33, ppx.Files.Count);

            Assert.IsTrue(ppx.Files.Any(x => x.Name == file1.Name));
            Assert.IsTrue(ppx.Files.Any(x => x.Name == file2.Name));
            
            Assert.IsFalse(ppx.Files.Any(x => x.Name == file3.Name));

            TestCommon.TeardownPPX(ppx);
        }

        [TestMethod]
        public void DefragementArchiveTest()
        {
            var ppx = TestCommon.GeneratePPX();

            ExtendedArchiveAppender appender = new ExtendedArchiveAppender(ppx);

            Assert.IsFalse(appender.WastedSpaceExists);

            var file3 = appender.BaseArchive.RawFiles.Find(x => x.Name == "test00");

            appender.FilesToRemove.Add(file3);

            appender.Write();

            Assert.IsTrue(appender.WastedSpaceExists);

            appender.Defragment();

            Assert.IsFalse(appender.WastedSpaceExists);

            TestCommon.TeardownPPX(ppx);
        }
    }
}
