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
                Version = reader.ReadInt32();

                Unknown = reader.ReadBytes(22);

                RootObject = new xxObject(reader, Version);

                UnencodedData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }
    }
}
