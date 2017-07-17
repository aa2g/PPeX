using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LZ4;
using ZstdNet;

namespace PPeX.Manager
{
    public class PipedFileSource : IDataSource
    {
        public string Address;
        protected uint CompressedSize;
        public uint DecompressedSize;
        public ArchiveFileCompression Compression;
        public ArchiveFileType Type;

        public uint Size => DecompressedSize;

        public byte[] Md5
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PipedFileSource(string address, uint compressedSize, uint size, ArchiveFileCompression compression, ArchiveFileType type)
        {
            Address = address;
            DecompressedSize = size;
            CompressedSize = compressedSize;
            Compression = compression;
            Type = type;
        }

        static object lockObject = new object();

        public Stream GetStream()
        {
            Stream stream;
            lock (lockObject)
            {
                var handler = Manager.Client.CreateConnection();
                handler.WriteString("load");
                handler.WriteString(Address);

                using (BinaryReader reader = new BinaryReader(handler.BaseStream, Encoding.Unicode, true))
                {
                    int length = reader.ReadInt32();
                    stream = new MemoryStream(
                        reader.ReadBytes(length),
                        false);
                }
            }


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
                    using (Decompressor zstd = new Decompressor())
                        return new MemoryStream(zstd.Unwrap(output), false);
                case ArchiveFileCompression.Uncompressed:
                    return stream;
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            };
        }
    }
}
