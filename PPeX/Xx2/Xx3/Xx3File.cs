using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class Xx3File
    {
        public int Version;

        public byte[] HeaderUnknown;

        public byte[] RootObject;

        public byte[] MaterialUnknown;

        public List<xxMaterial> Materials = new List<xxMaterial>();

        public List<xxTextureReference> TextureRefs = new List<xxTextureReference>();

        public byte[] UnencodedData;

        public Xx3File(xxParser parser, TextureBank bank)
        {
            Version = parser.Version;
            HeaderUnknown = parser.HeaderUnknown;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                parser.RootObject.Write(writer, Version);
                RootObject = mem.ToArray();
            }

            MaterialUnknown = parser.MaterialUnknown;
            Materials = parser.Materials;

            TextureRefs = parser.Textures.Select(x => xxTextureReference.FromTexture(x, bank)).ToList();

            UnencodedData = parser.UnencodedData;
        }

        public Xx3File()
        {

        }

        public virtual void DecodeToBlankXX(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write((int)Version);

                writer.Write(HeaderUnknown);

                writer.Write(RootObject);


                writer.Write(MaterialUnknown);


                writer.Write(Materials.Count);

                for (int i = 0; i < Materials.Count; i++)
                    Materials[i].Write(writer);

                //zero textures
                writer.Write((int)0);

                writer.Write(UnencodedData);
            }
        }

        public virtual void DecodeToXX(Stream stream, TextureBank bank)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write((int)Version);

                writer.Write(HeaderUnknown);

                writer.Write(RootObject);


                writer.Write(MaterialUnknown);


                writer.Write(Materials.Count);

                for (int i = 0; i < Materials.Count; i++)
                    Materials[i].Write(writer);


                writer.Write(TextureRefs.Count);

                for (int i = 0; i < TextureRefs.Count; i++)
                    TextureRefs[i].Write(writer, bank);


                writer.Write(UnencodedData);
            }
        }
    }
}
