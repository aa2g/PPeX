using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The specific file type of the archived file.
    /// </summary>
    public enum ArchiveFileType : ushort
    {
        /// <summary>
        /// The file is generic data and can be compressed normally.
        /// </summary>
        Raw = 0,

        /// <summary>
        /// The file is audio data, encoded via <see cref="External.Wave.WaveWriter"/>.
        /// </summary>
        WaveAudio = 1,

        /// <summary>
        /// The file is audio data, encoded via <see cref="Encoders.OpusEncoder"/>.
        /// </summary>
        OpusAudio = 2,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx2Encoder"/>.
        /// </summary>
        XxMesh = 10,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx2Encoder"/>.
        /// </summary>
        Xx2Mesh = 11,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx3Encoder"/>.
        /// </summary>
        Xx3Mesh = 12,
    }

    /// <summary>
    /// The data type of the archived file.
    /// </summary>
    public enum ArchiveDataType : ushort
    {
        /// <summary>
        /// The file is generic data and can be compressed normally.
        /// </summary>
        Raw = 0,

        /// <summary>
        /// The data is audio.
        /// </summary>
        Audio = 1,

        /// <summary>
        /// The data relates to meshes.
        /// </summary>
        Mesh = 2
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
