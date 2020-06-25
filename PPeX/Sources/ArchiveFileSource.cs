using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string EmulatedName { get; protected set; }

        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 { get; protected set; }


        public ExtendedArchive BaseArchive { get; protected set; }

        ExtendedArchiveChunk _indexedChunk = null;

        public ExtendedArchiveChunk Chunk
        {
            get
            {
                if (_indexedChunk == null)
                    _indexedChunk = BaseArchive.Chunks.First(x => x.ID == ChunkID);

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
            file.BaseArchive = archive;

            ushort len = reader.ReadUInt16();
            file.ArchiveName = Encoding.Unicode.GetString(reader.ReadBytes(len));

            len = reader.ReadUInt16();
            file.Name = Encoding.Unicode.GetString(reader.ReadBytes(len));

            len = reader.ReadUInt16();
            file.EmulatedName = Encoding.Unicode.GetString(reader.ReadBytes(len));

            file.Type = (ArchiveFileType)reader.ReadUInt16();

            file.Md5 = reader.ReadBytes(16);

            file.ChunkID = reader.ReadUInt32();

            file.Offset = reader.ReadUInt64();

            file.Size = (uint)reader.ReadUInt64();

            return file;
        }

        public Task GenerateMd5HashAsync()
        {
	        throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns a stream of uncompressed and unencoded data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
	        using var buffer = MemoryPool<byte>.Shared.Rent((int)Chunk.UncompressedLength);
	        Chunk.CopyToMemory(buffer.Memory);

            byte[] uncompressedBuffer = new byte[Size];

            buffer.Memory.Slice((int)Offset, (int)Size).CopyTo(uncompressedBuffer);

            return new MemoryStream(uncompressedBuffer, false);
        }

        public void Dispose() { }
    }
}