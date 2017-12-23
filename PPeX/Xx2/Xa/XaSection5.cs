using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class XaAnimationClip
    {
        public string Name;
        public float Speed;
        public byte[] Unknown1;
        public float Start;
        public float End;
        public byte[] Unknown2;
        public byte[] Unknown3;
        public byte[] Unknown4;
        public int Next;
        public byte[] Unknown5;
        public byte[] Unknown6;
        public byte[] Unknown7;

        public static XaAnimationClip FromReader(BinaryReader reader)
        {
            XaAnimationClip clip = new XaAnimationClip();

            clip.Name = reader.ReadEncryptedString(64);
            clip.Speed = reader.ReadSingle();
            clip.Unknown1 = reader.ReadBytes(4);
            clip.Start = reader.ReadSingle();
            clip.End = reader.ReadSingle();
            clip.Unknown2 = reader.ReadBytes(1);
            clip.Unknown3 = reader.ReadBytes(1);
            clip.Unknown4 = reader.ReadBytes(1);
            clip.Next = reader.ReadInt32();
            clip.Unknown5 = reader.ReadBytes(1);
            clip.Unknown6 = reader.ReadBytes(4);
            clip.Unknown7 = reader.ReadBytes(16);

            return clip;
        }
    }

    public class XaAnimationKeyframe
    {
        public int Index;
        public float[] Rotation = new float[4];
        public byte[] Unknown;
        public float[] Translation = new float[3];
        public float[] Scaling = new float[3];

        public static XaAnimationKeyframe FromReader(BinaryReader reader)
        {
            XaAnimationKeyframe keyframe = new XaAnimationKeyframe();

            keyframe.Index = reader.ReadInt32();

            for (int i = 0; i < 4; i++)
                keyframe.Rotation[i] = reader.ReadSingle();

            keyframe.Unknown = reader.ReadBytes(8);

            for (int i = 0; i < 3; i++)
                keyframe.Translation[i] = reader.ReadSingle();

            for (int i = 0; i < 3; i++)
                keyframe.Scaling[i] = reader.ReadSingle();

            return keyframe;
        }
    }

    public class XaAnimationTrack
    {
        public string Name;
        public XaAnimationKeyframe[] Keyframes;
        public byte[] Unknown;

        public static XaAnimationTrack FromReader(BinaryReader reader)
        {
            XaAnimationTrack track = new XaAnimationTrack();

            track.Name = reader.ReadEncryptedString();

            int count = reader.ReadInt32();
            track.Unknown = reader.ReadBytes(4);

            track.Keyframes = new XaAnimationKeyframe[count];
            for (int i = 0; i < count; i++)
                track.Keyframes[i] = XaAnimationKeyframe.FromReader(reader);

            return track;
        }
    }

    public class XaSection5
    {
        public XaAnimationClip[] Clips = new XaAnimationClip[0];
        public XaAnimationTrack[] Tracks = new XaAnimationTrack[0];

        public static XaSection5 FromReader(BinaryReader reader, byte format)
        {
            XaSection5 section = new XaSection5();

            if (reader.ReadByte() == 0)
                return section;

            int numClips = 512;

            if (format == 0x03)
                numClips = 1024;

            
            section.Clips = new XaAnimationClip[numClips];
            for (int i = 0; i < numClips; i++)
                section.Clips[i] = XaAnimationClip.FromReader(reader);

            int count = reader.ReadInt32();
            section.Tracks = new XaAnimationTrack[count];
            for (int i = 0; i < count; i++)
                section.Tracks[i] = XaAnimationTrack.FromReader(reader);

            return section;
        }
    }
}
