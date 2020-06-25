using System;
using System.IO;

namespace PPeX.Common
{
	public class MemorySpanStream : Stream
	{
		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => true;
		public override long Length { get; }
		public override long Position { get; set; }

		public Memory<byte> Data { get; }

		public MemorySpanStream(Memory<byte> data)
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

			Data.Span.CopyTo(buffer.Slice(0, readCount));

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

		public override void Write(byte[] buffer, int offset, int count)
			=> Write(buffer.AsSpan(offset, count));

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			if (!buffer.TryCopyTo(Data.Span.Slice((int)Position)))
				throw new EndOfStreamException("Supplied memory buffer is not large enough");

			Position += buffer.Length;
		}

		public Memory<byte> SliceToCurrentPosition()
		{
			return Data.Slice(0, (int)Position);
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			
		}
	}
}