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
        ICompressor compressor;
        IDecompressor decompressor;

        public CompressedTextureBank(ArchiveChunkCompression compression)
        {
            compressor = CompressorFactory.GetCompressor(compression);
            decompressor = CompressorFactory.GetDecompressor(compression);
        }

        protected byte[] GetCompressed(byte[] source)
        {
            using (MemoryStream stream = new MemoryStream(source))
            using (MemoryStream destination = new MemoryStream())
            {
                compressor.WriteToStream(stream, destination);
                return destination.ToArray();
            }
        }

        protected byte[] GetDecompressed(byte[] compressed)
        {
            using (MemoryStream stream = new MemoryStream(compressed))
            using (MemoryStream destination = new MemoryStream())
            {
                decompressor.Decompress(stream).CopyTo(destination);
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
