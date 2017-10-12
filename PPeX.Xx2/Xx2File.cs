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

        public Xx2File(int version, xxObject root, byte[] otherData)
        {
            Version = version;
            RootObject = root;
            UnencodedData = otherData;
        }

        public Xx2File(xxParser parser)
        {
            Version = parser.Version;
            RootObject = parser.RootObject;
            UnencodedData = parser.UnencodedData;
        }

        
    }
}
