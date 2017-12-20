using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Sviex
{
    public class Sviex2Reader
    {
        public static SviexFile FromReader(BinaryReader reader)
        {
            SviexFile file = new SviexFile();

            file.Version = reader.ReadInt32();

            int sectionCount = reader.ReadInt32();
            file.Sections = new SviexSection[sectionCount];

            for (int i = 0; i < sectionCount; i++)
                file.Sections[i] = new SviexSection();

            //indices
            for (int s = 0; s < sectionCount; s++)
            {
                //unshuffle the bytes
                int count = reader.ReadInt32();
                file.Sections[s].Indices = new ushort[count];

                byte[][] unshuffled = new byte[2][];

                for (int i = 0; i < 2; i++)
                    unshuffled[i] = reader.ReadBytes(count);

                ushort old = 0;

                //resolve deltas
                for (int i = 0; i < count; i++)
                {
                    ushort current = (ushort)BitConverter.ToInt16(new byte[] { unshuffled[0][i], unshuffled[1][i] }, 0);

                    file.Sections[s].Indices[i] = (ushort)(old + current);

                    old += current;
                }
            }

            //positions
            for (int s = 0; s < sectionCount; s++)
            {
                int count = reader.ReadInt32();

                file.Sections[s].Positions = new float[count];

                for (int i = 0; i < count; i++)
                    file.Sections[s].Positions[i] = reader.ReadSingle();
            }

            //weights
            for (int s = 0; s < sectionCount; s++)
            {
                int count = reader.ReadInt32();

                file.Sections[s].Weights = new float[count];

                for (int i = 0; i < count; i++)
                    file.Sections[s].Weights[i] = reader.ReadSingle();
            }

            //bone indices
            for (int s = 0; s < sectionCount; s++)
            {
                int count = reader.ReadInt32();

                file.Sections[s].boneIndices = new sbyte[count];

                for (int i = 0; i < count; i++)
                    file.Sections[s].boneIndices[i] = reader.ReadSByte();
            }
            
            //bone -> indices
            for (int s = 0; s < sectionCount; s++)
            {
                //unshuffle the bytes
                int count = reader.ReadInt32();

                //note: for this section only
                file.Sections[s].Bones = new SviexBone[count];
                for (int i = 0; i < count; i++)
                    file.Sections[s].Bones[i] = new SviexBone();

                byte[][] unshuffled = new byte[4][];

                for (int i = 0; i < 4; i++)
                    unshuffled[i] = reader.ReadBytes(count);

                //resolve deltas
                int old = 0;

                for (int i = 0; i < count; i++)
                {
                    int current = BitConverter.ToInt32(new byte[] { unshuffled[0][i], unshuffled[1][i], unshuffled[2][i], unshuffled[3][i] }, 0);

                    file.Sections[s].Bones[i].Index = old + current;

                    old += current;
                }
            }

            //bone -> name
            for (int s = 0; s < sectionCount; s++)
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    file.Sections[s].Bones[i].Name = reader.ReadString();
                }
            }

            //bone -> transforms
            for (int t = 0; t < 16; t++)
                for (int s = 0; s < sectionCount; s++)
                {
                    int count = reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        file.Sections[s].Bones[i].Transforms[t] = reader.ReadSingle();
                    }
                }

            //normals
            for (int s = 0; s < sectionCount; s++)
            {
                int count = reader.ReadInt32();

                file.Sections[s].Normals = new float[count];

                for (int i = 0; i < count; i++)
                    file.Sections[s].Normals[i] = reader.ReadSingle();
            }

            //UV
            for (int s = 0; s < sectionCount; s++)
            {
                int count = reader.ReadInt32();

                file.Sections[s].UV = new float[count];

                for (int i = 0; i < count; i++)
                    file.Sections[s].UV[i] = reader.ReadSingle();
            }

            //names
            for (int s = 0; s < sectionCount; s++)
            {
                file.Sections[s].Name = reader.ReadString();
            }

            //submesh IDX
            for (int s = 0; s < sectionCount; s++)
            {
                file.Sections[s].submeshIdx = reader.ReadInt32();
            }

            //unknowns
            for (int s = 0; s < sectionCount; s++)
            {
                file.Sections[s].Unknown = reader.ReadBytes(1);
            }

            return file;
        }
    }
}
