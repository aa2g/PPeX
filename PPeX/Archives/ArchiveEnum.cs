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