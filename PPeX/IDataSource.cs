using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The interface for data that is to be used in writing archives.
    /// As such, data sources usually only ouput compressed data.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// The size of the uncompressed data.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        byte[] Md5 { get; }

        /// <summary>
        /// Returns a stream that reads (compressed) data.
        /// </summary>
        /// <returns></returns>
        Stream GetStream();
    }
}
