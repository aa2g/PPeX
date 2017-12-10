using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    [System.Diagnostics.DebuggerDisplay("{Name}", Name = "{Name}")]
    public class ArchiveSubfile : ISubfile
    {
        public ArchiveFileSource RawSource { get; protected set; }
        public IDataSource Source => RawSource;

        public ArchiveSubfile(ArchiveFileSource source)
        {
            RawSource = source;
        }

        public string ArchiveName => RawSource.ArchiveName;

        public string Name => RawSource.Name;

        public string EmulatedArchiveName => RawSource.EmulatedArchiveName;

        public string EmulatedName => RawSource.EmulatedName;

        public ulong Size => (uint)RawSource.Size;

        public ArchiveFileType Type => RawSource.Type;

        public Stream GetRawStream()
        {
            return RawSource.GetStream();
        }
    }
}
