﻿using PPeX.Compressors;
using PPeX.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class Subfile : ISubfile
    {
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public ulong Size { get; protected set; }

        /// <summary>
        /// The name of the .pp file the subfile is associated with.
        /// </summary>
        public string ArchiveName { get; protected set; }

        /// <summary>
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The content type of the subfile.
        /// </summary>
        public ArchiveFileType Type { get; protected set; }

        /// <summary>
        /// The data source of the subfile.
        /// </summary>
        public IDataSource Source { get; protected set; }

        /// <summary>
        /// Writes an uncompressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the uncompressed data to.</param>
        public Stream GetRawStream()
        {
            return Source.GetStream();
        }

        public Subfile(IDataSource source, string name, string archiveName, ArchiveFileType type)
        {
            ArchiveName = archiveName;
            //Name = name;
            Source = source;
            Type = type;

            if (Type == ArchiveFileType.Xx3Mesh)
                Name = name.Replace(".xx", ".xx3");
            else
                using (IEncoder encoder = EncoderFactory.GetGenericEncoder(Source.GetStream(), Type))
                {
                    Name = encoder.NameTransform(name);
                }
        }

        public Subfile(IDataSource source, string name, string archiveName) : this(source, name, archiveName, ArchiveFileType.Raw)
        {
            if (name.EndsWith(".wav"))
                Type = ArchiveFileType.OpusAudio;
            else if (name.EndsWith(".xx"))
                Type = ArchiveFileType.Xx3Mesh;
            else
                Type = ArchiveFileType.Raw;

            if (Type == ArchiveFileType.Xx3Mesh)
                Name = name.Replace(".xx", ".xx3");
            else
                using (IEncoder encoder = EncoderFactory.GetGenericEncoder(Source.GetStream(), Type))
                {
                    Name = encoder.NameTransform(name);
                }
        }
    }
}
