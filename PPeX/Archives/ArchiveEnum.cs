using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The type of the PPX archive.
    /// </summary>
    public enum ArchiveType : ushort
    {
        /// <summary>
        /// The archive is related to the base game.
        /// </summary>
        Archive = 1,

        /// <summary>
        /// The archive is related to a modification.
        /// </summary>
        Mod = 2,

        /// <summary>
        /// The archive is a custom music pack.
        /// </summary>
        BGMPack = 3
    }

    /// <summary>
    /// The data type of the archived file.
    /// </summary>
    public enum ArchiveFileEncoding : byte
    {
        /// <summary>
        /// The file is generic data and can be compressed normally.
        /// </summary>
        Raw = 0,

        /// <summary>
        /// The file is audio data, encoded via <see cref="Encoders.XggEncoder"/>.
        /// </summary>
        XggAudio = 1
    }

    /// <summary>
    /// The method of compression used on the archived file.
    /// </summary>
    public enum ArchiveFileCompression : byte
    {
        /// <summary>
        /// No compression is used.
        /// </summary>
        Uncompressed = 0,

        /// <summary>
        /// LZ4 compression is used (see <see cref="Compressors.Lz4Compressor"/>)
        /// </summary>
        LZ4 = 1,

        /// <summary>
        /// Zstd compression is used (see <see cref="Compressors.ZstdCompressor"/>)
        /// </summary>
        Zstandard = 2
    }

    /// <summary>
    /// Metadata flags that are attached to archived files.
    /// </summary>
    [Flags]
    public enum ArchiveFileFlags : byte
    {
#warning properly (re)implement this
        /// <summary>
        /// No metadata flags have been attached.
        /// </summary>
        None = 0,
        /// <summary>
        /// The file is related to the .ppx file and/or PPeX itself, and is not a file relating to AA2.
        /// </summary>
        Meta = 1,
        /// <summary>
        /// Indicates that the file should be forced into being kept in memory.
        /// </summary>
        MemoryCached = 2
    }
}
