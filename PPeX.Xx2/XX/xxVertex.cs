using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxVertex
    {
        public bool isIndexUInt16;

        public int Index;

        public float[] Position = new float[3];

        public float[] Weights = new float[3];

        public byte[] BoneIndicies = new byte[4];

        public float[] Normal = new float[3];

        public float[] UV = new float[2];

        public byte[] Unknown = new byte[0];

        public xxVertex(BinaryReader reader, int version)
        {
            isIndexUInt16 = version > 3;

            if (isIndexUInt16)
                Index = reader.ReadUInt16();
            else
                Index = reader.ReadInt32();

            for (int i = 0; i < 3; i++)
                Position[i] = reader.ReadSingle();

            for (int i = 0; i < 3; i++)
                Weights[i] = reader.ReadSingle();

            BoneIndicies = reader.ReadBytes(4);

            for (int i = 0; i < 3; i++)
                Normal[i] = reader.ReadSingle();

            for (int i = 0; i < 2; i++)
                UV[i] = reader.ReadSingle();

            if (version > 3)
                Unknown = reader.ReadBytes(20);
        }

        internal xxVertex()
        {

        }

        public void Write(BinaryWriter writer)
        {
            if (isIndexUInt16)
                writer.Write((ushort)Index);
            else
                writer.Write(Index);

            for (int i = 0; i < 3; i++)
                writer.Write(Position[i]);

            for (int i = 0; i < 3; i++)
                writer.Write(Weights[i]);

            writer.Write(BoneIndicies);

            for (int i = 0; i < 3; i++)
                writer.Write(Normal[i]);

            for (int i = 0; i < 2; i++)
                writer.Write(UV[i]);

            writer.Write(Unknown);
        }
    }
}
