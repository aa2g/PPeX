using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class ExtendedHeader
    {
        public string Title { get; protected set; }

        protected List<ArchiveFileSource> files = new List<ArchiveFileSource>();
        public IReadOnlyCollection<ArchiveFileSource> ArchiveFiles => files.AsReadOnly();

        internal ExtendedHeader(BinaryReader reader)
        {

            ushort strlen = reader.ReadUInt16();

            Title = Encoding.Unicode.GetString(reader.ReadBytes((int)strlen));

            reader.BaseStream.Position = 1024;

            uint number = reader.ReadUInt32();
            uint headerlength = reader.ReadUInt32();

            List<ArchiveFileSource> dupes = new List<ArchiveFileSource>();

            for (int i = 0; i < number; i++)
            {
                var file = new ArchiveFileSource(reader);


                files.Add(file);
            }
        }
    }
}
