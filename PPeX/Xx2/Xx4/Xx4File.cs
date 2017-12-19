using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class Xx4File
    {
        public int Version;

        public byte[] HeaderUnknown;

        public xxObject RootObject;

        public byte[] MaterialUnknown;

        public List<xxMaterial> Materials = new List<xxMaterial>();

        public List<xxTextureReference> TextureRefs = new List<xxTextureReference>();

        public byte[] UnencodedData;

        public Xx4File(xxParser parser, TextureBank bank)
        {
            Version = parser.Version;
            HeaderUnknown = parser.HeaderUnknown;

            RootObject = parser.RootObject;

            MaterialUnknown = parser.MaterialUnknown;
            Materials = parser.Materials;

            TextureRefs = parser.Textures.Select(x => xxTextureReference.FromTexture(x, bank)).ToList();

            UnencodedData = parser.UnencodedData;
        }

        public Xx4File()
        {

        }

        public virtual void DecodeToBlankXX(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write((int)Version);

                writer.Write(HeaderUnknown);

                RootObject.Write(writer, Version);


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

                RootObject.Write(writer, Version);


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
