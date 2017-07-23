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
                Md5 = Utility.GetMd5(stream);

                Size = (uint)stream.Position;
            }
        }
        
        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 { get; protected set; }
        
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public uint Size { get; protected set; }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            return subfile.CreateReadStream();
        }

        public void Dispose()
        {
            
        }
    }
}
