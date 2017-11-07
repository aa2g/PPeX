using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxParser
    {
        public int Version { get; protected set; }

        public byte[] HeaderUnknown;

        public xxObject RootObject { get; protected set; }

        public byte[] MaterialUnknown;

        public List<xxMaterial> Materials = new List<xxMaterial>();

        public List<xxTexture> Textures = new List<xxTexture>();

        public byte[] UnencodedData;

        public xxParser(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Version = 0;

                byte[] format = reader.ReadBytes(5);

                if ((format[0] >= 0x01) && (BitConverter.ToInt32(format, 1) == 0))
                {
                    Version = BitConverter.ToInt32(format, 0);
                }
                else
                {
                    uint id = BitConverter.ToUInt32(format, 0);
                    if ((id == 0x3F8F5C29) || (id == 0x3F90A3D7) || (id == 0x3F91EB85) || (id == 0x3F933333) ||
                        (id == 0x3F947AE1) || (id == 0x3F95C28F) || (id == 0x3F970A3D) || (id == 0x3F99999A) ||
                        (id == 0x3FA66666) || (id == 0x3FB33333))
                    {
                        Version = -1;
                    }
                }

                int unknownLength;

                if (Version >= 1)
                {
                    unknownLength = 22;
                }
                else
                {
                    unknownLength = 17;
                }

                HeaderUnknown = new byte[unknownLength];
                HeaderUnknown[0] = format[4];

                stream.Read(HeaderUnknown, 1, unknownLength - 1);

                RootObject = new xxObject(reader, Version);



                MaterialUnknown = reader.ReadBytes(4);

                int materialCount = reader.ReadInt32();

                for (int i = 0; i < materialCount; i++)
                    Materials.Add(new xxMaterial(reader, Version));


                int textureCount = reader.ReadInt32();

                for (int i = 0; i < textureCount; i++)
                    Textures.Add(new xxTexture(reader));


                using (MemoryStream unencoded = new MemoryStream())
                {
                    stream.CopyTo(unencoded);
                    UnencodedData = unencoded.ToArray();
                }
            }
        }
    }
}
