using System;
using System.IO;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The interface for data that is to be used in retrieving uncompressed data.
    /// </summary>
    public interface IDataSource : IDisposable
    {
        /// <summary>
        /// The size of the uncompressed data.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        byte[] Md5 { get; }

        Task GenerateMd5HashAsync();

        /// <summary>
        /// Returns a stream that reads uncompressed data.
        /// </summary>
        /// <returns></returns>
        Stream GetStream();
    }
}
