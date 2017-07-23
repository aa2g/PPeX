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
        [TestMethod()]
        public void CreateAndReadTest()
        {
            FileStream arc = new FileStream("test.ppx", FileMode.Create);
            var writer = new ExtendedArchiveWriter(arc, "test", true);

            byte[] data = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ac purus id diam consectetur fermentum. Etiam nulla nisi, tincidunt sed sagittis nec, finibus vel elit. Pellentesque sodales massa eget tortor eleifend dictum. Ut finibus tellus efficitur nulla hendrerit convallis. Cras sed neque sed tellus luctus vehicula sed in sapien.");

            writer.Files.Add(new ArchiveFile(
                new Subfile(new MemorySource(data),
                    "test1",
                    "t",
                    ArchiveFileCompression.Uncompressed,
                    ArchiveFileEncoding.Raw),
                ArchiveFileCompression.Uncompressed,
                150));

            writer.Files.Add(new ArchiveFile(
                new Subfile(new MemorySource(data),
                    "test2",
                    "t",
                    ArchiveFileCompression.Uncompressed,
                    ArchiveFileEncoding.Raw),
                ArchiveFileCompression.Uncompressed,
                150));

            writer.Files.Add(new ArchiveFile(
                new Subfile(new MemorySource(data),
                    "test3",
                    "t",
                    ArchiveFileCompression.LZ4,
                    ArchiveFileEncoding.Raw),
                ArchiveFileCompression.LZ4,
                150));

            writer.Files.Add(new ArchiveFile(
                new Subfile(new MemorySource(data),
                    "test4",
                    "t",
                    ArchiveFileCompression.Zstandard,
                    ArchiveFileEncoding.Raw),
                ArchiveFileCompression.Zstandard,
                150));

            writer.Write();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            arc.Close();

            var archive = new ExtendedArchive("test.ppx");

            Assert.AreEqual("test", archive.Title);

            sw.Stop();
            Trace.WriteLine("Setup: " + Math.Round(sw.Elapsed.TotalMilliseconds, 4) + "ms");

            sw.Restart();
            TimeSpan old = sw.Elapsed;


            foreach (var file in archive.ArchiveFiles)
            {
                using (MemoryStream mem = new MemoryStream())
                using (Stream source = file.Source.GetStream())
                {
                    source.CopyTo(mem);
                    sw.Stop();
                    double mbs = ((double)file.Size / (1024 * 1024 * ((sw.Elapsed.TotalMilliseconds - old.TotalMilliseconds) / 1000.0)));
                    Trace.WriteLine(file.Name + ": " + Math.Round(mbs, 5) + "mb/s");
                    old = sw.Elapsed;

                    Assert.IsTrue(Utility.CompareBytes(data, mem.ToArray()), "Data is not consistent.");

                    Assert.IsTrue((file.Source as ArchiveFileSource).VerifyChecksum(), "CRC32C does not match data.");

                    sw.Start();
                }

            }

            sw.Stop();
            Trace.WriteLine("Total: " + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }
}