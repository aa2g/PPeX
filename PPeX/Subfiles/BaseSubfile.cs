using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public abstract class BaseSubfile : ISubfile
    {
        public BaseSubfile(IDataSource Source, string Name, string Archive)
        {
            ArchiveName = Archive;
            _name = Name;
            _source = Source;
        }

        public abstract uint Size { get; }
        
        public string ArchiveName { get; set; }

        protected string _name;
        public string Name => _name;

        protected IDataSource _source;
        public IDataSource Source => _source;

        public abstract void WriteToStream(Stream stream);
    }

    public static class SubfileFactory
    {
        public static ISubfile Create(ArchiveFileSource source, string Archive)
        {
            switch (source.Type)
            {
                case ArchiveFileType.Audio:
                    return new Xgg.XggSubfile(source, source.Name, Archive);
                case ArchiveFileType.Image:
                    return new ImageSubfile(source, source.Name, Archive);
                case ArchiveFileType.Raw:
                    return new RawSubfile(source, source.Name, Archive);
                default:
                    return null;
            }
        }

        public static ISubfile Create(IDataSource source, ArchiveFileType type)
        {
            switch (type)
            {
                case ArchiveFileType.Audio:
                    return new Xgg.XggSubfile(source, "", "");
                case ArchiveFileType.Image:
                    return new ImageSubfile(source, "", "");
                case ArchiveFileType.Raw:
                    return new RawSubfile(source, "", "");
                default:
                    return null;
            }
        }
    }
}
