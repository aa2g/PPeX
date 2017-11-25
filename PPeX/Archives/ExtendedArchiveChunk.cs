using System.Collections.Generic;
using System.IO;
using PPeX.Compressors;

namespace PPeX
{
    /// <summary>
    /// The type of the chunk, which determines the file provider it uses.
    /// </summary>
    public enum ChunkType : byte
    {
        Generic = 0,
        Xx3 = 3
    }

    public class ExtendedArchiveChunk
    {
        public uint ID { get; protected set; }

        public ChunkType Type { get; protected set; }

        public ArchiveChunkCompression Compression { get; protected set; }

        public uint CRC32 { get; protected set; }

        public ulong Offset { get; protected set; }

        public ulong CompressedLength { get; protected set; }

        public ulong UncompressedLength { get; protected set; }

        public uint GlobalFileIndex { get; protected set; }

        public uint LocalFileCount { get; protected set; }


        protected ExtendedArchive baseArchive;

        protected ExtendedArchiveChunk()
        {

        }

        public static ExtendedArchiveChunk ReadFromTable(BinaryReader reader, ExtendedArchive archive)
        {
            ExtendedArchiveChunk chunk = new ExtendedArchiveChunk();
            chunk.baseArchive = archive;

            chunk.ID = reader.ReadUInt32();
            chunk.Type = (ChunkType)reader.ReadByte();
            chunk.Compression = (ArchiveChunkCompression)reader.ReadByte();
            chunk.CRC32 = reader.ReadUInt32();
            chunk.Offset = reader.ReadUInt64();
            chunk.CompressedLength = reader.ReadUInt64();
            chunk.UncompressedLength = reader.ReadUInt64();
            chunk.GlobalFileIndex = reader.ReadUInt32();
            chunk.LocalFileCount = reader.ReadUInt32();

            return chunk;
        }

        protected IReadOnlyList<ArchiveSubfile> generateFileList()
        {
            List<ArchiveSubfile> files = new List<ArchiveSubfile>();

            foreach (ArchiveFileSource file in baseArchive.RawFiles)
            {
                if (file.ChunkID == ID)
                    files.Add(new ArchiveSubfile(file));
            }

            return files;
        }

        IReadOnlyList<ArchiveSubfile> _cachedFileList = null;

        public IReadOnlyList<ArchiveSubfile> Files
        {
            get
            {
                if (_cachedFileList == null)
                    _cachedFileList = generateFileList();

                return _cachedFileList;
            }
        }

        public Stream GetStream()
        {
            MemoryStream mem = new MemoryStream();

            using (Stream raw = GetRawStream())
            using (IDecompressor decompressor = CompressorFactory.GetDecompressor(raw, Compression))
                decompressor.Decompress().CopyTo(mem);

            mem.Position = 0;

            return mem;
               // return decompressor.Decompress();
        }

        public Stream GetRawStream()
        {
            Stream stream = new FileStream(baseArchive.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return new Substream(stream, (long)Offset, (long)CompressedLength);
        }

        /// <summary>
        /// Verifies the chunk data to the CRC32C checksum.
        /// </summary>
        /// <returns></returns>
        public bool VerifyChecksum()
        {
            uint crc = 0;

            using (Stream source = GetRawStream())
            {
                crc = External.CRC32.CRC32.Compute(source);
            }

            return crc == CRC32;
        }
    }
}
