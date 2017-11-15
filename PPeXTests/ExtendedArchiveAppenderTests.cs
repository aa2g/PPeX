using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using PPeX;
using System.Text;

namespace PPeXTests
{
    [TestClass]
    public class ExtendedArchiveAppenderTests
    {
        public static byte[] TestData = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");
        public static byte[] TestData2 = Encoding.UTF8.GetBytes("orem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");
        public static ExtendedArchive TestArchive;

        [DeploymentItem("libstd64.dll")]
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            FileStream arc = new FileStream("test-append.ppx", FileMode.Create);
            var writer = new ExtendedArchiveWriter(arc, "test", true);

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

            ExtendedArchiveAppender appender = new ExtendedArchiveAppender("test-append.ppx");

            appender.Title = "test-appended";

            appender.FilesToAdd.Add(new Subfile(
                new MemorySource(TestData2),
                    "test3-2",
                    "t",
                    ArchiveFileType.Raw));

            appender.FilesToAdd.Add(new Subfile(
                new MemorySource(Encoding.UTF8.GetBytes("short data")),
                    "test4",
                    "t",
                    ArchiveFileType.Raw));

            appender.FilesToRemove.Add(appender.BaseArchive.RawFiles.Find(x => x.Name == "test1"));

            appender.Write(new Progress<string>(), new Progress<int>());

            TestArchive = new ExtendedArchive("test-append.ppx");
        }

        [TestMethod]
        public void AppendMetadataTest()
        {
            Assert.AreEqual("test-appended", TestArchive.Title);

            Assert.AreEqual(4, TestArchive.Files.Count);
        }
    }
}
