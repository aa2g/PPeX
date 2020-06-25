using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using PPeX.Compressors;

namespace PPeX
{
    public class ExtendedArchiveChunk
    {
        public uint ID { get; protected set; }

        public ArchiveChunkCompression Compression { get; protected set; }

        public uint CRC32 { get; protected set; }

        public ulong Offset { get; protected set; }

        public ulong CompressedLength { get; protected set; }

        public ulong UncompressedLength { get; protected set; }


        protected ExtendedArchive baseArchive;

        protected ExtendedArchiveChunk()
        {

        }

        internal static ExtendedArchiveChunk ReadFromTable(BinaryReader reader, ExtendedArchive archive)
        {
            ExtendedArchiveChunk chunk = new ExtendedArchiveChunk();
            chunk.baseArchive = archive;

            chunk.ID = reader.ReadUInt32();
            chunk.Compression = (ArchiveChunkCompression)reader.ReadByte();
            chunk.CRC32 = reader.ReadUInt32();
            chunk.Offset = reader.ReadUInt64();
            chunk.CompressedLength = reader.ReadUInt64();
            chunk.UncompressedLength = reader.ReadUInt64();

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

        //public Stream GetStream()
        //{
        //    MemoryStream mem = new MemoryStream();

        //    using (Stream raw = GetRawStream())
        //    using (IDecompressor decompressor = CompressorFactory.GetDecompressor(Compression))
        //        decompressor.Decompress(raw).CopyTo(mem);

        //    mem.Position = 0;

        //    return mem;
        //       // return decompressor.Decompress();
        //}

        public void CopyToMemory(Memory<byte> memory)
        {
            if (memory.Length < (int)UncompressedLength)
                throw new ArgumentException("Memory buffer must be at least the uncompressed size of the chunk");

	        using var rawStream = GetRawStream();

	        if (Compression == ArchiveChunkCompression.Zstandard)
	        {
                using var zstdDecompressor = new ZstdDecompressor();

                using var buffer = MemoryPool<byte>.Shared.Rent((int)CompressedLength);
                var compressedMemory = buffer.Memory.Slice(0, (int)CompressedLength);

                rawStream.Read(compressedMemory.Span);

                zstdDecompressor.DecompressData(compressedMemory.Span, memory.Span, out _);
	        }
            else
	        {
		        rawStream.Read(memory.Span);
	        }
        }

        public Stream GetRawStream()
        {
            Stream stream = new FileStream(baseArchive.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return new PartialStream(stream, (long)Offset, (long)CompressedLength);
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