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
        public int Version { get; protected set; }

        public xxObject RootObject;

        public byte[] UnencodedData;

        public byte[] Unknown;

        public Xx2File(int version, xxObject root, byte[] unknown, byte[] otherData)
        {
            Version = version;
            RootObject = root;
            Unknown = unknown;
            UnencodedData = otherData;
        }

        public Xx2File(xxParser parser)
        {
            Version = parser.Version;
            RootObject = parser.RootObject;
            Unknown = parser.Unknown;
            UnencodedData = parser.UnencodedData;
        }

        public void DecodeToXX(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write((int)Version);

                writer.Write(Unknown);

                RootObject.Write(writer, Version);

                writer.Write(UnencodedData);
            }
        }
    }
}
