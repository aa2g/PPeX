using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SB3Utility;

namespace PPeX
{
    /// <summary>
    /// A data source from a .pp file.
    /// </summary>
    public class PPSource : IDataSource
    {
        protected IReadFile subfile;

        /// <summary>
        /// Creates a data source from a subfile in a .pp file.
        /// </summary>
        /// <param name="subfile">The subfile to read.</param>
        public PPSource(IReadFile subfile)
        {
            this.subfile = subfile;

            using (Stream stream = GetStream())
            {
                _md5 = Utility.GetMd5(stream);

                _size = (uint)stream.Position;
            }
        }

        protected byte[] _md5;
        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 => _md5;

        protected uint _size;
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public uint Size => _size;

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            return subfile.CreateReadStream();
        }
    }
}
