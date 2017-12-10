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
            TryAddFile(file, 666, true);
        }

        public bool TryAddFile(ISubfile file, ulong maxChunkSize)
        {
            return TryAddFile(file, maxChunkSize, false);
        }

        protected bool TryAddFile(ISubfile file, ulong maxChunkSize, bool continueAnyway)
        {
            Md5Hash hash = file.Source.Md5;

            if (fileReceipts.Any(x => x.Md5 == hash))
            {
                FileReceipt original = fileReceipts.First(x => x.Md5 == hash);

                FileReceipt duplicate = FileReceipt.CreateDuplicate(original, file);

                fileReceipts.Add(duplicate);

                return true;
            }

#warning should add conversion/encoding settings here
            ArchiveFileType target;

            switch (file.Type)
            {
                case ArchiveFileType.WaveAudio:
                    target = ArchiveFileType.OpusAudio;
                    break;
                case ArchiveFileType.XxMesh:
                    target = ArchiveFileType.Xx3Mesh;
                    break;
                default:
                    target = file.Type;
                    break;
            }

            Stream dataStream;
            string internalName;
            string emulatedName;

            if (target == file.Type)
            {
                dataStream = file.Source.GetStream();
                internalName = file.Name;
                emulatedName = file.EmulatedName;
            }
            else
            {
                using (IEncoder decoder = EncoderFactory.GetEncoder(file.Source.GetStream(), writer, file.Type))
                using (IEncoder encoder = EncoderFactory.GetEncoder(decoder.Decode(), writer, target))
                {
                    dataStream = encoder.Encode();

                    internalName = encoder.NameTransform(file.Name);

                    switch (encoder.DataType)
                    {
                        case ArchiveDataType.Audio:
                            emulatedName = $"{file.Name.Substring(0, file.Name.LastIndexOf('.'))}.wav";
                            break;
                        case ArchiveDataType.Mesh:
                            emulatedName = $"{file.Name.Substring(0, file.Name.LastIndexOf('.'))}.xx";
                            break;
                        default:
                            emulatedName = file.EmulatedName;
                            break;
                    }
                }
            }
            
            using (dataStream)
            {
                if (continueAnyway ||
                    UncompressedStream.Length == 0 ||
                    (ulong)(dataStream.Length + UncompressedStream.Length) <= maxChunkSize)
                {
                    FileReceipt receipt = new FileReceipt
                    {
                        Md5 = hash,
                        Length = (ulong)dataStream.Length,
                        Offset = (ulong)UncompressedStream.Position,
                        InternalName = internalName,
                        EmulatedName = emulatedName,
                        Encoding = target,
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

        public string InternalName;
        public string EmulatedName;

        public ArchiveFileType Encoding;

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
