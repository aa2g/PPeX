﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

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
        [JsonProperty]
        public double XggFrameSize = 0.060;

        /// <summary>
        /// The bitrate that the Opus encoder should use.
        /// </summary>
        [JsonProperty]
        public int XggBitrate = 48000;

        /// <summary>
        /// Whether or not checksums should be verified on an archive load. Note: If enabled, causes massive slowdowns on first loads.
        /// </summary>
        [JsonProperty]
        public bool VerifyChecksums = false;

        /// <summary>
        /// The default buffer size that should be used.
        /// </summary>
        [JsonProperty]
        public uint BufferSize = 4096;

        /// <summary>
        /// The location to load PPX archives.
        /// </summary>
        [JsonProperty]
        public string PPXLocation = "";
        /// <summary>
        /// The location to load PPX archives.
        /// </summary>
        [JsonProperty]
        public string PlaceholdersLocation = "";

        public static Settings Load()
        {
            if (File.Exists("PPeX.cfg"))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText("PPeX.cfg"));
            else
            {
                Settings settings = new Settings();
                settings.Save();
                return settings;
            }
        }

        public void Save()
        {
            File.WriteAllText("PPeX.cfg", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
