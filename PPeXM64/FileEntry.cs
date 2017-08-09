using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXM64
{
    /// <summary>
    /// A file entry that is used for indexing subfiles.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Archive}: {File}")]
    public class FileEntry
    {
        /// <summary>
        /// The archive that the subfile belongs to.
        /// </summary>
        public string Archive;
        /// <summary>
        /// The name of the subfile.
        /// </summary>
        public string File;

        public FileEntry(string Archive, string File)
        {
            this.Archive = Archive.ToLower().Replace(".pp", "");
            this.File = File.ToLower();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FileEntry))
                return false;
            var other = obj as FileEntry;
            return (Archive == other.Archive) && (File == other.File);
        }

        public override int GetHashCode()
        {
            return (Archive + File).GetHashCode();
        }
    }
}
