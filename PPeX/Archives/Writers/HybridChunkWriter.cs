using PPeX.Archives.Writers;
using PPeX.Compressors;
using PPeX.Encoders;
using PPeX.External.CRC32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Archives
{
    internal class HybridChunkWriter : IThreadWork
    {
        public uint ID { get; protected set; }

        public ChunkType Type { get; protected set; }

        public ArchiveChunkCompression Compression { get; protected set; }

        ulong UncompressedSize { get; set; }

        protected IArchiveContainer writer;

        public HybridChunkWriter(uint id, ArchiveChunkCompression compression, ChunkType type, IArchiveContainer writer)
        {
            ID = id;
            Compression = compression;
            Type = type;
            this.writer = writer;
        }

        protected MemoryStream UncompressedStream = new MemoryStream();

        public bool IsReady => CompressedStream != null;

        public bool ContainsFiles => fileReceipts.Count > 0;

        public MemoryStream CompressedStream { get; protected set; }

        protected List<FileReceipt> fileReceipts = new List<FileReceipt>();

        public ChunkReceipt Receipt { get; protected set; }

        public void AddFile(ISubfile file)
        {
            Md5Hash hash = file.Source.Md5;

            if (fileReceipts.Any(x => x.Md5 == hash))
            {
                FileReceipt original = fileReceipts.First(x => x.Md5 == hash);

                FileReceipt duplicate = FileReceipt.CreateDuplicate(original, file);

                fileReceipts.Add(duplicate);

                return;
            }

            using (IEncoder encoder = EncoderFactory.GetEncoder(file.Source.GetStream(), writer, file.Type))
            using (var encoded = encoder.Encode())
            {
                FileReceipt receipt = new FileReceipt
                {
                    Md5 = hash,
                    Length = (ulong)encoded.Length,
                    Offset = (ulong)UncompressedStream.Position,
                    Subfile = file
                };

                fileReceipts.Add(receipt);

                encoded.CopyTo(UncompressedStream);
            }
        }

        public bool TryAddFile(ISubfile file, ulong maxChunkSize)
        {
            Md5Hash hash = file.Source.Md5;

            if (fileReceipts.Any(x => x.Md5 == hash))
            {
                FileReceipt original = fileReceipts.First(x => x.Md5 == hash);

                FileReceipt duplicate = FileReceipt.CreateDuplicate(original, file);

                fileReceipts.Add(duplicate);

                return true;
            }

            IEncoder encoder = EncoderFactory.GetEncoder(file.Source.GetStream(), writer, file.Type);
            Stream dataStream;

            if (file.Source is ArchiveFileSource &&
                file.Type == ArchiveFileType.OpusAudio)
            {
                //don't need to reencode
                dataStream = (file.Source as ArchiveFileSource).GetStream();
            }
            else
            {
                dataStream = encoder.Encode();
            }

            using (dataStream)
            {
                if (UncompressedStream.Length == 0 ||
                    (ulong)(dataStream.Length + UncompressedStream.Length) <= maxChunkSize)
                {
                    FileReceipt receipt = new FileReceipt
                    {
                        Md5 = hash,
                        Length = (ulong)dataStream.Length,
                        Offset = (ulong)UncompressedStream.Position,
                        Subfile = file
                    };

                    fileReceipts.Add(receipt);

                    dataStream.CopyTo(UncompressedStream);

                    return true;
                }
            }
            return false;
        }

        public void Compress()
        {
            UncompressedStream.Position = 0;

            using (UncompressedStream)
            using (ICompressor compressor = CompressorFactory.GetCompressor(UncompressedStream, Compression))
            {
                CompressedStream = new MemoryStream();

                compressor.WriteToStream(CompressedStream);

                uint crc = CRC32.Compute(CompressedStream.ToArray());

                Receipt = new ChunkReceipt
                {
                    ID = this.ID,
                    Compression = this.Compression,
                    Type = this.Type,
                    CRC = crc,
                    UncompressedSize = (ulong)UncompressedStream.Length,
                    CompressedSize = (ulong)CompressedStream.Length,
                    FileReceipts = fileReceipts
                };

                CompressedStream.Position = 0;
            }

            //FinishedWriters.Enqueue(this);
        }

        public void Dispose()
        {
            if (UncompressedStream != null)
                UncompressedStream.Dispose();

            if (CompressedStream != null)
                CompressedStream.Dispose();
        }

        public Stream GetData()
        {
            if (!IsReady)
                Compress();

            return CompressedStream;
        }
    }

    public class FileReceipt
    {
        public ISubfile Subfile;

        public Md5Hash Md5;
        public ulong Offset;
        public ulong Length;
        //public int Index;

        public static FileReceipt CreateDuplicate(FileReceipt original, ISubfile subfile)
        {
            FileReceipt receipt = original.MemberwiseClone() as FileReceipt;

            receipt.Subfile = subfile;

            return receipt;
        }
    }

    public class ChunkReceipt
    {
        public uint ID;
        public ChunkType Type;
        public ArchiveChunkCompression Compression;
        public uint CRC;
        public ulong FileOffset;
        public ulong CompressedSize;
        public ulong UncompressedSize;

        public ICollection<FileReceipt> FileReceipts;
    }
}
