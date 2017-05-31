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
        string ArchiveName { get; set; }
        /// <summary>
        /// The internal filename of the subfile.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The filesize of the subfile.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Writes uncompressed data to the stream.
        /// </summary>
        /// <param name="stream">The stream to uncompress data into.</param>
        void WriteToStream(Stream stream);
    }
}
