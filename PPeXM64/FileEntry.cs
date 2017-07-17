using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXM64
{
    [System.Diagnostics.DebuggerDisplay("{Archive}: {File}")]
    public class FileEntry
    {
        public string Archive;
        public string File;

        public FileEntry(string Archive, string File)
        {
            this.Archive = Archive.ToLower();
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
