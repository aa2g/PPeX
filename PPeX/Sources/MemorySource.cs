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
    public class MemorySource : IDataSource
    {
        protected uint size;

        public MemorySource(byte[] data, ArchiveFileCompression compression, ArchiveFileType type)
        {
            DataStream = new MemoryStream(data);
            Compression = compression;
            Type = type;

            Md5 = Utility.GetMd5(DataStream);
            DataStream.Position = 0;
        }

        public MemorySource(Stream stream, ArchiveFileCompression compression, ArchiveFileType type)
        {
            DataStream = new MemoryStream();
            stream.CopyTo(DataStream);

            Compression = compression;
            Type = type;

            Md5 = Utility.GetMd5(DataStream);
            DataStream.Position = 0;
        }

        public byte[] Md5 { get; protected set; }

        public uint Size => 0;

        public ArchiveFileCompression Compression { get; set; }

        public ArchiveFileType Type { get; set; }
        
        public MemoryStream DataStream { get; protected set; }

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
