using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX;
using PPeX.Compressors;
using PPeX.Encoders;

namespace PPeXM64
{
    /// <summary>
    /// A cached version of a chunk to be kept in memory.
    /// </summary>
    public class CachedChunk
    {
        public byte[] Data;
        public uint Accesses;
        public uint ID => BaseChunk.ID;
        public ExtendedArchiveChunk BaseChunk;

        public CachedChunk(ExtendedArchiveChunk baseChunk)
        {
            BaseChunk = baseChunk;
            Accesses = 0;
            Data = null;
        }


        object _allocLock = new object();

        public void Allocate()
        {
            lock (_allocLock)
            {
                Data = new byte[BaseChunk.CompressedLength];

                using (Stream fstream = BaseChunk.GetRawStream())
                {
                    fstream.Read(Data, 0, (int)BaseChunk.CompressedLength);
                }
            }
        }

        public void Deallocate()
        {
            lock (_allocLock)
            {
                Data = null;
            }
        }

        protected Stream decompress(int offset, int length)
        {
            lock (_allocLock)
            {
                if (Data == null)
                    Allocate();

                if (length < 0)
                    length = Data.Length;

                MemoryStream output = new MemoryStream();

                using (MemoryStream input = new MemoryStream(Data))
                using (IDecompressor decompressor = CompressorFactory.GetDecompressor(input, BaseChunk.Compression))
                using (Stream decomp = decompressor.Decompress())
                    decomp.CopyTo(output, offset, length);

                Accesses++;

                output.Position = 0;
                return output;
            }
                
        }

        public Stream GetStream()
        {
            return decompress(0, -1);
        }

        public Stream GetStream(int offset, int length)
        {
            return decompress(offset, length);
        }
    }

    public class CachedFile
    {
        public string Name { get; protected set; }

        public string ArchiveName { get; protected set; }

        public byte[] MD5 { get; protected set; }

        public ArchiveFileType Type { get; protected set; }


        public int Offset { get; protected set; }

        public int Length { get; protected set; }


        public CachedChunk Chunk;
        public ArchiveFileSource Source;

        public CachedFile(ArchiveFileSource source, CachedChunk chunk)
        {
            Source = source;
            Chunk = chunk;

            Name = source.Name;
            ArchiveName = source.ArchiveName;
            MD5 = source.Md5;
            Type = source.Type;

            Offset = (int)source.Offset;
            Length = (int)source.Size;
        }

        public Stream GetStream()
        {
            //using (IDecoder decoder = EncoderFactory.GetDecoder(Chunk.GetStream(Offset, Length), Type))
            IDecoder decoder = EncoderFactory.GetDecoder(Chunk.GetStream(Offset, Length), Type);
            //{
            return decoder.Decode();
            //}
        }
    }
}
