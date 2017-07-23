using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    /*
     * Container data:
     *  0 - magic PPEX
     *  4 - version [short]
     *  5 - archive type [short]
     *  7 - archive name length in bytes [ushort]
     *  9 - archive name [unicode]
     *  9 + n - number of subfiles [uint]
     *  2^10 to 2^20 - individual subfile headers
     *
     *  Note: The container data part of the header is capped at 2^10 bytes (1KB), an arbitrary amount of free space is left for the individual file headers and any unused area is zeroed out.
     *  This is for when the header needs to be modified (i.e. name changes) the entire .ppx file doesn't have to be rewritten.
     *  
     *  Only the container limit is hard coded to 1024.
     *  In regards to the individual file headers, there is no specific alignment but in this implementation a 512kb block of free space is left for additions and/or modifications.
     *  
     *  The data layout of a .ppx archive looks like this:
     *  (x is the length of file headers + free space)
     *  
     *  0           1024            1024 + x                                                      n
     *   _________________________________________________________________________________________
     *  |  Container |    Individual    |                 Compressed file data                    |
     *  |____data____|___file headers___|_________________________________________________________|
     */


    /// <summary>
    /// An extended PPX archive.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Title}", Name = "{Title}")]
    public class ExtendedArchive
    {
        internal static readonly string Magic = "PPEX";

        /// <summary>
        /// The display name of the archive.
        /// </summary>
        public string Title { get; protected set; }

        public static readonly ushort Version = 4;
        /// <summary>
        /// The type of the archive.
        /// </summary>
        public static ArchiveType Type = ArchiveType.Archive;
        
        protected List<ISubfile> files = new List<ISubfile>();
        /// <summary>
        /// Subfiles that are contained within the extended archive.
        /// </summary>
        public IReadOnlyCollection<ISubfile> ArchiveFiles => files.AsReadOnly();
        
        /// <summary>
        /// Reads from a .ppx file.
        /// </summary>
        /// <param name="Filename">The filename of the .ppx file.</param>
        protected void ReadFromFile(string Filename)
        {
            using (FileStream arc = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(arc))
            {
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

                //Check magic
                if (Magic != magic)
                    throw new InvalidDataException("Supplied file is not an extended PP archive.");

                //Check version
                ushort version = reader.ReadUInt16();

                if (version != Version)
                    throw new InvalidDataException("Supplied extended PP archive is of an incompatible version.");

                //Check type (and special code)
                ushort type = reader.ReadUInt16();

                switch (type)
                {
                    case 1:
                    case 2:
                    case 3:
                        break;
                    default:
                        throw new InvalidDataException("Supplied extended PP archive is of an incompatible type.");
                }

                //Read the file headers
                ushort strlen = reader.ReadUInt16();

                Title = Encoding.Unicode.GetString(reader.ReadBytes((int)strlen));

                reader.BaseStream.Position = 1024;

                uint number = reader.ReadUInt32();
                uint headerlength = reader.ReadUInt32();

                List<ArchiveFileSource> dupes = new List<ArchiveFileSource>();

                for (int i = 0; i < number; i++)
                {
                    var source = new ArchiveFileSource(reader, Filename);
                    var file = new IsolatedSubfile(source);

                    files.Add(file);
                }
            }
        }

        /// <summary>
        /// Opens an extended archive from a .ppx file.
        /// </summary>
        /// <param name="Filename">The filename of the .ppx file.</param>
        public ExtendedArchive(string Filename)
        {
            ReadFromFile(Filename);
        }
    }
}
