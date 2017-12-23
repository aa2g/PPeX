using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class XaSection2Data
    {
        public int Unknown;
        public string Name;
        public byte[] UnknownBlock;

        public static XaSection2Data FromReader(BinaryReader reader)
        {
            XaSection2Data data = new XaSection2Data();

            data.Unknown = reader.ReadInt32();

            data.Name = reader.ReadEncryptedString();

            data.UnknownBlock = reader.ReadBytes(17);

            return data;
        }
    }

    public class XaSection2
    {
        public XaSection2Data[] Data = new XaSection2Data[0];

        public static XaSection2 FromReader(BinaryReader reader)
        {
            XaSection2 section = new XaSection2();

            if (reader.ReadByte() == 0)
                return section;

            int count = reader.ReadInt32();

            section.Data = new XaSection2Data[count];

            for (int i = 0; i < count; i++)
                section.Data[i] = XaSection2Data.FromReader(reader);

            return section;
        }
    }
}
