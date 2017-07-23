using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// A subfile that stores data in a segregated fashion.
    /// </summary>
    public class IsolatedSubfile : BaseSubfile
    {
        /// <summary>
        /// The compressed size of the data.
        /// </summary>
        public override uint Size => Source.Size;

        /// <summary>
        /// Creates a new subfile from generic data.
        /// </summary>
        /// <param name="Source">The source of the data.</param>
        /// <param name="Name">The name of the subfile.</param>
        /// <param name="Archive">The name of the .pp archive to associate with.</param>
        public IsolatedSubfile(ArchiveFileSource Source) : base(Source, Source.Name, Source.ArchiveName)
        {
            
        }

        /// <summary>
        /// Writes an uncompressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the uncompressed data to.</param>
        public override void WriteToStream(Stream stream)
        {
            ArchiveFileSource arcsource = Source as ArchiveFileSource;

            using (Stream substream = new Substream(
                new FileStream(arcsource.ArchiveFilename, FileMode.Open, FileAccess.Read, FileShare.Read),
                (long)arcsource.Offset,
                arcsource.Length))
            {
                substream.CopyTo(stream);
            }
        }
    }
}
