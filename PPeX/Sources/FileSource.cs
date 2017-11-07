﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crc32C;

namespace PPeX
{
    /// <summary>
    /// A data source from a file.
    /// </summary>
    public class FileSource : BaseSource
    {
        /// <summary>
        /// The filename of the file in use.
        /// </summary>
        public string Filename { get; protected set; }

        public FileSource(string Filename)
        {
            this.Filename = Filename;
            Size = (uint)new FileInfo(Filename).Length;
        }

        public override byte[] Md5
        {
            get
            {
                if (Core.Settings.UseMd5Cache)
                {
                    CachedMd5 cached;
                    if (Core.Settings.Md5Cache.TryGetValue(Filename, out cached))
                    {
                        if (cached.WeakFileCompare(Filename))
                            return cached.Hash;
                    }

                    //otherwise generate new cached hash
                    cached = CachedMd5.FromFile(Filename);

                    Core.Settings.Md5Cache.Add(Filename, cached);

                    return cached.Hash;
                }

                //otherwise fall back to normal hash
                return base.Md5;
            }
        }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public override Stream GetStream()
        {
            return new FileStream(Filename, FileMode.Open, FileAccess.Read);
        }

        public override void Dispose()
        {

        }
    }
}
