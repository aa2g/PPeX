using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The settings that the currently running PPeX instance is using.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The frame size (in seconds) that the Opus encoder should use.
        /// </summary>
        public static double XggFrameSize = 0.020;

        /// <summary>
        /// The bitrate that the Opus encoder should use.
        /// </summary>
        public static int XggBitrate = 48000;

        /// <summary>
        /// Whether or not checksums should be verified on an archive load. Note: If enabled, causes massive slowdowns on first loads.
        /// </summary>
        public static bool VerifyChecksums = false;

        /// <summary>
        /// The default buffer size that should be used.
        /// </summary>
        public static uint BufferSize = 4096;
    }
}
