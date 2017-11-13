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
    public class xx3ParserTests
    {
        public static Xx3File File;
        public static Xx3Writer x3writer = new Xx3Writer(0);
        public static string Path = "A00_00_00_03.xx";

        public static TextureBank bank = new TextureBank();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            File = new Xx3File(new xxParser(new FileStream(Path, FileMode.Open)), bank);
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

                VerifyFile(File, new Xx3File(parser, bank));
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

                System.IO.File.WriteAllBytes("B:\\1.xx", data);
                System.IO.File.WriteAllBytes("B:\\2.xx", encoded);

                Assert.IsTrue(PPeX.Utility.CompareBytes(data, encoded));
            }
        }
    }
}
