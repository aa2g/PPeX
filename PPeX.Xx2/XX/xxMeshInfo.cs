using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxMeshInfo
    {
        public List<byte[]> Unknowns = new List<byte[]>();

        public List<xxFace> Faces = new List<xxFace>();

        public List<xxVertex> Verticies = new List<xxVertex>();

        public List<float[]> Vertex2List = new List<float[]>();

        public xxMeshInfo(BinaryReader reader, int version, byte numVector2PerVertex)
        {
            if (version < 7)
                Unknowns.Add(reader.ReadBytes(0x10));
            else
                Unknowns.Add(reader.ReadBytes(0x40));

            Unknowns.Add(reader.ReadBytes(4));

            int faceCount = reader.ReadInt32() / 3;
            
            for (int i = 0; i < faceCount; i++)
                Faces.Add(new xxFace(reader));

            int vertexCount = reader.ReadInt32();

            if (version >= 4)
                for (int i = 0; i < vertexCount; i++)
                    Verticies.Add(new xxVertex(reader, version));

            if (version >= 7)
                Unknowns.Add(reader.ReadBytes(20));

            if (numVector2PerVertex > 0)
            {
                for (int j = 0; j < Verticies.Count * numVector2PerVertex; j++)
                {
                    float[] vertex2 = new float[2];
                    vertex2[0] = reader.ReadSingle();
                    vertex2[1] = reader.ReadSingle();

                    Vertex2List.Add(vertex2);
                }
            }

            if (version >= 2)
                Unknowns.Add(reader.ReadBytes(100));

            if (version >= 7)
            {
                Unknowns.Add(reader.ReadBytes(284));

                if (version >= 8)
                {
                    Unknowns.Add(reader.ReadBytes(1));

                    Unknowns.Add(Encoding.ASCII.GetBytes(reader.ReadEncryptedString()));

                    Unknowns.Add(reader.ReadBytes(16));
                }
            }
            else
            {
                if (version >= 3)
                {
                    Unknowns.Add(reader.ReadBytes(64));
                }
                if (version >= 5)
                {
                    Unknowns.Add(reader.ReadBytes(20));
                }
                if (version >= 6)
                {
                    Unknowns.Add(reader.ReadBytes(28));
                }
            }
        }

        internal xxMeshInfo()
        {

        }

        public void Write(BinaryWriter writer, int version)
        {
            int unknownCounter = 0;

            writer.Write(Unknowns[unknownCounter++]);

            writer.Write(Unknowns[unknownCounter++]);

            writer.Write((int)Faces.Count * 3);

            for (int i = 0; i < Faces.Count; i++)
                Faces[i].Write(writer);

            writer.Write((int)Verticies.Count);

            if (version >= 4)
                for (int i = 0; i < Verticies.Count; i++)
                    Verticies[i].Write(writer);

            if (version >= 7)
                writer.Write(Unknowns[unknownCounter++]);

            if (version >= 2)
                writer.Write(Unknowns[unknownCounter++]);

            if (version > 6)
            {
                writer.Write(Unknowns[unknownCounter++]);

                if (version >= 8)
                {
                    writer.Write(Unknowns[unknownCounter++]);

                    writer.WriteEncryptedString(Encoding.ASCII.GetString(Unknowns[unknownCounter++]));

                    writer.Write(Unknowns[unknownCounter++]);
                }
            }
            else
            {
                if (version >= 3)
                {
                    writer.Write(Unknowns[unknownCounter++]);
                }
                if (version >= 5)
                {
                    writer.Write(Unknowns[unknownCounter++]);
                }
                if (version >= 6)
                {
                    writer.Write(Unknowns[unknownCounter++]);
                }
            }
        }
    }
}
