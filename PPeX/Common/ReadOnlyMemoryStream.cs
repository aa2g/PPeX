using System;
using System.IO;

namespace PPeX.Common
{
	public class ReadOnlyMemoryStream : Stream
	{
		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length { get; }
		public override long Position { get; set; }

		public ReadOnlyMemory<byte> Data { get; }

		public ReadOnlyMemoryStream(ReadOnlyMemory<byte> data)
		{
			Data = data;

			Length = data.Length;
		}

		public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

		public override int Read(Span<byte> buffer)
		{
			int readCount = (int)Math.Min(buffer.Length, Length - Position);

			if (readCount <= 0)
				return 0;

			Data.Span.Slice((int)Position, readCount).CopyTo(buffer.Slice(0, readCount));

			Position += readCount;

			return readCount;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long newPosition;

			switch (origin)
			{
				case SeekOrigin.Begin:
					newPosition = offset;
					break;

				case SeekOrigin.Current:
					newPosition = Position + offset;
					break;

				case SeekOrigin.End:
					newPosition = Length + offset;
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
			}

			if (newPosition < 0 || newPosition >= Length)
				throw new ArgumentOutOfRangeException(nameof(offset));

			Position = newPosition;

			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}
	}
}