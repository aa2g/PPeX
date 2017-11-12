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
        /// The uncompressed file size of the subfile.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// The content type of the subfile.
        /// </summary>
        ArchiveFileType Type { get; }

        /// <summary>
        /// Creates a stream that returns uncompressed and unencoded data.
        /// </summary>
        /// <returns></returns>
        Stream GetRawStream();
    }
}
