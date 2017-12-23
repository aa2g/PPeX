using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class XaFile
    {
        public byte Type;
        public byte[] Unknown;

        public XaSection1 Section1;
        public XaSection2 Section2;
        public XaSection3 Section3;
        public XaSection4 Section4;
        public XaSection5 Section5;

        public byte SectionUnknown;

        public static XaFile FromReader(BinaryReader reader)
        {
            XaFile file = new XaFile();

            file.Type = reader.ReadByte();
            file.Unknown = reader.ReadBytes(4);
            
            file.Section1 = XaSection1.FromReader(reader);
            
            file.Section2 = XaSection2.FromReader(reader);

            file.Section3 = XaSection3.FromReader(reader);

            file.Section4 = XaSection4.FromReader(reader);

            file.Section5 = XaSection5.FromReader(reader, file.Type);

            return file;
        }
    }
}
