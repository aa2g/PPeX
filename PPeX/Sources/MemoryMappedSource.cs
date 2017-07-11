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
    public class MemoryMappedSource : IDataSource
    {
        protected uint size;

        public MemoryMappedSource(MemoryMappedFile file, uint compressedSize, uint size, ArchiveFileCompression compression, ArchiveFileType type)
        {
            MMFile = file;
            //Address = address;
            this.size = size;
            CompressedSize = compressedSize;
            Compression = compression;
            Type = type;
        }

        public byte[] Md5
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public uint Size => size;

        public ArchiveFileCompression Compression { get; set; }

        public ArchiveFileType Type { get; set; }

        public uint CompressedSize { get; set; }

        //public string Address { get; set; }
        public MemoryMappedFile MMFile { get; set; }

        public Stream GetStream()
        {
            //MemoryMappedFile MMFile = MemoryMappedFile.OpenExisting(Address, MemoryMappedFileRights.Read);
            Stream stream = MMFile.CreateViewStream(0, CompressedSize, MemoryMappedFileAccess.Read);

            switch (Compression)
            {
                case ArchiveFileCompression.LZ4:
                    return new LZ4Stream(stream,
                        LZ4StreamMode.Decompress);
                case ArchiveFileCompression.Zstandard:
                    byte[] output;
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        stream.CopyTo(buffer);
                        output = buffer.ToArray();
                    }
                    using (ZstdNet.Decompressor zstd = new ZstdNet.Decompressor())
                        return new MemoryStream(zstd.Unwrap(output), false); //, (int)_size
                case ArchiveFileCompression.Uncompressed:
                    return stream;
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }
    }
}
