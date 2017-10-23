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

        public byte[] Unknown;

        public xxObject RootObject { get; protected set; }

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

                Unknown = new byte[unknownLength];
                Unknown[0] = format[4];

                stream.Read(Unknown, 1, unknownLength - 1);



                RootObject = new xxObject(reader, Version);

                using (MemoryStream unencoded = new MemoryStream())
                {
                    stream.CopyTo(unencoded);
                    UnencodedData = unencoded.ToArray();
                }
            }
        }
    }
}
