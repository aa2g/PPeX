﻿using System.Collections.Generic;

namespace PPeX
{
    /// <summary>
    /// The settings that the currently running PPeX instance is using.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The frame size (in seconds) that the Opus encoder should use.
        /// </summary>
        public double OpusFrameSize = 0.120;

        /// <summary>
        /// The bitrate that the Opus encoder should use.
        /// </summary>
        public int OpusMusicBitrate = 44000;

        /// <summary>
        /// The bitrate that the Opus encoder should use.
        /// </summary>
        public int OpusVoiceBitrate = 36000;

        /// <summary>
        /// The bit precision that the mesh encoder should use. Set to 0 for lossless mode.
        /// </summary>
        public int Xx2Precision = 0;

        /// <summary>
        /// The encode quality that the mesh encoder should use.
        /// </summary>
        public float Xx2Quality = 10f;

        /// <summary>
        /// Whether or not the Xx2 encoder should use quality instead of precision.
        /// </summary>
        public bool Xx2IsUsingQuality = false;

        /// <summary>
        /// The default level Zstandard should use in compression.
        /// </summary>
        public int ZstdCompressionLevel = 22;

        /// <summary>
        /// Whether or not checksums should be verified on an archive load. Note: If enabled, causes massive slowdowns on first loads.
        /// </summary>
        public bool VerifyChecksums = false;

        /// <summary>
        /// The default buffer size that should be used.
        /// </summary>
        public uint BufferSize = 4096;

        /// <summary>
        /// Default encoding rules all archive writers use.
        /// </summary>
        public Dictionary<ArchiveFileType, ArchiveFileType> DefaultEncodingConversions = new Dictionary<ArchiveFileType, ArchiveFileType>
        {
            { ArchiveFileType.WaveAudio, ArchiveFileType.OpusAudio }
        };
    }
}
