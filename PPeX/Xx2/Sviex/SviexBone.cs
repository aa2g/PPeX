using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Sviex
{
    public class SviexBone
    {
        public string Name;
        public int Index;
        public float[] Transforms = new float[16];

        public static SviexBone FromReader(BinaryReader reader)
        {
            SviexBone bone = new SviexBone();

            bone.Name = reader.ReadEncryptedString();
            bone.Index = reader.ReadInt32();
            
            for (int i = 0; i < 16; i++)
                bone.Transforms[i] = reader.ReadSingle();

            return bone;
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteEncryptedString(Name);
            writer.Write(Index);

            for (int i = 0; i < 16; i++)
                writer.Write(Transforms[i]);
        }
    }
}
