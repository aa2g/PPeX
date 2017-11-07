using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public abstract class BaseSource : IDataSource
    {
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public virtual ulong Size { get; protected set; }

        protected byte[] _md5;

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public virtual byte[] Md5
        {
            get
            {
                if (_md5 == null)
                    using (Stream stream = GetStream())
                        _md5 = Utility.GetMd5(stream);

                return _md5;
            }
        }

        public abstract void Dispose();
        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public abstract Stream GetStream();
    }
}
