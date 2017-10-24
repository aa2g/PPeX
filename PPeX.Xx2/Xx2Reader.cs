using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public static class Xx2Reader
    {
        public static Xx2File Read(string filename)
        {
            return Read(new FileStream(filename, FileMode.Open));
        }

        public static Xx2File Read(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
            {
                int version = reader.ReadInt32();


                int unknownLength = reader.ReadInt32();

                byte[] headerUnknown = reader.ReadBytes(unknownLength);


                xxObject obj = ReadObject(reader);

                //always length of 4
                byte[] materialUnknown = reader.ReadBytes(4);


                int materialCount = reader.ReadInt32();

                List<xxMaterial> materials = ReadMaterials(reader, materialCount);


                int textureCount = reader.ReadInt32();

                List<xxTexture> textures = ReadTextures(reader, textureCount);


                int unencodedLength = reader.ReadInt32();

                byte[] unencoded = reader.ReadBytes(unencodedLength);


                return new Xx2File(version, obj, headerUnknown, materialUnknown, materials, textures, unencoded);
            }
        }


        public static xxObject ReadObject(BinaryReader reader)
        {
            xxObject obj = new xxObject();

            obj.Name = reader.ReadString();

            int unknownsCount = reader.ReadInt32();

            for (int i = 0; i < unknownsCount; i++)
            {
                ushort unknownLength = reader.ReadUInt16();
                obj.Unknowns.Add(reader.ReadBytes(unknownLength));
            }

            int meshesCount = reader.ReadInt32();

            for (int i = 0; i < meshesCount; i++)
                obj.Meshes.Add(ReadMesh(reader));
            
            int dupeVertexCount = reader.ReadInt32();

            obj.DuplicateVerticies = ReadVerticies(reader, dupeVertexCount);


            float[] transforms = FloatEncoder.Decode(reader, 16);

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    obj.Transforms[x, y] = transforms[(x * 4) + y];
            
            

            int boneCount = reader.ReadInt32();

            obj.Bones = ReadBones(reader, boneCount);


            int childrenCount = reader.ReadInt16();

            for (int i = 0; i < childrenCount; i++)
                obj.Children.Add(ReadObject(reader));


            return obj;
        }


        public static xxMeshInfo ReadMesh(BinaryReader reader)
        {
            xxMeshInfo mesh = new xxMeshInfo();

            int unknownsCount = reader.ReadInt32();

            for (int i = 0; i < unknownsCount; i++)
            {
                ushort unknownLength = reader.ReadUInt16();
                mesh.Unknowns.Add(reader.ReadBytes(unknownLength));
            }

            int vertexCount = reader.ReadInt32();

            mesh.Verticies = ReadVerticies(reader, vertexCount);


            int faceCount = reader.ReadInt32();

            for (int x = 0; x < faceCount; x++)
                mesh.Faces.Add(new xxFace());

            for (int i = 0; i < 3; i++)
            {
                ushort[] collection = IntegerEncoder.DecodeFull(reader, faceCount, true).Select(x => (ushort)x).ToArray();

                for (int x = 0; x < faceCount; x++)
                    mesh.Faces[x].VertexIndicies[i] = collection[x];
            }

            return mesh;
        }

        public static List<xxVertex> ReadVerticies(BinaryReader reader, int count)
        {
            if (count == 0)
                return new List<xxVertex>();

            List<xxVertex> verticies = new List<xxVertex>();

            for (int i = 0; i < count; i++)
                verticies.Add(new xxVertex());

            //decode index sizes
            for (int i = 0; i < verticies.Count; i++)
            {
                verticies[i].isIndexUInt16 = reader.ReadByte() == 1;
            }

            //decode indicies
            uint[] indicies = EncoderCommon.DecodeAll(reader, count, false, true);

            for (int i = 0; i < count; i++)
            {
                verticies[i].Index = (int)indicies[i];
            }

            //decode each position collated
            for (int i = 0; i < 3; i++)
            {
                float[] positions = FloatEncoder.Decode(reader, count);

                for (int ii = 0; ii < count; ii++)
                    verticies[ii].Position[i] = positions[ii];
            }

            //decode each normal collated
            for (int i = 0; i < 3; i++)
            {
                float[] normals = FloatEncoder.Decode(reader, count);

                for (int ii = 0; ii < count; ii++)
                    verticies[ii].Normal[i] = normals[ii];
            }

            //decode each weight collated
            for (int i = 0; i < 3; i++)
            {
                float[] weights = FloatEncoder.Decode(reader, count);

                for (int ii = 0; ii < count; ii++)
                    verticies[ii].Weights[i] = weights[ii];
            }

            //decode each UV collated
            for (int i = 0; i < 2; i++)
            {
                float[] uvs = FloatEncoder.Decode(reader, count);
                
                for (int ii = 0; ii < count; ii++)
                    verticies[ii].UV[i] = uvs[ii];
            }

            //decode bone indicies
            for (int i = 0; i < count; i++)
            {
                verticies[i].BoneIndicies = reader.ReadBytes(4);
            }

            //decode unknowns
            for (int i = 0; i < count; i++)
            {
                verticies[i].Unknown = reader.ReadBytes(20);
            }

            return verticies;
        }

        public static List<xxBone> ReadBones(BinaryReader reader, int count)
        {
            List<xxBone> bones = new List<xxBone>();

            if (count == 0)
                return bones;

            for (int i = 0; i < count; i++)
                bones.Add(new xxBone());


            //decode indicies
            uint[] indicies = EncoderCommon.DecodeAll(reader, count, false, true);

            for (int i = 0; i < count; i++)
            {
                bones[i].Index = (int)indicies[i];
            }

            //decode each name
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].Name = reader.ReadString();
            }

            //decode each transform
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    float[] transforms = FloatEncoder.Decode(reader, count);

                    for (int i = 0; i < count; i++)
                        bones[i].Transforms[x, y] = transforms[i];
                }

            return bones;
        }

        public static List<xxMaterial> ReadMaterials(BinaryReader reader, int count)
        {
            List<xxMaterial> materials = new List<xxMaterial>();

            //decode universal unknown size
            int unknownSize = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                materials.Add(new xxMaterial());

                //read name
                materials[i].Name = reader.ReadString();

                //read color sets
                materials[i].Ambient = new Color4(reader);
                materials[i].Diffuse = new Color4(reader);
                materials[i].Emissive = new Color4(reader);
                materials[i].Specular = new Color4(reader);

                //read power
                materials[i].Power = reader.ReadSingle();

                //read material textures
                //*always* in groups of 4
                for (int x = 0; x < 4; x++)
                {
                    materials[i].Textures.Add(new xxMaterialTexture());

                    materials[i].Textures[x].Name = reader.ReadString();

                    //always a size of 16
                    materials[i].Textures[x].Unknown = reader.ReadBytes(16);
                }

                //read universally sized unknown
                materials[i].Unknown = reader.ReadBytes(unknownSize);
            }

            return materials;
        }

        public static List<xxTexture> ReadTextures(BinaryReader reader, int count)
        {
            List<xxTexture> textures = new List<xxTexture>();

            List<int> indexes = new List<int>();

            List<byte[]> datasets = new List<byte[]>();

            //read each texture metadata
            for (int i = 0; i < count; i++)
            {
                textures.Add(new xxTexture());

                textures[i].Name = reader.ReadString();

                textures[i].Checksum = reader.ReadByte();
                textures[i].Depth = reader.ReadInt32();
                textures[i].Format = reader.ReadInt32();
                textures[i].Height = reader.ReadInt32();
                textures[i].ImageFileFormat = reader.ReadInt32();
                textures[i].MipLevels = reader.ReadInt32();
                textures[i].ResourceType = reader.ReadInt32();
                textures[i].Width = reader.ReadInt32();

                //unknown is always 4 bytes long
                textures[i].Unknown = reader.ReadBytes(4);

                //read the data index
                indexes.Add(reader.ReadInt32());
            }

            //read each individual texture data
            int datasetCount = reader.ReadInt32();

            for (int i = 0; i < datasetCount; i++)
            {
                int length = reader.ReadInt32();

                datasets.Add(reader.ReadBytes(length));
            }

            //reassign data to textures
            for (int i = 0; i < count; i++)
            {
                textures[i].ImageData = datasets[indexes[i]];
            }

            return textures;
        }
    }
}
