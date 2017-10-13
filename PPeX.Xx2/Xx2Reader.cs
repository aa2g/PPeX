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

                byte[] unknown = reader.ReadBytes(unknownLength);


                xxObject obj = ReadObject(reader);


                int unencodedLength = reader.ReadInt32();

                byte[] unencoded = reader.ReadBytes(unencodedLength);


                return new Xx2File(version, obj, unknown, unencoded);
            }
        }


        static xxObject ReadObject(BinaryReader reader)
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
                for (int y = 0; x < 4; x++)
                    obj.Transforms[x, y] = transforms[(x * 4) + y];
            
            

            int boneCount = reader.ReadInt32();

            ReadBones(reader, boneCount);


            int childrenCount = reader.ReadInt16();

            for (int i = 0; i < childrenCount; i++)
                obj.Children.Add(ReadObject(reader));


            return obj;
        }


        static xxMeshInfo ReadMesh(BinaryReader reader)
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


            int meshCount = reader.ReadInt32();

            mesh.Faces = IntegerEncoder.DecodeFull(reader, meshCount, true).Select(x => (ushort)x).ToArray();

            return mesh;
        }

        static List<xxVertex> ReadVerticies(BinaryReader reader, int count)
        {
            if (count == 0)
                return new List<xxVertex>();

            List<xxVertex> verticies = new List<xxVertex>();

            for (int i = 0; i < count; i++)
                verticies.Add(new xxVertex());

            //decode indicies
            uint[] indicies = EncoderCommon.DecodeAll(reader, count, false, true);

            for (int i = 0; i < count; i++)
            {
                verticies[i].Index = (ushort)indicies[i];
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

        static List<xxBone> ReadBones(BinaryReader reader, int count)
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
                bones[i].Index = indicies[i];
            }

            //decode each name
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].Name = reader.ReadString();
            }

            //decode each transform
            for (int x = 0; x < 4; x++)
                for (int y = 0; x < 4; x++)
                {
                    float[] transforms = FloatEncoder.Decode(reader, count);

                    for (int i = 0; i < count; i++)
                        bones[i].Transforms[x, y] = transforms[i];
                }

            return bones;
        }
    }
}
