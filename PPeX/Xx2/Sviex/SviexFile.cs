using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2.Sviex
{
    public class SviexFile
    {
        public int Version;
        public SviexSection[] Sections;

        public static SviexFile FromReader(BinaryReader reader)
        {
            SviexFile file = new SviexFile();

            file.Version = reader.ReadInt32();

            if (file.Version != 0x64)
            {
                throw new Exception("Bad version: " + file.Version);
            }

            int sectionCount = reader.ReadInt32();
            file.Sections = new SviexSection[sectionCount];

            for (int i = 0; i < sectionCount; i++)
            {
                int type = reader.ReadInt32();
                file.Sections[i] = SviexSection.FromReader(reader);
            }

            return file;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Sections.Length);

            for (int i = 0; i < Sections.Length; i++)
            {
                writer.Write(0x64);
                Sections[i].Write(writer);
            }
        }
    }
}
