using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX;
using PPeX.Xx2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PPeXTests.xxCommon;

namespace PPeXTests
{
    [DeploymentItem("A00_00_00_03.xx")]
    [TestClass()]
    public class Xx4ParserTests
    {
        public static Xx4File File;
        public static Xx4Writer x4writer = new Xx4Writer();
        public static string Path = "A00_00_00_03.xx";

        public static TextureBank bank = new TextureBank();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            File = new Xx4File(new xxParser(new FileStream(Path, FileMode.Open)), bank);
        }


        [ClassCleanup]
        public static void Cleanup()
        {
            System.IO.File.Delete("A00_00_00_03.xx");
        }

        [TestMethod()]
        public void FileWritingTest()
        {
            byte[] data = System.IO.File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (MemoryStream write = new MemoryStream())
            {
                File.DecodeToXX(mem, bank);

                mem.Position = 0;

                xxParser parser = new xxParser(mem);

                VerifyFile(File, new Xx4File(parser, bank));
            }
        }

        [TestMethod()]
        public void BinaryFileWritingTest()
        {
            byte[] data = System.IO.File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (MemoryStream write = new MemoryStream())
            {
                File.DecodeToXX(mem, bank);

                mem.Position = 0;

                byte[] encoded = mem.ToArray();

                Assert.IsTrue(PPeX.Utility.CompareBytes(data, encoded));
            }
        }

        [TestMethod()]
        public void Xx4WritingTest()
        {
            byte[] data = System.IO.File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (MemoryStream write = new MemoryStream())
            {
                x4writer.Write(File, mem);

                mem.Position = 0;

                var newxx = Xx4Reader.Read(mem);

                VerifyFile(File, newxx);
            }
        }
    }
}
