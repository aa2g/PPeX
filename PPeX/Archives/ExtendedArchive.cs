﻿using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPeX
{
    /*
     * Container data:
     *  0 - magic PPEX
     *  4 - version [short]
     *  8 - archive name length in bytes [ushort]
     *  10 - archive name [unicode]
     *  10 + n - number of subfiles [uint]
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

        /// <summary>
        /// The filename of the archive.
        /// </summary>
        public string Filename { get; protected set; }

        public static readonly ushort Version = 8;

        protected List<ExtendedArchiveChunk> chunks = new List<ExtendedArchiveChunk>();
        /// <summary>
        /// Subfiles that are contained within the extended archive.
        /// </summary>
        public IReadOnlyList<ExtendedArchiveChunk> Chunks => chunks.AsReadOnly();


        public List<ArchiveFileSource> RawFiles = new List<ArchiveFileSource>();
        /// <summary>
        /// Subfiles that are contained within the extended archive.
        /// </summary>
        public List<ISubfile> Files = new List<ISubfile>();

        internal ulong ChunkTableOffset = 0;
        internal ulong FileTableOffset = 0;
        internal ulong TableInfoOffset = 0;


        /// <summary>
        /// Reads from a .ppx file.
        /// </summary>
        /// <param name="Filename">The filename of the .ppx file.</param>
        protected void ReadFromFile(string Filename)
        {
            this.Filename = Filename;

            using (FileStream arc = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(arc))
            {
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

                //Check magic
                if (Magic != magic)
                    throw new PpexException("Supplied file is not an extended PP archive.", PpexException.PpexErrorCode.FileNotPPXArchive);

                //Check version
                ushort version = reader.ReadUInt16();

                if (version != Version)
                    throw new PpexException($"Supplied extended PP archive is of version {version}; was expecting version {Version}", PpexException.PpexErrorCode.IncorrectVersionNumber);

                //Read the title
                ushort strlen = reader.ReadUInt16();

                Title = Encoding.Unicode.GetString(reader.ReadBytes((int)strlen));


                //Read chunk and file headers
                TableInfoOffset = (ulong)arc.Position;

                ChunkTableOffset = reader.ReadUInt64();
                FileTableOffset = reader.ReadUInt64();
                

                reader.BaseStream.Position = (long)ChunkTableOffset;

                uint number = reader.ReadUInt32();

                for (int i = 0; i < number; i++)
                {
                    var chunk = ExtendedArchiveChunk.ReadFromTable(reader, this);

                    chunks.Add(chunk);
                }

                reader.BaseStream.Position = (long)FileTableOffset;

                number = reader.ReadUInt32();
                
                for (int i = 0; i < number; i++)
                {
                    var source = ArchiveFileSource.ReadFromTable(reader, this);

                    RawFiles.Add(source);
                    Files.Add(new ArchiveSubfile(source));
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