using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class Xx2File
    {
        public int Version;

        public byte[] HeaderUnknown;

        public xxObject RootObject;

        public byte[] MaterialUnknown;

        public List<xxMaterial> Materials = new List<xxMaterial>();

        public List<xxTexture> Textures = new List<xxTexture>();

        public byte[] UnencodedData;

        public Xx2File(int version, xxObject root, byte[] headerUnknown, byte[] materialUnknown, List<xxMaterial> materials, List<xxTexture> textures, byte[] unencoded)
        {
            Version = version;
            HeaderUnknown = headerUnknown;
            RootObject = root;
            MaterialUnknown = materialUnknown;
            Materials = materials;
            Textures = textures;
            UnencodedData = unencoded;
        }

        public Xx2File(xxParser parser)
        {
            Version = parser.Version;
            HeaderUnknown = parser.HeaderUnknown;
            RootObject = parser.RootObject;
            MaterialUnknown = parser.MaterialUnknown;
            Materials = parser.Materials;
            Textures = parser.Textures;
            UnencodedData = parser.UnencodedData;
        }

        public virtual void DecodeToXX(Stream stream)
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


                writer.Write(Textures.Count);

                for (int i = 0; i < Textures.Count; i++)
                    Textures[i].Write(writer);


                writer.Write(UnencodedData);
            }
        }
    }
}
