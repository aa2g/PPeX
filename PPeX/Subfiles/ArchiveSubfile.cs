using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class ArchiveSubfile : ISubfile
    {
        public ArchiveFileSource RawSource { get; protected set; }
        public IDataSource Source => RawSource;

        public ArchiveSubfile(ArchiveFileSource source)
        {
            RawSource = source;
            Name = RawSource.Name;
        }

        public ArchiveSubfile(ArchiveFileSource source, string name)
        {
            RawSource = source;
            Name = name;
        }

        public string ArchiveName => RawSource.ArchiveName;

        public string Name { get; protected set; }

        public ulong Size => (uint)RawSource.Size;

        public ArchiveFileType Type => RawSource.Type;

        public Stream GetRawStream()
        {
            return RawSource.GetStream();
        }
    }
}
