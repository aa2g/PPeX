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
    

    public class CachedFile
    {
        public string Name { get; protected set; }

        public string ArchiveName { get; protected set; }

        public ArchiveFileSource Source { get; protected set; }

        public CompressedCache Cache { get; protected set; }

        public CachedChunk Chunk { get; protected set; }

        public byte[] MD5 => Source.Md5;

        public ArchiveFileType Type => Source.Type;

        public int Length { get; protected set; }

        public ArchiveChunkCompression Compression { get; set; }

        public byte[] CompressedData { get; set; }

        public bool Allocated => CompressedData != null;

        public int Accesses { get; protected set; }

        public CachedFile(ArchiveFileSource source, CompressedCache cache, CachedChunk chunk)
        {
            Source = source;
            Cache = cache;
            Chunk = chunk;

            Name = source.Name;
            ArchiveName = source.ArchiveName;

            //Offset = (int)source.Offset;
            Length = (int)source.Size;

            Accesses = 0;
        }

        public void Allocate()
        {
            if (!Allocated)
                Chunk.Allocate();
        }

        public void Deallocate()
        {
            CompressedData = null;

            Accesses = 0;
        }

        public Stream GetStream()
        {
            Allocate();

            Accesses++;

            using (MemoryStream mem = new MemoryStream(CompressedData))
            using (IDecompressor decompressor = CompressorFactory.GetDecompressor(mem, Compression))
            {
                if (Type == ArchiveFileType.Xx3Mesh)
                {
                    IEncoder decoder = new Xx3Encoder(decompressor.Decompress(), Cache.UniversalTexBank);
                    
                    Stream decoded = decoder.Decode();
                    decoded.Position = 0;

                    return decoded;
                }
                else
                {
                    IEncoder decoder = EncoderFactory.GetEncoder(decompressor.Decompress(), Source.BaseArchive, Type);

                    Stream decoded = decoder.Decode();
                    decoded.Position = 0;

                    return decoded;
                }
            }
        }
    }
}
