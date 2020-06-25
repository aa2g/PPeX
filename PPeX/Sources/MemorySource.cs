using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using PPeX.Common;

namespace PPeX
{
    /// <summary>
    /// A data source that is created from memory.
    /// </summary>
    public class MemorySource : IDataSource
    {
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// Creates a new data source from an unprocessed byte array.
        /// </summary>
        /// <param name="data">The byte array to use.</param>
        public MemorySource(ReadOnlyMemory<byte> data) : this(data, ArchiveFileType.Raw) { }

        /// <summary>
        /// Creates a new data source from a processed byte array.
        /// </summary>
        /// <param name="data">The byte array to use.</param>
        /// <param name="compression">The compression that the byte array has used.</param>
        /// <param name="encoding">The encoding of the data.</param>
        public MemorySource(ReadOnlyMemory<byte> data, ArchiveFileType encoding)
        {
	        Data = data;
            Encoding = encoding;
        }


        private byte[] _md5 = null;

        /// <inheritdoc/>
        public byte[] Md5
        {
	        get
	        {
                if (_md5 == null)
                {
                    _md5 = new byte[16];
	                using var hasher = MD5.Create();
	                hasher.TryComputeHash(Data.Span, _md5, out _);
                }

                return _md5;
	        }
        }

        /// <inheritdoc/>
        public ulong Size => (ulong)Data.Length;

        /// <summary>
        /// The type of the data.
        /// </summary>
        public ArchiveFileType Encoding { get; set; }

        public Task GenerateMd5HashAsync()
        {
	        throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Stream GetStream()
        {
            return new ReadOnlyMemoryStream(Data);
        }

        public void Dispose() { }
    }
}