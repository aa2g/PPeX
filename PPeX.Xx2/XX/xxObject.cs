using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxObject
    {
        public string Name { get; set; }

        public float[,] Transforms = new float[4, 4];

        public List<byte[]> Unknowns = new List<byte[]>();

        public List<xxObject> Children = new List<xxObject>();

        public List<xxBone> Bones = new List<xxBone>();

        public List<xxMeshInfo> Meshes = new List<xxMeshInfo>();

        public List<xxVertex> DuplicateVerticies = new List<xxVertex>();

        internal xxObject(BinaryReader reader, int version)
        {
            Name = reader.ReadEncryptedString();

            int childCount = reader.ReadInt32();

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    Transforms[x, y] = reader.ReadSingle();

            if (version < 8)
                Unknowns.Add(reader.ReadBytes(0x10));
            else
                Unknowns.Add(reader.ReadBytes(0x20));

            int meshCount = reader.ReadInt32();

            Unknowns.Add(reader.ReadBytes(24));


            if (version < 8)
                Unknowns.Add(reader.ReadBytes(0x10));
            else
                Unknowns.Add(reader.ReadBytes(0x40));


            if (version >= 6)
                Unknowns.Add(Encoding.ASCII.GetBytes(reader.ReadEncryptedString()));


            if (meshCount > 0)
            {
                Unknowns.Add(reader.ReadBytes(1));

                for (int i = 0; i < meshCount; i++)
                    Meshes.Add(new xxMeshInfo(reader, version));

                int dupeCount = reader.ReadUInt16();

                Unknowns.Add(reader.ReadBytes(8));

                for (int i = 0; i < dupeCount; i++)
                    DuplicateVerticies.Add(new xxVertex(reader));

                int boneCount = reader.ReadInt32();

                for (int i = 0; i < boneCount; i++)
                    Bones.Add(new xxBone(reader));
            }


            for (int i = 0; i < childCount; i++)
                Children.Add(new xxObject(reader, version));
        }

        internal xxObject()
        {

        }

        public void Write(BinaryWriter writer, int version)
        {
            int unknownCounter = 0;

            writer.WriteEncryptedString(Name);
            
            writer.Write((int)Children.Count);

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    writer.Write(Transforms[x, y]);

            writer.Write(Unknowns[unknownCounter++]);

            writer.Write((int)Meshes.Count);

            writer.Write(Unknowns[unknownCounter++]);


            writer.Write(Unknowns[unknownCounter++]);


            if (version >= 6)
                writer.WriteEncryptedString(Encoding.ASCII.GetString(Unknowns[unknownCounter++]));


            if (Meshes.Count > 0)
            {
                writer.Write(Unknowns[unknownCounter++]);

                for (int i = 0; i < Meshes.Count; i++)
                    Meshes[i].Write(writer, version);

                writer.Write((ushort)DuplicateVerticies.Count);

                writer.Write(Unknowns[unknownCounter++]);

                for (int i = 0; i < DuplicateVerticies.Count; i++)
                    DuplicateVerticies[i].Write(writer);

                writer.Write((int)Bones.Count);

                for (int i = 0; i < Bones.Count; i++)
                    Bones[i].Write(writer);
            }


            for (int i = 0; i < Children.Count; i++)
                Children[i].Write(writer, version);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
