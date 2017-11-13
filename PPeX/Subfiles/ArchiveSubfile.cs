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
        protected ArchiveFileSource _source;
        public IDataSource Source => _source;

        public ArchiveSubfile(ArchiveFileSource source)
        {
            _source = source;
            Name = _source.Name;
        }

        public ArchiveSubfile(ArchiveFileSource source, string name)
        {
            _source = source;
            Name = name;
        }

        public string ArchiveName => _source.ArchiveName;

        public string Name { get; protected set; }

        public ulong Size => (uint)_source.Size;

        public ArchiveFileType Type => _source.Type;

        public Stream GetRawStream()
        {
            return _source.GetStream();
        }
    }
}
