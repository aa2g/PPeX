using System;
using System.Collections.Generic;
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
    }
}
