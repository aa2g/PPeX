using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class XaSection4Data
    {
        public byte[] Unknown1;
        public byte[] Unknown2;
        public byte[] Unknown3;

        public static XaSection4Data FromReader(BinaryReader reader)
        {
            XaSection4Data data = new XaSection4Data();

            data.Unknown1 = reader.ReadBytes(104);
            data.Unknown2 = reader.ReadBytes(4);
            data.Unknown3 = reader.ReadBytes(64);

            return data;
        }
    }

    public class XaSection4
    {
        public XaSection4Data[] Data = new XaSection4Data[0];

        public static XaSection4 FromReader(BinaryReader reader)
        {
            XaSection4 section = new XaSection4();

            if (reader.ReadByte() == 0)
                return section;

            int count = reader.ReadInt32();

            section.Data = new XaSection4Data[count];

            for (int i = 0; i < count; i++)
                section.Data[i] = XaSection4Data.FromReader(reader);

            return section;
        }
    }
}
