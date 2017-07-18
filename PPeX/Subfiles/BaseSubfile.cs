using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The abstract implementation of a subfile stored in a .ppx archive.
    /// </summary>
    public abstract class BaseSubfile : ISubfile
    {
        public BaseSubfile(IDataSource Source, string Name, string Archive)
        {
            ArchiveName = Archive;
            _name = Name;
            _source = Source;
        }

        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public abstract uint Size { get; }

        /// <summary>
        /// The name of the .pp file the subfile is associated with.
        /// </summary>
        public string ArchiveName { get; set; }

        protected string _name;
        /// <summary>
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string Name => _name;

        protected IDataSource _source;
        /// <summary>
        /// The data source of the subfile.
        /// </summary>
        public IDataSource Source => _source;

        /// <summary>
        /// Writes an uncompressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the uncompressed data to.</param>
        public abstract void WriteToStream(Stream stream);
    }

    /// <summary>
    /// Contains methods related to creating subfiles.
    /// </summary>
    public static class SubfileFactory
    {
        /// <summary>
        /// Creates a relevant subfile based on the type of data it contains.
        /// </summary>
        /// <param name="source">The subfile belonging to a .ppx archive.</param>
        /// <returns></returns>
        public static ISubfile Create(ArchiveFileSource source)
        {
            switch (source.Type)
            {
                case ArchiveFileType.Audio:
                    return new Xgg.XggSubfile(source, source.Name, source.ArchiveName);
                case ArchiveFileType.Image:
                    return new ImageSubfile(source, source.Name, source.ArchiveName);
                case ArchiveFileType.Raw:
                    return new RawSubfile(source, source.Name, source.ArchiveName);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates an anonymous subfile from a data source.
        /// </summary>
        /// <param name="source">The data source of the file.</param>
        /// <param name="type">The type of data that the data source is.</param>
        /// <returns></returns>
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
