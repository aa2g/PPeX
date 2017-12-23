using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Xa
{
    public class XaSection1Data
    {
        public string Name;
        public byte[] Unknown;

        public static XaSection1Data FromReader(BinaryReader reader)
        {
            XaSection1Data data = new XaSection1Data();

            data.Name = reader.ReadEncryptedString();

            int count = reader.ReadInt32();

            data.Unknown = reader.ReadBytes(count * 72);

            return data;
        }
    }

    public class XaSection1
    {
        public XaSection1Data[] Data = new XaSection1Data[0];

        public static XaSection1 FromReader(BinaryReader reader)
        {
            XaSection1 section = new XaSection1();

            if (reader.ReadByte() == 0)
                return section;

            int count = reader.ReadInt32();

            section.Data = new XaSection1Data[count];

            for (int i = 0; i < count; i++)
                section.Data[i] = XaSection1Data.FromReader(reader);

            return section;
        }
    }
}
