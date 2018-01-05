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

        public ArchiveChunkCompression Compression { get; protected set; }

        ulong UncompressedSize { get; set; }

        protected IArchiveContainer writer;

        protected ulong MaxChunkSize;

        public HybridChunkWriter(uint id, ArchiveChunkCompression compression, IArchiveContainer writer, ulong maxChunkSize)
        {
            ID = id;
            Compression = compression;
            this.writer = writer;
            MaxChunkSize = maxChunkSize;

            UncompressedStream = new MemoryStream((int)MaxChunkSize);
        }

        protected MemoryStream UncompressedStream = new MemoryStream();

        public bool IsReady => CompressedStream != null;

        public bool ContainsFiles => fileReceipts.Count > 0;

        public Stream CompressedStream { get; protected set; }

        protected List<FileReceipt> fileReceipts = new List<FileReceipt>();

        public ChunkReceipt Receipt { get; protected set; }

        public void AddFile(ISubfile file)
        {
            TryAddFile(file, true);
        }

        public bool TryAddFile(ISubfile file)
        {
            return TryAddFile(file, false);
        }

        protected bool TryAddFile(ISubfile file, bool continueAnyway)
        {
            Md5Hash hash = file.Source.Md5;
            bool isDuplicate = fileReceipts.Any(x => x.Md5 == hash);

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
                case ArchiveFileType.SviexMesh:
                    target = ArchiveFileType.Sviex2Mesh;
                    break;
                case ArchiveFileType.XaAnimation:
                    target = ArchiveFileType.Xa2Animation;
                    break;
                default:
                    target = file.Type;
                    break;
            }

            Stream dataStream = null;
            string internalName;
            string emulatedName;

            if (target == file.Type)
            {
                internalName = file.Name;
                emulatedName = file.EmulatedName;

                if (!isDuplicate)
                    dataStream = file.Source.GetStream();
            }
            else
            {
                using (IEncoder decoder = EncoderFactory.GetEncoder(file.Source.GetStream(), writer, file.Type))
                using (IEncoder encoder = EncoderFactory.GetEncoder(decoder.Decode(), writer, target))
                {
                    internalName = encoder.NameTransform(file.Name);

                    switch (encoder.DataType)
                    {
                        case ArchiveDataType.Audio:
                            emulatedName = $"{file.Name.Substring(0, file.Name.LastIndexOf('.'))}.wav";
                            break;
                        case ArchiveDataType.Mesh:
                            emulatedName = $"{file.Name.Substring(0, file.Name.LastIndexOf('.'))}.xx";
                            break;
                        case ArchiveDataType.Sviex:
                            emulatedName = $"{file.Name.Substring(0, file.Name.LastIndexOf('.'))}.sviex";
                            break;
                        default:
                            emulatedName = file.EmulatedName;
                            break;
                    }

                    if (!isDuplicate)
                        dataStream = encoder.Encode();
                }
            }

            if (isDuplicate)
            {
                FileReceipt original = fileReceipts.First(x => x.Md5 == hash);

                FileReceipt duplicate = FileReceipt.CreateDuplicate(original, file);

                duplicate.InternalName = internalName;
                duplicate.EmulatedName = emulatedName;

                fileReceipts.Add(duplicate);

                return true;
            }

            using (dataStream)
            {
                if (continueAnyway ||
                    UncompressedStream.Length == 0 ||
                    (ulong)(dataStream.Length + UncompressedStream.Length) <= MaxChunkSize)
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

        public void Compress(IEnumerable<ICompressor> compressors)
        {
            UncompressedStream.SetLength(UncompressedStream.Length);
            UncompressedStream.Position = 0;
            ICompressor compressor = compressors.First(x => x.Compression == Compression);

            using (UncompressedStream)
            {
                CompressedStream = compressor.GetStream(UncompressedStream);

                uint crc = CRC32.Compute(CompressedStream);

                Receipt = new ChunkReceipt
                {
                    ID = this.ID,
                    Compression = compressor.Compression,
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

        public Stream GetData(IEnumerable<ICompressor> compressors)
        {
            if (!IsReady)
                Compress(compressors);

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
        public ArchiveChunkCompression Compression;
        public uint CRC;
        public ulong FileOffset;
        public ulong CompressedSize;
        public ulong UncompressedSize;

        public ICollection<FileReceipt> FileReceipts;
    }
}
