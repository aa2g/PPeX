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

        protected ulong offset;
        protected uint length;
        
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

            reader.BaseStream.Seek(1, SeekOrigin.Current);
            _crc = reader.ReadUInt32();
            _md5 = reader.ReadBytes(16);
            reader.BaseStream.Seek(48, SeekOrigin.Current);

            ushort len = reader.ReadUInt16();
            string[] names = Encoding.Unicode.GetString(reader.ReadBytes(len)).Split('/');

            ArchiveName = names[0];
            Name = names[1];

            offset = reader.ReadUInt64();
            _size = reader.ReadUInt32();
            length = reader.ReadUInt32();

            ArchiveFilename = (reader.BaseStream as FileStream).Name;
        }

        internal bool VerifyChecksum()
        {
            uint crc = 0;

            using (Stream source = GetStream())
            {
                byte[] buffer = new byte[PPeXCore.Settings.BufferSize];
                int length = 0;
                while ((length = source.Read(buffer, 0, (int)PPeXCore.Settings.BufferSize)) > 0)
                {
                    crc = Crc32CAlgorithm.Append(crc, buffer, 0, length);
                }
            }

            return crc == Crc;
        }

        internal ArchiveFileSource Dedupe(ArchiveFileSource dupe)
        {
            dupe._crc = _crc;
            dupe._md5 = _md5;
            dupe._size = _size;
            dupe.length = length;
            dupe.offset = offset;
            dupe.ArchiveFilename = ArchiveFilename;
            dupe.Compression = Compression;
            dupe.Type = Type;
            dupe.Flags |= ArchiveFileFlags.Duplicate; //was originally a duplicate

            return dupe;
        }

        public Stream GetStream()
        {
            Stream stream = new Substream(
                new FileStream(ArchiveFilename, FileMode.Open, FileAccess.Read, FileShare.Read),
                (long)offset,
                length);

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
