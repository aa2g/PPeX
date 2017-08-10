using Crc32C;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;

namespace PPeX
{
    public class ExtendedArchiveChunk
    {
        public uint ID { get; protected set; }

        public ArchiveChunkCompression Compression { get; protected set; }

        public uint CRC32C { get; protected set; }

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
            chunk.Compression = (ArchiveChunkCompression)reader.ReadByte();
            chunk.CRC32C = reader.ReadUInt32();
            chunk.Offset = reader.ReadUInt64();
            chunk.CompressedLength = reader.ReadUInt64();
            chunk.UncompressedLength = reader.ReadUInt64();
            chunk.GlobalFileIndex = reader.ReadUInt32();
            chunk.LocalFileCount = reader.ReadUInt32();

            return chunk;
        }

        protected IReadOnlyList<ArchiveFileSource> generateFileList()
        {
            List<ArchiveFileSource> files = new List<ArchiveFileSource>();

            foreach (ArchiveFileSource file in baseArchive.Files)
            {
                if (file.ChunkID == ID)
                    files.Add(file);
            }

            return files;
        }

        IReadOnlyList<ArchiveFileSource> _cachedFileList = null;

        public IReadOnlyList<ArchiveFileSource> Files
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
            using (Stream raw = GetRawStream())
            using (IDecompressor decompressor = CompressorFactory.GetDecompressor(raw, Compression))
                return decompressor.Decompress();
        }

        public Stream GetRawStream()
        {
            Stream stream = new FileStream(baseArchive.Filename, FileMode.Open, FileAccess.Read, FileShare.Read);

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
                byte[] buffer = new byte[Core.Settings.BufferSize];
                int length = 0;
                while ((length = source.Read(buffer, 0, (int)Core.Settings.BufferSize)) > 0)
                {
                    crc = Crc32CAlgorithm.Append(crc, buffer, 0, length);
                }
            }

            return crc == CRC32C;
        }
    }
}
