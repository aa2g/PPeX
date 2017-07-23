using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// The abstract implementation of a subfile stored in a .ppx archive.
    /// </summary>
    public abstract class BaseSubfile : ISubfile
    {
        public BaseSubfile(IDataSource Source, string Name, string Archive)
        {
            ArchiveName = Archive;
            this.Name = Name;
            this.Source = Source;
        }

        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public abstract uint Size { get; }

        /// <summary>
        /// The name of the .pp file the subfile is associated with.
        /// </summary>
        public string ArchiveName { get; protected set; }
        
        /// <summary>
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string Name { get; protected set; }
        
        /// <summary>
        /// The data source of the subfile.
        /// </summary>
        public IDataSource Source { get; protected set; }

        /// <summary>
        /// Writes an uncompressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the uncompressed data to.</param>
        public abstract void WriteToStream(Stream stream);
    }
}
