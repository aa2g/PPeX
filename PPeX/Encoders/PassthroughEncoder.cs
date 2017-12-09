using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class PassthroughEncoder : IEncoder
    {
        public Stream BaseStream { get; set; }

        public PassthroughEncoder(Stream baseStream)
        {
            BaseStream = baseStream;
        }

        public uint EncodedLength => (uint)BaseStream.Length;

        public ArchiveFileType Encoding => ArchiveFileType.Raw;

        public ArchiveDataType DataType => ArchiveDataType.Raw;

        public Stream Encode()
        {
            byte[] buffer = new byte[BaseStream.Length];
            BaseStream.Read(buffer, 0, (int)BaseStream.Length);

            return new MemoryStream(buffer);
        }

        public Stream Decode()
        {
            return Encode();
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
