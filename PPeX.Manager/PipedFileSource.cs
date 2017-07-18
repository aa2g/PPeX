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
    /// <summary>
    /// A data source that is streamed over a pipe.
    /// </summary>
    public class PipedFileSource : IDataSource
    {
        /// <summary>
        /// The address of the file.
        /// </summary>
        public string Address;
        /// <summary>
        /// The compressed size of the file.
        /// </summary>
        protected uint CompressedSize;
        /// <summary>
        /// The uncompressed size of the file.
        /// </summary>
        public uint DecompressedSize;
        /// <summary>
        /// The compression used for the data of the file.
        /// </summary>
        public ArchiveFileCompression Compression;
        /// <summary>
        /// The type of the data.
        /// </summary>
        public ArchiveFileType Type;
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public uint Size => DecompressedSize;

        /// <summary>
        /// The MD5 hash of the data.
        /// </summary>
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

        /// <summary>
        /// Returns an uncompressed stream of data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            Stream stream;
            lock (lockObject)
            {
                //Ask the pipe for the data
                var handler = Manager.Client.CreateConnection();
                handler.WriteString("load");
                handler.WriteString(Address);

                //Put it in memory
                using (BinaryReader reader = new BinaryReader(handler.BaseStream, Encoding.Unicode, true))
                {
                    int length = reader.ReadInt32();
                    stream = new MemoryStream(
                        reader.ReadBytes(length),
                        false);
                }
            }

            //Decompress it
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
