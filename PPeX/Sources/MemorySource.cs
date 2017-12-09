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
        public MemorySource(byte[] data) : this(data, ArchiveFileType.Raw)
        {
            using (MemoryStream mem = new MemoryStream(data))
                Md5 = Utility.GetMd5(mem);
        }

        /// <summary>
        /// Creates a new data source from a processed byte array.
        /// </summary>
        /// <param name="data">The byte array to use.</param>
        /// <param name="compression">The compression that the byte array has used.</param>
        /// <param name="encoding">The encoding of the data.</param>
        public MemorySource(byte[] data, ArchiveFileType encoding)
        {
            DataStream = new MemoryStream(data, 0, data.Length, false, true);
            Encoding = encoding;

            using (Stream mem = GetStream())
                Md5 = Utility.GetMd5(mem);
        }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 { get; protected set; }

        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public ulong Size => (ulong)DataStream.Length;

        /// <summary>
        /// The type of the data.
        /// </summary>
        public ArchiveFileType Encoding { get; set; }
        
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
            using (IEncoder decoder = EncoderFactory.GetGenericEncoder(buffer, Encoding))
            using (Stream output = decoder.Decode())
            {
                MemoryStream temp = new MemoryStream();
                output.CopyTo(temp);
                temp.Position = 0;
                return temp;
            }
                
        }

        public void Dispose()
        {
            DataStream.Dispose();
        }
    }
}
