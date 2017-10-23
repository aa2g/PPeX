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
        public string Name;

        public int Index;

        public float[,] Transforms = new float[4, 4];

        public xxBone(BinaryReader reader)
        {
            Name = reader.ReadEncryptedString();
            Index = reader.ReadInt32();

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    Transforms[x, y] = reader.ReadSingle();
        }

        internal xxBone()
        {

        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteEncryptedString(Name);

            writer.Write(Index);

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    writer.Write(Transforms[x, y]);
        }
    }
}
