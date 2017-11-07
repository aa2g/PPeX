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
        }

        public string ArchiveName => _source.ArchiveName;

        public string Name => _source.Name;

        public uint Size => _source.Size;

        public ArchiveFileType Type => _source.Type;

        public Stream GetRawStream()
        {
            return _source.GetStream();
        }
    }
}
