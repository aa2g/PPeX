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

namespace PPeXTests
{
    [TestClass()]
    public class FloatEncoderTests
    {
        [TestMethod()]
        public void SplitFloatTest()
        {
            float test = 1.5f;

            FloatComponent component = FloatEncoder.SplitFloat(test);

            float result = (float)component;

            Assert.AreEqual(test, result);
        }


        [TestMethod()]
        public void EncodeFloatTest()
        {
            float[] array = { 1.5f, 2f, 2.5f, 500f, 1010.1010f, 60f };

            byte[] data = FloatEncoder.Encode(array);

            float[] result = FloatEncoder.Decode(new System.IO.BinaryReader(new System.IO.MemoryStream(data)), 6);

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(array[i], result[i]);
        }


        [TestMethod()]
        public void EncodeFloatLossyTest()
        {
            float[] array = { 1.5f, 2f, 2.5f, 500f, 1010.1010f, 60f };

            byte[] data = FloatEncoder.Encode(array, 16);

            float[] result = FloatEncoder.Decode(new System.IO.BinaryReader(new System.IO.MemoryStream(data)), 6);

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(array[i], result[i], 0.005);
        }
    }


    [TestClass()]
    public class IntegerEncoderTests
    {
        [TestMethod()]
        public void EncodeFullTest()
        {
            uint[] array = { 1, 2, 3, 4, 5000, 20 };

            byte[] data = IntegerEncoder.Encode(array);

            uint[] result = IntegerEncoder.DecodeFull(new System.IO.BinaryReader(new System.IO.MemoryStream(data)), 6);

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(array[i], result[i]);
        }


        [TestMethod()]
        public void EncodeHalfTest()
        {
            uint[] array = { 1, 2, 3, 4, 5000, 20 };

            byte[] data = IntegerEncoder.Encode(array, true, true);

            uint[] result = IntegerEncoder.DecodeHalf(new System.IO.BinaryReader(new System.IO.MemoryStream(data)), 6);

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(array[i], result[i]);
        }
    }


    [TestClass()]
    public class EncoderCommonTests
    {
        [TestMethod()]
        public void ZigzagTest()
        {
            Assert.AreEqual(50, EncoderCommon.DecodeZigzag(EncoderCommon.ZigzagBase(50)));


            Assert.AreEqual(-20, EncoderCommon.DecodeZigzag(EncoderCommon.ZigzagBase(-20)));
        }


        [TestMethod()]
        public void EncryptedStringTest()
        {
            string testString = "test";

            MemoryStream mem = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(mem, Encoding.ASCII, true))
                writer.WriteEncryptedString(testString);

            Assert.AreEqual(4 + testString.Length + 1, mem.Position);

            mem.Position = 0;

            string result;

            using (BinaryReader reader = new BinaryReader(mem, Encoding.ASCII, true))
                result = reader.ReadEncryptedString();

            Assert.AreEqual(testString, result);
        }
    }
}
