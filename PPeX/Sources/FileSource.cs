using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crc32C;

namespace PPeX
{
    /// <summary>
    /// A data source from a file.
    /// </summary>
    public class FileSource : IDataSource
    {
        /// <summary>
        /// The filename of the file in use.
        /// </summary>
        public string Filename { get; protected set; }

        uint _size;
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public uint Size => _size;

        byte[] _md5;
        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 => _md5;

        public FileSource(string Filename)
        {
            this.Filename = Filename;
            using (FileStream fs = new FileStream(Filename, FileMode.Open))
            {
                _size = (uint)fs.Length;
                _md5 = Utility.GetMd5(fs);
            }
        }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            return new FileStream(Filename, FileMode.Open, FileAccess.Read);
        }

        public void Dispose()
        {

        }
    }
}
