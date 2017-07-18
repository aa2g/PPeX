using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// A subfile used for generic data.
    /// </summary>
    public class RawSubfile : BaseSubfile
    {
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public override uint Size => Source.Size;

        /// <summary>
        /// Creates a new subfile from generic data.
        /// </summary>
        /// <param name="Source">The source of the data.</param>
        /// <param name="Name">The name of the subfile.</param>
        /// <param name="Archive">The name of the .pp archive to associate with.</param>
        public RawSubfile(IDataSource Source, string Name, string Archive) : base(Source, Name, Archive)
        {
            
        }

        /// <summary>
        /// Writes an uncompressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the uncompressed data to.</param>
        public override void WriteToStream(Stream stream)
        {
            using (Stream source = Source.GetStream())
            {
                source.CopyTo(stream);
            }
        }
    }
}
