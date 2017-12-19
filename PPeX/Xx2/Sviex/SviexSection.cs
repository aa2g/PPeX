using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Sviex
{
    public class SviexSection
    {
        public string Name;
        public int submeshIdx;
        public ushort[] Indices;
        public float[] Positions;
        public float[] Weights;
        public sbyte[] boneIndices;
        public SviexBone[] Bones;
        public float[] Normals;
        public float[] UV;
        public byte[] Unknown;

        public static SviexSection FromReader(BinaryReader reader)
        {
            SviexSection section = new SviexSection();

            section.Name = reader.ReadEncryptedString();
            section.submeshIdx = reader.ReadInt32();

            int indicesCount = reader.ReadInt32();
            section.Indices = new ushort[indicesCount];

            for (int i = 0; i < indicesCount; i++)
                section.Indices[i] = reader.ReadUInt16();

            byte positionsPresent = reader.ReadByte();
            if (positionsPresent == 1)
            {
                int positionCount = indicesCount * 3;
                section.Positions = new float[positionCount];

                for (int i = 0; i < positionCount; i++)
                    section.Positions[i] = reader.ReadSingle();
            }
            else
            {
                section.Positions = new float[0];
            }

            byte bonesPresent = reader.ReadByte();
            if (bonesPresent == 1)
            {
                int weightCount = indicesCount * 3;
                section.Weights = new float[weightCount];

                for (int i = 0; i < weightCount; i++)
                    section.Weights[i] = reader.ReadSingle();

                int boneIndiciesCount = indicesCount * 4;
                section.boneIndices = new sbyte[boneIndiciesCount];

                for (int i = 0; i < boneIndiciesCount; i++)
                    section.Weights[i] = reader.ReadSByte();

                int boneCount = reader.ReadInt32();
                section.Bones = new SviexBone[boneCount];

                for (int i = 0; i < boneIndiciesCount; i++)
                    section.Bones[i] = SviexBone.FromReader(reader);
            }
            else
            {
                section.Weights = new float[0];
                section.boneIndices = new sbyte[0];
                section.Bones = new SviexBone[0];
            }

            byte normalsPresent = reader.ReadByte();
            if (normalsPresent == 1)
            {
                int normalCount = indicesCount * 3;
                section.Normals = new float[normalCount];
                
                for (int i = 0; i < normalCount; i++)
                    section.Normals[i] = reader.ReadSingle();
            }

            byte uvsPresent = reader.ReadByte();
            if (uvsPresent == 1)
            {
                int uvCount = indicesCount * 2;
                section.UV = new float[uvCount];

                for (int i = 0; i < uvCount; i++)
                    section.UV[i] = reader.ReadSingle();
            }

            section.Unknown = reader.ReadBytes(1);

            return section;
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteEncryptedString(Name);
            writer.Write(submeshIdx);

            writer.Write(Indices.Length);

            for (int i = 0; i < Indices.Length; i++)
                writer.Write(Indices[i]);

            byte positionsPresent = Positions.Length > 0 ? (byte)1 : (byte)0;
            writer.Write(positionsPresent);

            if (positionsPresent == 1)
            {
                for (int i = 0; i < Positions.Length; i++)
                    writer.Write(Positions[i]);
            }
            
            byte bonesPresent = Bones.Length > 0 ? (byte)1 : (byte)0;
            writer.Write(bonesPresent);

            if (bonesPresent == 1)
            {
                for (int i = 0; i < Weights.Length; i++)
                    writer.Write(Weights[i]);

                for (int i = 0; i < boneIndices.Length; i++)
                    writer.Write(boneIndices[i]);

                writer.Write(Bones.Length);
                for (int i = 0; i < Bones.Length; i++)
                    Bones[i].Write(writer);
            }

            byte normalsPresent = Normals.Length > 0 ? (byte)1 : (byte)0;
            writer.Write(normalsPresent);

            if (normalsPresent == 1)
            {
                for (int i = 0; i < Normals.Length; i++)
                    writer.Write(Normals[i]);
            }

            byte uvsPresent = UV.Length > 0 ? (byte)1 : (byte)0;
            writer.Write(uvsPresent);

            if (uvsPresent == 1)
            {
                for (int i = 0; i < UV.Length; i++)
                    writer.Write(UV[i]);
            }

            writer.Write(Unknown);
        }
    }
}
