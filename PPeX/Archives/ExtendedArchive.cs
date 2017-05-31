using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /*
    0 - magic PPEX
    4 - version [short]
    5 - archive type [short]
    7 - archive name length in bytes [ushort]
    9 - archive name [unicode]
    9 + n - number of subfiles [uint]
    11 + n to 1048576 - header
    */

    [System.Diagnostics.DebuggerDisplay("{Title}", Name = "{Title}")]
    public class ExtendedArchive
    {
        internal static readonly string Magic = "PPEX";

        public string Title => header.Title;
        protected string filename;

        public static readonly ushort Version = 3;
        public static readonly ushort Type = 1;

        public IReadOnlyCollection<ArchiveFileSource> ArchiveFiles => header.ArchiveFiles;

        public ExtendedArchive()
        {
            
        }

        protected ExtendedHeader header;

        protected void ReadFromFile(string Filename)
        {
            using (FileStream arc = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(arc))
            {
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (Magic != magic)
                    throw new InvalidDataException("Supplied file is not an extended PP archive.");

                ushort version = reader.ReadUInt16();

                if (version != Version)
                    throw new InvalidDataException("Supplied extended PP archive is of an incompatible version.");

                ushort type = reader.ReadUInt16();

                switch (type)
                {
                    case 1:
                        break;
                    default:
                        throw new InvalidDataException("Supplied extended PP archive is of an incompatible type.");
                }

                header = new ExtendedHeader(reader);
            }
        }

        public ExtendedArchive(string Filename)
        {
            filename = Filename;
            ReadFromFile(Filename);
        }
    }
}
