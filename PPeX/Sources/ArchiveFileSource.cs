using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crc32C;
using LZ4;

namespace PPeX
{
    [System.Diagnostics.DebuggerDisplay("{Name}", Name = "{Name}")]
    public class ArchiveFileSource : IDataSource
    {
        protected uint _size;
        public uint Size => _size;

        public ulong Offset { get; protected set; }
        public uint Length { get; protected set; }
        public byte Priority { get; protected set; }

        public string ArchiveFilename { get; protected set; }

        public ArchiveFileType Type;
        public ArchiveFileFlags Flags;
        public ArchiveFileCompression Compression;

        public string ArchiveName { get; protected set; }

        public string Name { get; protected set; }

        protected uint _crc;
        public uint Crc => _crc;

        protected byte[] _md5;
        public byte[] Md5 => _md5;

        internal ArchiveFileSource(BinaryReader reader)
        {
            Type = (ArchiveFileType)reader.ReadByte();
            Flags = (ArchiveFileFlags)reader.ReadByte();
            Compression = (ArchiveFileCompression)reader.ReadByte();

            Priority = reader.ReadByte();
            _crc = reader.ReadUInt32();
            _md5 = reader.ReadBytes(16);
            reader.BaseStream.Seek(48, SeekOrigin.Current);

            ushort len = reader.ReadUInt16();
            string[] names = Encoding.Unicode.GetString(reader.ReadBytes(len)).Split('/');

            ArchiveName = names[0];
            Name = names[1];

            Offset = reader.ReadUInt64();
            _size = reader.ReadUInt32();
            Length = reader.ReadUInt32();

            ArchiveFilename = (reader.BaseStream as FileStream).Name;
        }

        internal bool VerifyChecksum()
        {
            uint crc = 0;

            using (Stream source = GetStream())
            {
                byte[] buffer = new byte[Core.Settings.BufferSize];
                int length = 0;
                while ((length = source.Read(buffer, 0, (int)Core.Settings.BufferSize)) > 0)
                {
                    crc = Crc32CAlgorithm.Append(crc, buffer, 0, length);
                }
            }

            return crc == Crc;
        }

        public Stream GetStream()
        {
            Stream stream = new Substream(
                new FileStream(ArchiveFilename, FileMode.Open, FileAccess.Read, FileShare.Read),
                (long)Offset,
                Length);

            switch (Compression)
            {
                case ArchiveFileCompression.LZ4:
                    return new LZ4Stream(stream,
                        LZ4StreamMode.Decompress);
                case ArchiveFileCompression.Zstandard:
                    byte[] output;
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        stream.CopyTo(buffer);
                        output = buffer.ToArray();
                    }
                    using (ZstdNet.Decompressor zstd = new ZstdNet.Decompressor())
                        return new MemoryStream(zstd.Unwrap(output), false); //, (int)_size
                case ArchiveFileCompression.Uncompressed:
                    return stream;
                default:
                    throw new InvalidOperationException("Compression type is invalid.");
            }
        }
    }
}
