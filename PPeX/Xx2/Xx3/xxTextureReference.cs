using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxTextureReference
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
        
        public int Reference;

        public static xxTextureReference FromTexture(xxTexture texture, TextureBank bank)
        {
            xxTextureReference texref = new xxTextureReference();

            texref.Name = texture.Name;
            texref.Checksum = texture.Checksum;
            texref.Height = texture.Height;
            texref.Depth = texture.Depth;
            texref.ImageFileFormat = texture.ImageFileFormat;
            texref.MipLevels = texture.MipLevels;
            texref.ResourceType = texture.ResourceType;
            texref.Width = texture.Width;
            texref.Unknown = texture.Unknown;

            texref.Reference = bank.ProcessTexture(texture);

            return texref;
        }

        public void Write(BinaryWriter writer, TextureBank bank)
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

            byte[] imgData = bank.Textures[Reference].Data;

            writer.Write((int)imgData.Length);

            writer.Write(imgData);
        }
    }
}
