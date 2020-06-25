using System;
using System.IO;

namespace PPeX.External.PP.Crypto
{
	public class MemoryCryptoStream : Stream
	{
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => BaseStream.Length;
		public override long Position { get => BaseStream.Position; set => throw new NotSupportedException(); }

		protected Stream BaseStream { get; set; }
		protected ISpanCryptoTransform Transform { get; set; }

		public MemoryCryptoStream(Stream baseStream, ISpanCryptoTransform transform)
		{
			BaseStream = baseStream;
			Transform = transform;
		}


		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}
	}
}