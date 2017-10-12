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
        string Name;

        short Index;

        public float[,] Points = new float[3, 4];

        public int[] Unknowns = new int[7];

        internal xxVertex(BinaryReader reader)
        {
            Index = reader.ReadInt16();

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    Points[x, y] = reader.ReadSingle();

            Points[2,3] = reader.ReadSingle();

            for (int i = 0; i < 7; i++)
                Unknowns[i] = reader.ReadInt32();
        }
    }
}
