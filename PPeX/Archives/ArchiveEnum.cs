using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The data type of the archived file.
    /// </summary>
    public enum ArchiveFileType : ushort
    {
        /// <summary>
        /// The file is generic data and can be compressed normally.
        /// </summary>
        Raw = 0,

        /// <summary>
        /// The file is audio data, encoded via <see cref="Encoders.XggEncoder"/>.
        /// </summary>
        XggAudio = 1,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx2Encoder"/>.
        /// </summary>
        Xx2Mesh = 2,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx3Encoder"/>.
        /// </summary>
        Xx3Mesh = 3
    }

    /// <summary>
    /// The method of compression used on the archived file.
    /// </summary>
    public enum ArchiveChunkCompression : byte
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
}
