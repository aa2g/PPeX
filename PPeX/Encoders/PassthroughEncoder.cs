using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class PassthroughEncoder : IEncoder, IDecoder
    {
        public Stream BaseStream { get; set; }

        public PassthroughEncoder(Stream baseStream)
        {
            BaseStream = baseStream;
        }

        public uint EncodedLength => (uint)BaseStream.Length;

        public ArchiveFileEncoding Encoding => ArchiveFileEncoding.Raw;

        public Stream Decode()
        {
            return BaseStream;
        }

        public Stream Encode()
        {
            return BaseStream;
        }

        public string NameTransform(string original)
        {
            return original;
        }

        public void Dispose()
        {
            BaseStream.Dispose();
        }
    }
}
