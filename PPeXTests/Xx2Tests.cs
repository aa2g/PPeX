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
                Assert.AreEqual(array[i], result[i], 0.01);
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

    [DeploymentItem("A00_00_00_03.xx")]
    [TestClass()]
    public class Xx2WriterTests
    {
        public static Xx2File File;
        public static Xx2Writer x2writer = new Xx2Writer(0);
        public static string Path = "A00_00_00_03.xx";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            File = new Xx2File(new xxParser(new FileStream(Path, FileMode.Open)));
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            System.IO.File.Delete("A00_00_00_03.xx");
        }


        [TestMethod()]
        public void BoneWritingTest()
        {
            var bones = File.RootObject.Children[0].Children[0].Children[0].Bones;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                x2writer.WriteBones(writer, bones);

                mem.Position = 0;

                var newbones = Xx2Reader.ReadBones(reader, bones.Count);

                VerifyBones(bones, newbones);
            }
        }

        [TestMethod()]
        public void VertexWritingTest()
        {
            var verticies = File.RootObject.Children[0].Children[0].Children[0].Meshes[0].Verticies;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                x2writer.WriteVerticies(writer, verticies);

                mem.Position = 0;

                var newverticies = Xx2Reader.ReadVerticies(reader, verticies.Count);

                VerifyVerticies(verticies, newverticies);
            }
        }

        [TestMethod()]
        public void MeshWritingTest()
        {
            var meshes = File.RootObject.Children[0].Children[0].Children[0].Meshes;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                x2writer.WriteMesh(writer, meshes[0]);

                mem.Position = 0;

                var newmesh = Xx2Reader.ReadMesh(reader);

                VerifyMesh(meshes[0], newmesh);
            }
        }

        [TestMethod()]
        public void ObjectWritingTest()
        {
            var obj = File.RootObject;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                x2writer.WriteObject(writer, obj);

                mem.Position = 0;

                var newobj = Xx2Reader.ReadObject(reader);

                VerifyObject(obj, newobj);
            }
        }

        [TestMethod()]
        public void FileWritingTest()
        {
            byte[] data = System.IO.File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (MemoryStream write = new MemoryStream())
            {
                new Xx2Writer(0).Write(File, write);

                write.Position = 0;

                var xxfile2 = Xx2Reader.Read(write);

                VerifyFile(File, xxfile2);
            }
        }
    }

    [DeploymentItem("A00_00_00_03.xx")]
    [TestClass()]
    public class xxParserTests
    {
        public static Xx2File File;
        public static Xx2Writer x2writer = new Xx2Writer(0);
        public static string Path = "A00_00_00_03.xx";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            File = new Xx2File(new xxParser(new FileStream(Path, FileMode.Open)));
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            System.IO.File.Delete("A00_00_00_03.xx");
        }


        [TestMethod()]
        public void BoneWritingTest()
        {
            var bones = File.RootObject.Children[0].Children[0].Children[0].Bones;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                bones[0].Write(writer);

                mem.Position = 0;

                var newbone = new xxBone(reader);

                VerifyBones(new List<xxBone> { bones[0] }, new List<xxBone> { newbone });
            }
        }

        [TestMethod()]
        public void VertexWritingTest()
        {
            var verticies = File.RootObject.Children[0].Children[0].Children[0].Meshes[0].Verticies;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                verticies[0].Write(writer);

                mem.Position = 0;

                var newvertex = new xxVertex(reader, File.Version);

                VerifyVerticies(new List<xxVertex> { verticies[0] }, new List<xxVertex> { newvertex });
            }
        }

        [TestMethod()]
        public void MeshWritingTest()
        {
            var meshes = File.RootObject.Children[0].Children[0].Children[0].Meshes;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                meshes[0].Write(writer, File.Version);

                mem.Position = 0;

                var newmesh = new xxMeshInfo(reader, File.Version, 0);

                VerifyMesh(meshes[0], newmesh);
            }
        }

        [TestMethod()]
        public void ObjectWritingTest()
        {
            var obj = File.RootObject;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                obj.Write(writer, File.Version);

                mem.Position = 0;

                var newobj = new xxObject(reader, File.Version);

                VerifyObject(obj, newobj);
            }
        }

        [TestMethod()]
        public void FileWritingTest()
        {
            byte[] data = System.IO.File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (MemoryStream write = new MemoryStream())
            {
                File.DecodeToXX(mem);

                mem.Position = 0;

                xxParser parser = new xxParser(mem);

                VerifyFile(File, new Xx2File(parser));
            }
        }

        [TestMethod()]
        public void BinaryFileWritingTest()
        {
            byte[] data = System.IO.File.ReadAllBytes(Path);

            using (MemoryStream mem = new MemoryStream())
            using (MemoryStream write = new MemoryStream())
            {
                File.DecodeToXX(mem);

                mem.Position = 0;

                xxParser parser = new xxParser(mem);

                Assert.IsTrue(PPeX.Utility.CompareBytes(data, mem.ToArray()));
            }
        }
    }
}
