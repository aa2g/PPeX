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
    public class PPSource : BaseSource
    {
        protected IReadFile subfile;

        /// <summary>
        /// Creates a data source from a subfile in a .pp file.
        /// </summary>
        /// <param name="subfile">The subfile to read.</param>
        public PPSource(IReadFile subfile)
        {
            this.subfile = subfile;

            var pp = subfile as SB3Utility.ppSubfile;
            Size = pp.size;
        }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public override Stream GetStream()
        {
            return subfile.CreateReadStream();
        }

        public override void Dispose()
        {
            
        }
    }
}
