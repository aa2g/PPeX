using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX.Xx2.Sviex;

namespace PPeXTests
{
    [DeploymentItem("A00_01_O_blazer_S.sviex")]
    [TestClass]
    public class SviexTests
    {
        public static readonly string Path = "A00_01_O_blazer_S.sviex";

        [TestMethod()]
        public void BinaryFileWritingTest()
        {
            byte[] data = File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(File.OpenRead(Path)))
            {
                SviexFile file = SviexFile.FromReader(reader);

                file.Write(writer);

                byte[] encoded = mem.ToArray();

                Assert.IsTrue(PPeX.Utility.CompareBytes(data, encoded));
            }
        }
    }
}
