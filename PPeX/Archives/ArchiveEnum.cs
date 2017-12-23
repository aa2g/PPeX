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
        WaveAudio = 11,

        /// <summary>
        /// The file is audio data, encoded via <see cref="Encoders.OpusEncoder"/>.
        /// </summary>
        OpusAudio = 12,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx2Encoder"/>.
        /// </summary>
        XxMesh = 20,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx2Encoder"/>.
        /// </summary>
        Xx2Mesh = 21,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx3Encoder"/>.
        /// </summary>
        Xx3Mesh = 22,

        /// <summary>
        /// The file is .xx mesh data, encoded via <see cref="Encoders.Xx4Encoder"/>.
        /// </summary>
        Xx4Mesh = 23,

        /// <summary>
        /// The file is .sviex mesh data, encoded via <see cref="Encoders.SviexEncoder"/>.
        /// </summary>
        SviexMesh = 30,

        /// <summary>
        /// The file is .sviex2 mesh data, encoded via <see cref="Encoders.Sviex2Encoder"/>.
        /// </summary>
        Sviex2Mesh = 31,

        /// <summary>
        /// The file is .xa animation data, encoded via <see cref="Encoders.XaEncoder"/>.
        /// </summary>
        XaAnimation = 40,

        /// <summary>
        /// The file is .xa2 animation data, encoded via <see cref="Encoders.Xa2Encoder"/>.
        /// </summary>
        Xa2Animation = 41,
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
        Mesh = 2,

        /// <summary>
        /// The data relates to SVIEX meshes.
        /// </summary>
        Sviex = 3,

        /// <summary>
        /// The data relates to animation.
        /// </summary>
        Animation = 4,
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
