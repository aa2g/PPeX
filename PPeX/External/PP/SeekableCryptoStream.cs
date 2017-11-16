using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.External.PP
{
    public class SeekableCryptoStream : CryptoStream
    {
        private Stream _stream;
        public SeekableCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode) : base(stream, transform, mode)
        {
            _stream = stream;
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }

            set
            {
                _stream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override long Length => _stream.Length;

        public override bool CanSeek => true;
    }
}
