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
        public short Index;

        public float[] Position = new float[3];

        public float[] Weights = new float[3];

        public byte[] BoneIndicies = new byte[4];

        public float[] Normal = new float[3];

        public float[] UV = new float[2];

        public byte[] Unknown = new byte[20];

        internal xxVertex(BinaryReader reader)
        {
            Index = reader.ReadInt16();

            for (int i = 0; i < 3; i++)
                Position[i] = reader.ReadSingle();

            for (int i = 0; i < 3; i++)
                Position[i] = reader.ReadSingle();

            BoneIndicies = reader.ReadBytes(4);

            for (int i = 0; i < 3; i++)
                Normal[i] = reader.ReadSingle();

            for (int i = 0; i < 2; i++)
                UV[i] = reader.ReadSingle();

            Unknown = reader.ReadBytes(20);
        }
    }
}
