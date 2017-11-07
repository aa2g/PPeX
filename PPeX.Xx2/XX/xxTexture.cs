using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxTexture
    {
        public string Name;

        public byte[] Unknown;

        public int Width;

        public int Height;

        public int Depth;

        public int MipLevels;

        public int Format;

        public int ResourceType;

        public int ImageFileFormat;

        public byte Checksum;

        public byte[] ImageData;

        public xxTexture(BinaryReader reader)
        {
            Name = reader.ReadEncryptedString();

            Unknown = reader.ReadBytes(4);

            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Depth = reader.ReadInt32();
            MipLevels = reader.ReadInt32();
            Format = reader.ReadInt32();
            ResourceType = reader.ReadInt32();
            ImageFileFormat = reader.ReadInt32();
            Checksum = reader.ReadByte();

            int length = reader.ReadInt32();

            ImageData = reader.ReadBytes(length);
        }

        internal xxTexture()
        {

        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.WriteEncryptedString(Name);

            writer.Write(Unknown);

            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Depth);
            writer.Write(MipLevels);
            writer.Write(Format);
            writer.Write(ResourceType);
            writer.Write(ImageFileFormat);
            writer.Write(Checksum);

            writer.Write((int)ImageData.Length);

            writer.Write(ImageData);
        }
    }
}
