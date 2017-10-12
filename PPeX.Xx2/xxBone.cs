using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxBone
    {
        string Name;

        int Index;

        public float[,] Transforms = new float[4, 4];

        internal xxBone(BinaryReader reader)
        {
            Name = reader.ReadEncryptedStringInt();
            Index = reader.ReadInt32();

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    Transforms[x, y] = reader.ReadSingle();
        }
    }
}
