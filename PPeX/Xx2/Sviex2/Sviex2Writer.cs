using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Sviex
{
    public class Sviex2Writer
    {
        public void Write(BinaryWriter writer, SviexFile file)
        {
            writer.Write(file.Version);
            writer.Write(file.Sections.Length);

            //indices
            foreach (var section in file.Sections)
            {
                int count = section.Indices.Length;
                writer.Write(count);

                short old = 0;
                List<short> deltas = new List<short>();

                for (int i = 0; i < count; i++)
                {
                    short current = (short)section.Indices[i];
                    short delta = (short)(current - old);
                    old = current;

                    //writer.Write(delta);
                    deltas.Add(delta);
                }

                for (int i = 0; i < 2; i++)
                    foreach (short delta in deltas)
                    {
                        writer.Write(BitConverter.GetBytes(delta)[i]);
                    }
            }

            //positions
            foreach (var section in file.Sections)
            {
                int count = section.Positions.Length;
                writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    writer.Write(section.Positions[i]);
                }
            }

            //weights
            foreach (var section in file.Sections)
            {
                int count = section.Weights.Length;
                writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    writer.Write(section.Weights[i]);
                }
            }

            //bone indices
            foreach (var section in file.Sections)
            {
                int count = section.boneIndices.Length;
                writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    writer.Write(section.boneIndices[i]);
                }
            }

            //bone -> indices 
            foreach (var section in file.Sections)
            {
                int count = section.Bones.Length;
                writer.Write(count);

                int old = 0;
                List<int> deltas = new List<int>();

                for (int i = 0; i < count; i++)
                {
                    int current = section.Bones[i].Index;
                    int delta = current - old;
                    old = current;

                    //writer.Write(delta);
                    deltas.Add(delta);
                }

                for (int i = 0; i < 4; i++)
                foreach (int delta in deltas)
                    {
                        writer.Write(BitConverter.GetBytes(delta)[i]);
                    }
            }

            //bone -> name 
            foreach (var section in file.Sections)
            {
                int count = section.Bones.Length;
                writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    writer.Write(section.Bones[i].Name);
                }
            }

            //bone -> transforms
            for (int t = 0; t < 16; t++)
                foreach (var section in file.Sections)
                {
                    int count = section.Bones.Length;
                    writer.Write(count);

                    for (int i = 0; i < count; i++)
                    {
                        writer.Write(section.Bones[i].Transforms[t]);
                    }
                }
            
            //normals 
            foreach (var section in file.Sections)
            {
                int count = section.Normals.Length;
                writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    writer.Write(section.Normals[i]);
                }
            }

            //UV 
            foreach (var section in file.Sections)
            {
                int count = section.UV.Length;
                writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    writer.Write(section.UV[i]);
                }
            }

            //names
            foreach (var section in file.Sections)
            {
                writer.Write(section.Name);
            }

            //submesh IDX
            foreach (var section in file.Sections)
            {
                writer.Write(section.submeshIdx);
            }

            //unknowns 
            foreach (var section in file.Sections)
            {
                writer.Write(section.Unknown);
            }
        }
    }
}
