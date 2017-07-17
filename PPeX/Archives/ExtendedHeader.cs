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
            reader.BaseStream.Position = 1024;

            ushort strlen = reader.ReadUInt16();

            Title = Encoding.Unicode.GetString(reader.ReadBytes((int)strlen));

            uint number = reader.ReadUInt32();
            uint headerlength = reader.ReadUInt32();

            List<ArchiveFileSource> dupes = new List<ArchiveFileSource>();

            for (int i = 0; i < number; i++)
            {
                var file = new ArchiveFileSource(reader);

                if (file.Compression == ArchiveFileCompression.Duplicate)
                    dupes.Add(file);
                else
                    files.Add(file);
            }

            foreach (var file in dupes)
            {
                var original = files.First(x =>
                    Enumerable.SequenceEqual(x.Md5, file.Md5) &&
                    !x.Flags.HasFlag(ArchiveFileFlags.Duplicate));

                files.Add(original.Dedupe(file));
            }
        }
    }
}
