using System.IO;
using PPeX.Encoders;

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
        /// The filename of the .pp archive that the subfile belongs to.
        /// </summary>
        string ArchiveName { get; }

        /// <summary>
        /// The filename of the subfile.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of the file that AA2 sees. Empty string if should not be seen.
        /// </summary>
        string EmulatedName { get; }

        /// <summary>
        /// The uncompressed file size of the subfile.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// The current content type of the subfile.
        /// </summary>
        ArchiveFileType Type { get; }

        RequestedConversion RequestedConversion { get; }

        /// <summary>
        /// Creates a stream that returns uncompressed data.
        /// </summary>
        /// <returns></returns>
        Stream GetStream();
    }
}