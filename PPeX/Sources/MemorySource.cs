using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using PPeX.Encoders;

namespace PPeX
{
    /// <summary>
    /// A data source that is created from memory.
    /// </summary>
    public class MemorySource : IDataSource
    {
        protected uint size;

        /// <summary>
        /// Creates a new data source from an unprocessed byte array.
        /// </summary>
        /// <param name="data">The byte array to use.</param>
        public MemorySource(byte[] data) : this(data, new byte[] { }, ArchiveFileCompression.Uncompressed, ArchiveFileEncoding.Raw)
        {

        }

        /// <summary>
        /// Creates a new data source from a processed byte array.
        /// </summary>
        /// <param name="data">The byte array to use.</param>
        /// <param name="compression">The compression that the byte array has used.</param>
        /// <param name="encoding">The encoding of the data.</param>
        public MemorySource(byte[] data, byte[] metadata, ArchiveFileCompression compression, ArchiveFileEncoding encoding)
        {
            DataStream = new MemoryStream(data, 0, data.Length, false, true);
            Metadata = metadata;
            Size = (uint)data.Length;
            Compression = compression;
            Encoding = encoding;

            Md5 = Utility.GetMd5(DataStream);
            DataStream.Position = 0;
        }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 { get; protected set; }

        /// <summary>
        /// Metadata relating to the encoding of the file.
        /// </summary>
        public byte[] Metadata { get; protected set; }

        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
#warning implement
        public uint Size { get; protected set; }

        /// <summary>
        /// The compression method used on the data.
        /// </summary>
        public ArchiveFileCompression Compression { get; set; }

        /// <summary>
        /// The type of the data.
        /// </summary>
        public ArchiveFileEncoding Encoding { get; set; }
        
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
            using (MemoryStream buffer = new MemoryStream(DataStream.GetBuffer(), false))
            using (IDecompressor decompressor = CompressorFactory.GetDecompressor(buffer, Compression))
            using (IDecoder decoder = EncoderFactory.GetDecoder(decompressor.Decompress(), Encoding, Metadata))
            using (Stream output = decoder.Decode())
            {
                MemoryStream temp = new MemoryStream();
                output.CopyTo(temp);
                return temp;
            }
                
        }

        public void Dispose()
        {
            DataStream.Dispose();
        }
    }
}
