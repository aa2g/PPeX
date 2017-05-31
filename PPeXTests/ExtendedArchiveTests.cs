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
        public void DecompressionSpeedTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var archive = new ExtendedArchive(@"I:\AA2\Artificial Academy 2\Artificial Academy 2\data\05\jg2p06_00_002png.ppx");
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
                    sw.Start();
                }

            }

            sw.Stop();
            Trace.WriteLine("Total: " + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }
}