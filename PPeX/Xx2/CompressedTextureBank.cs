using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Compressors;
using System.IO;

namespace PPeX.Xx2
{
    public class CompressedTextureBank : TextureBank
    {
        ArchiveChunkCompression compression;

        public CompressedTextureBank(ArchiveChunkCompression compression)
        {
            this.compression = compression;
        }

        protected byte[] GetCompressed(byte[] source)
        {
            using (MemoryStream stream = new MemoryStream(source))
            using (ICompressor compressor = CompressorFactory.GetCompressor(stream, compression))
            using (MemoryStream destination = new MemoryStream())
            {
                compressor.WriteToStream(destination);
                return destination.ToArray();
            }
        }

        protected byte[] GetDecompressed(byte[] compressed)
        {
            using (MemoryStream stream = new MemoryStream(compressed))
            using (IDecompressor decompressor = CompressorFactory.GetDecompressor(stream, compression))
            using (MemoryStream destination = new MemoryStream())
            {
                decompressor.Decompress().CopyTo(destination);
                return destination.ToArray();
            }
        }

        public void AddRaw(string name, byte[] data)
        {
            base[name] = data;
        }

        public override byte[] this[string name]
        {
            get => GetDecompressed(base[name]);
            set => base[name] = GetCompressed(value);
        }
    }
}
