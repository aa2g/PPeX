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

        public ushort[] Faces;

        public List<xxVertex> Verticies = new List<xxVertex>();

        internal xxMeshInfo(BinaryReader reader, int version)
        {
            if (version < 8)
                Unknowns.Add(reader.ReadBytes(0x10));
            else
                Unknowns.Add(reader.ReadBytes(0x40));

            Unknowns.Add(reader.ReadBytes(4));

            int faceCount = reader.ReadInt32();

            Faces = new ushort[faceCount];

            for (int i = 0; i < faceCount; i++)
                Faces[i] = reader.ReadUInt16();

            int vertexCount = reader.ReadInt32();

            if (version >= 4)
                for (int i = 0; i < vertexCount; i++)
                    Verticies.Add(new xxVertex(reader));

            if (version >= 5)
                Unknowns.Add(reader.ReadBytes(20));

            if (version >= 2)
                Unknowns.Add(reader.ReadBytes(0x64));

            if (version >= 3)
                if (version < 6)
                    Unknowns.Add(reader.ReadBytes(0x40));
                else
                    Unknowns.Add(reader.ReadBytes(0x100));

            if (version >= 6)
                Unknowns.Add(reader.ReadBytes(0x1C));

            if (version >= 8)
            {
                Unknowns.Add(reader.ReadBytes(1));

                Unknowns.Add(Encoding.ASCII.GetBytes(reader.ReadEncryptedString()));
                //uint unknownLength = reader.ReadUInt32();
                //Unknowns.Add(BitConverter.GetBytes(unknownLength));

                //Unknowns.Add(reader.ReadBytes(unknownLength));

                Unknowns.Add(reader.ReadBytes(16));
            }
        }
    }
}
