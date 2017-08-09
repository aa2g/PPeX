using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The interface for a subfile within an extended archive. 
    /// Since these are reading from an already packed archive, subfiles usually only decompress.
    /// </summary>
    public interface ISubfile
    {
        /// <summary>
        /// The original, (usually) compressed data source.
        /// </summary>
        IDataSource Source { get; }

        /// <summary>
        /// The name of the archive that the subfile belongs to.
        /// </summary>
        string ArchiveName { get; }
        /// <summary>
        /// The internal filename of the subfile.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The compressed file size of the subfile.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// The content type of the subfile.
        /// </summary>
        ArchiveFileType Type { get; }

        /// <summary>
        /// Copies raw, compressed and/or encoded data to the stream.
        /// </summary>
        /// <param name="stream">The stream to copy.</param>
        void WriteToStream(Stream stream);
    }
}
