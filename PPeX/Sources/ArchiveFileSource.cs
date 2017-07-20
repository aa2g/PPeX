using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using Crc32C;

namespace PPeX
{
#warning possibly inherit ISubfile as well
    /// <summary>
    /// A data source from an extended archive (.ppx file).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name}", Name = "{Name}")]
    public class ArchiveFileSource : IDataSource, ISubfile
    {
        protected uint _size;
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public uint Size => _size;

        /// <summary>
        /// The offset of the compressed data in the file.
        /// </summary>
        public ulong Offset { get; protected set; }
        /// <summary>
        /// The length of the compressed data in the file.
        /// </summary>
        public uint Length { get; protected set; }
        /// <summary>
        /// The memory priority of the file.
        /// </summary>
        public byte Priority { get; protected set; }

        /// <summary>
        /// The filename of the .ppx file the subfile belongs to.
        /// </summary>
        public string ArchiveFilename { get; protected set; }

        /// <summary>
        /// The type of the data.
        /// </summary>
        public ArchiveFileType Type;
        /// <summary>
        /// The metadata flags associated with the file.
        /// </summary>
        public ArchiveFileFlags Flags;
        /// <summary>
        /// The compression method used on the data.
        /// </summary>
        public ArchiveFileCompression Compression;

        /// <summary>
        /// The name of the .pp file the subfile is associated with.
        /// </summary>
        public string ArchiveName { get; protected set; }

        /// <summary>
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string Name { get; protected set; }

        protected uint _crc;
        /// <summary>
        /// The CRC32C checksum of the compressed data.
        /// </summary>
        public uint Crc => _crc;

        protected byte[] _md5;
        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 => _md5;

        public IDataSource Source
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Reads a subfile from a .ppx file reader.
        /// </summary>
        /// <param name="reader">The .ppx file reader.</param>
        internal ArchiveFileSource(BinaryReader reader)
        {
            Type = (ArchiveFileType)reader.ReadByte();
            Flags = (ArchiveFileFlags)reader.ReadByte();
            Compression = (ArchiveFileCompression)reader.ReadByte();

            Priority = reader.ReadByte();
            _crc = reader.ReadUInt32();
            _md5 = reader.ReadBytes(16);
            reader.BaseStream.Seek(48, SeekOrigin.Current);

            //Names are stored as "{PPfile}/{subfile}"
            ushort len = reader.ReadUInt16();
            string[] names = Encoding.Unicode.GetString(reader.ReadBytes(len)).Split('/');

            ArchiveName = names[0];
            Name = names[1];

            Offset = reader.ReadUInt64();
            _size = reader.ReadUInt32();
            Length = reader.ReadUInt32();

            ArchiveFilename = (reader.BaseStream as FileStream).Name;
        }

        /// <summary>
        /// Verifies the compressed data to the CRC32C checksum.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            Stream stream = new Substream(
                new FileStream(ArchiveFilename, FileMode.Open, FileAccess.Read, FileShare.Read),
                (long)Offset,
                Length);

            IDecompressor decompressor = CompressorFactory.GetDecompressor(stream, Compression);

            return decompressor.Decompress();
        }

        public void WriteToStream(Stream stream)
        {
            using (Stream input = GetStream())
                input.CopyTo(stream);
        }
    }
}
