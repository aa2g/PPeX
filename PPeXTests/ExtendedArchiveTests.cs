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

            byte[] data = Encoding.UTF8.GetBytes("dfdfdfdfdfdfdfdf");

            writer.Files.Add(new ArchiveFile(
                new MemorySource(data, ArchiveFileCompression.Uncompressed, ArchiveFileType.Raw),
                "t/test1",
                ArchiveFileCompression.Zstandard,
                150));

            writer.Files.Add(new ArchiveFile(
                new MemorySource(data, ArchiveFileCompression.Uncompressed, ArchiveFileType.Raw),
                "t/test2",
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
                using (Stream source = file.GetStream())
                {
                    source.CopyTo(mem);
                    sw.Stop();
                    double mbs =  (file.Size / (1024 * 1024 * ((sw.Elapsed.TotalMilliseconds - old.TotalMilliseconds) / 1000)));
                    Trace.WriteLine(file.Name + ": " + Math.Round(mbs, 1) + "mb/s");
                    old = sw.Elapsed;

                    Assert.IsTrue(Utility.CompareBytes(data, mem.ToArray()));

                    sw.Start();
                }

            }

            sw.Stop();
            Trace.WriteLine("Total: " + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }
}