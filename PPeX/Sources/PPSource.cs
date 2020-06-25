using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using PPeX.External.PP;

namespace PPeX
{
    /// <summary>
    /// A data source from a .pp file.
    /// </summary>
    public class PPSource : BaseSource
    {
        public ppSubfile Subfile { get; set; }

        /// <summary>
        /// Creates a data source from a subfile in a .pp file.
        /// </summary>
        /// <param name="subfile">The subfile to read.</param>
        public PPSource(ppSubfile subfile)
        {
	        Subfile = subfile;
            Size = Subfile.size;
        }

        public override async Task GenerateMd5HashAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public override Stream GetStream()
        {
            return Subfile.CreateReadStream();
        }

        public override void Dispose()
        {
            
        }
    }
}