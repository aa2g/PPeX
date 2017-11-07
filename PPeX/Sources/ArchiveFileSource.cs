using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using PPeX.Encoders;
using Crc32C;

namespace PPeX
{
    /// <summary>
    /// A data source from an extended archive (.ppx file).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name}", Name = "{Name}")]
    public class ArchiveFileSource : IDataSource
    {
        public uint ChunkID { get; protected set; }

        /// <summary>
        /// The length of the file data in the uncompressed chunk.
        /// </summary>
        public ulong Size { get; protected set; }

        /// <summary>
        /// The offset of the file data in the uncompressed chunk.
        /// </summary>
        public ulong Offset { get; protected set; }

        /// <summary>
        /// The encoding of the data.
        /// </summary>
        public ArchiveFileType Type { get; protected set; }

        /// <summary>
        /// The name of the .pp file the subfile is associated with.
        /// </summary>
        public string ArchiveName { get; protected set; }

        /// <summary>
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 { get; protected set; }

        public const uint CanonLinkID = 0xFFFFFFFF;

        public uint LinkID { get; protected set; }

        protected ExtendedArchive baseArchive;

        ExtendedArchiveChunk _indexedChunk = null;

        public ExtendedArchiveChunk Chunk
        {
            get
            {
                if (_indexedChunk == null)
                    _indexedChunk = baseArchive.Chunks.First(x => x.ID == ChunkID);

                return _indexedChunk;
            }
        }
        
        protected ArchiveFileSource()
        {
            
        }

        /// <summary>
        /// Reads a subfile from file table metadata.
        /// </summary>
        /// <param name="reader">The .ppx file reader.</param>
        public static ArchiveFileSource ReadFromTable(BinaryReader reader, ExtendedArchive archive)
        {
            ArchiveFileSource file = new ArchiveFileSource();
            file.baseArchive = archive;

            ushort len = reader.ReadUInt16();
            file.ArchiveName = System.Text.Encoding.Unicode.GetString(reader.ReadBytes(len));

            len = reader.ReadUInt16();
            file.Name = System.Text.Encoding.Unicode.GetString(reader.ReadBytes(len));

            file.Type = (ArchiveFileType)reader.ReadUInt16();

            file.Md5 = reader.ReadBytes(16);

            file.LinkID = reader.ReadUInt32();

            file.ChunkID = reader.ReadUInt32();

            file.Offset = reader.ReadUInt64();

            file.Size = (uint)reader.ReadUInt64();

            return file;
        }



        /// <summary>
        /// Returns a stream of uncompressed and unencoded data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            using (Stream stream = new Substream(
                Chunk.GetStream(),
                (long)Offset,
                (long)Size))
            using (var decoder = EncoderFactory.GetDecoder(stream, Type))
            using (Stream output = decoder.Decode())
            {
                MemoryStream temp = new MemoryStream();
                output.CopyTo(temp);
                temp.Position = 0;
                return temp;
            }
        }

        /// <summary>
        /// Returns a stream of only uncompressed data.
        /// </summary>
        /// <returns></returns>
        public Stream GetRawStream()
        {
            using (Stream stream = new Substream(
                Chunk.GetStream(),
                (long)Offset,
                (long)Size))
            {
                MemoryStream temp = new MemoryStream();
                stream.CopyTo(temp);
                temp.Position = 0;
                return temp;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
