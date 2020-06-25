using System.IO;
using PPeX.Encoders;

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

        public string EmulatedName => RawSource.EmulatedName;

        public ulong Size => (uint)RawSource.Size;

        public ArchiveFileType Type => RawSource.Type;

        RequestedConversion ISubfile.RequestedConversion => null;

        public Stream GetStream()
        {
            return RawSource.GetStream();
        }
    }
}