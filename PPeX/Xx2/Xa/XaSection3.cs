using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class XaMorphIndexSet
    {
        public byte[] Unknown;
        public int VertexCount;
        public ushort[] MeshIndices;
        public ushort[] MorphIndices;

        public string Name;

        public static XaMorphIndexSet FromReader(BinaryReader reader)
        {
            XaMorphIndexSet set = new XaMorphIndexSet();

            set.Unknown = reader.ReadBytes(1);
            set.VertexCount = reader.ReadInt32();

            set.MeshIndices = new ushort[set.VertexCount];
            for (int i = 0; i < set.VertexCount; i++)
                set.MeshIndices[i] = reader.ReadUInt16();

            set.MorphIndices = new ushort[set.VertexCount];
            for (int i = 0; i < set.VertexCount; i++)
                set.MorphIndices[i] = reader.ReadUInt16();

            set.Name = reader.ReadEncryptedString();

            return set;
        }
    }

    public class XaMorphKeyframe
    {
        public int VertexCount;
        public float[] Positions;
        public float[] Normals;
        public string Name;

        public static XaMorphKeyframe FromReader(BinaryReader reader)
        {
            XaMorphKeyframe frame = new XaMorphKeyframe();

            frame.VertexCount = reader.ReadInt32();

            int totalCount = frame.VertexCount * 3;

            frame.Positions = new float[totalCount];
            for (int i = 0; i < totalCount; i++)
                frame.Positions[i] = reader.ReadSingle();

            frame.Normals = new float[totalCount];
            for (int i = 0; i < totalCount; i++)
                frame.Normals[i] = reader.ReadSingle();

            frame.Name = reader.ReadEncryptedString();

            return frame;
        }
    }

    public class XaMorphKeyframeReference
    {
        public byte[] Unknown1;
        public int Index;
        public byte[] Unknown2;
        public string Name;

        public static XaMorphKeyframeReference FromReader(BinaryReader reader)
        {
            XaMorphKeyframeReference reference = new XaMorphKeyframeReference();

            reference.Unknown1 = reader.ReadBytes(1);

            reference.Index = reader.ReadInt32();

            reference.Unknown2 = reader.ReadBytes(1);

            reference.Name = reader.ReadEncryptedString();

            return reference;
        }
    }

    public class XaMorphClip
    {
        public string MeshName;
        public string Name;

        public XaMorphKeyframeReference[] KeyframeRefs;

        public byte[] Unknown;

        public static XaMorphClip FromReader(BinaryReader reader)
        {
            XaMorphClip clip = new XaMorphClip();

            clip.MeshName = reader.ReadEncryptedString();
            clip.Name = reader.ReadEncryptedString();

            int count = reader.ReadInt32();

            clip.KeyframeRefs = new XaMorphKeyframeReference[count];

            for (int i = 0; i < count; i++)
                clip.KeyframeRefs[i] = XaMorphKeyframeReference.FromReader(reader);

            clip.Unknown = reader.ReadBytes(4);

            return clip;
        }
    }

    public class XaSection3
    {
        XaMorphIndexSet[] IndexSets = new XaMorphIndexSet[0];
        XaMorphKeyframe[] Keyframes = new XaMorphKeyframe[0];
        XaMorphClip[] Clips = new XaMorphClip[0];

        public static XaSection3 FromReader(BinaryReader reader)
        {
            XaSection3 section = new XaSection3();

            if (reader.ReadByte() == 0)
                return section;

            int count = reader.ReadInt32();
            section.IndexSets = new XaMorphIndexSet[count];
            for (int i = 0; i < count; i++)
                section.IndexSets[i] = XaMorphIndexSet.FromReader(reader);

            count = reader.ReadInt32();
            section.Keyframes = new XaMorphKeyframe[count];
            for (int i = 0; i < count; i++)
                section.Keyframes[i] = XaMorphKeyframe.FromReader(reader);

            count = reader.ReadInt32();
            section.Clips = new XaMorphClip[count];
            for (int i = 0; i < count; i++)
                section.Clips[i] = XaMorphClip.FromReader(reader);

            return section;
        }
    }
}
