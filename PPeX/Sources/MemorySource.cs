using LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// A data source that is created from memory.
    /// </summary>
    public class MemorySource : IDataSource
    {
        protected uint size;

        /// <summary>
        /// Creates a new data source from a byte array.
        /// </summary>
        /// <param name="data">The byte array to use.</param>
        /// <param name="compression">The compression that the byte array has used.</param>
        /// <param name="type">The type of the data.</param>
        public MemorySource(byte[] data, ArchiveFileCompression compression, ArchiveFileType type)
        {
            DataStream = new MemoryStream(data);
            Compression = compression;
            Type = type;

            Md5 = Utility.GetMd5(DataStream);
            DataStream.Position = 0;
        }

        /// <summary>
        /// Creates a new data source from a stream, and copies it into memory.
        /// </summary>
        /// <param name="stream">The stream to use.</param>
        /// <param name="compression">The compression that the byte array has used.</param>
        /// <param name="type">The type of the data.</param>
        public MemorySource(Stream stream, ArchiveFileCompression compression, ArchiveFileType type)
        {
            DataStream = new MemoryStream();
            stream.CopyTo(DataStream);

            Compression = compression;
            Type = type;

            Md5 = Utility.GetMd5(DataStream);
            DataStream.Position = 0;
        }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 { get; protected set; }

        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
#warning implement
        public uint Size => 0;

        /// <summary>
        /// The compression method used on the data.
        /// </summary>
        public ArchiveFileCompression Compression { get; set; }

        /// <summary>
        /// The type of the data.
        /// </summary>
        public ArchiveFileType Type { get; set; }
        
        /// <summary>
        /// The internal memory stream that contains the data.
        /// </summary>
        public MemoryStream DataStream { get; protected set; }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            switch (Compression)
            {
                case ArchiveFileCompression.LZ4:
                    return new LZ4Stream(DataStream,
                        LZ4StreamMode.Decompress);
                case ArchiveFileCompression.Zstandard:
                    using (ZstdNet.Decompressor zstd = new ZstdNet.Decompressor())
                        return new MemoryStream(zstd.Unwrap(DataStream.GetBuffer()), false); //, (int)_size
                case ArchiveFileCompression.Uncompressed:
                    return DataStream;
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }
    }
}
