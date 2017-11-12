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
    public class Xx3WriterTests
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
        public void BoneWritingTest()
        {
            var bones = File.RootObject.Children[0].Children[0].Children[0].Bones;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                x3writer.WriteBones(writer, bones);

                mem.Position = 0;

                var newbones = Xx3Reader.ReadBones(reader, bones.Count);

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
                x3writer.WriteVerticies(writer, verticies);

                mem.Position = 0;

                var newverticies = Xx3Reader.ReadVerticies(reader, verticies.Count);

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
                x3writer.WriteMesh(writer, meshes[0], File.Version);

                mem.Position = 0;

                var newmesh = Xx3Reader.ReadMesh(reader);

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
                x3writer.WriteObject(writer, obj);

                mem.Position = 0;

                var newobj = Xx3Reader.ReadObject(reader);

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
                new Xx3Writer(0).Write(File, write);

                write.Position = 0;

                var xxfile2 = Xx3Reader.Read(write);

                VerifyFile(File, xxfile2);
            }
        }

        [TestMethod()]
        public void MaterialWritingTest()
        {
            var materials = File.Materials;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            using (BinaryReader reader = new BinaryReader(mem))
            {
                x3writer.WriteMaterials(writer, materials);

                mem.Position = 0;

                var newmaterials = Xx3Reader.ReadMaterials(reader, materials.Count);

                VerifyMaterials(materials, newmaterials);
            }
        }
    }

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
