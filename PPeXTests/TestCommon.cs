using PPeX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXTests
{
    static class TestCommon
    {
        public static int filecount = 8;
        public static byte[][] TestData;

        public static ExtendedArchive GeneratePPX()
        {
            //generate random data
            Random random = new Random();
            TestData = new byte[filecount][];

            for (int i = 0; i < filecount; i++)
            {
                TestData[i] = new byte[512];
                random.NextBytes(TestData[i]);
            }

            string name = "test_ppx" + random.Next(0, 255).ToString();

            //create archive
            var writer = new ExtendedArchiveWriter(name, true);

            writer.ChunkSizeLimit = 1024;

            //add files + duplicates
            for (int i = 0; i < 8; i++)
            {
                for (int d = 0; d < 4; d++)
                {
                    ISubfile file = new Subfile(new MemorySource(TestData[i]), $"test{i}{d}", "test", ArchiveFileType.Raw);

                    writer.Files.Add(file);
                }
            }

            using (FileStream arc = new FileStream($"{name}.ppx", FileMode.Create))
                writer.Write(arc);

            return new ExtendedArchive($"{name}.ppx");
        }

        public static void TeardownPPX(ExtendedArchive archive)
        {
            if (File.Exists(archive.Filename))
                File.Delete(archive.Filename);
        }
    }
}
