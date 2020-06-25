using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PPeX
{
	public class PartialStream : Stream
	{
		public override bool CanRead => BaseStream.CanRead;

		public override bool CanSeek => BaseStream.CanSeek;

		public override bool CanWrite => false;

		public override long Length => _length;

		public override long Position
		{
			get => _position;
			set => Seek(value, SeekOrigin.Begin);
		}

		public bool KeepOpen { get; set; }

		private readonly Stream BaseStream = null;

		private long _position = 0;
		private readonly long _offset;
		private readonly long _length;

		public PartialStream(Stream baseStream, long length) : this (baseStream, null, length) { }

		public PartialStream(Stream baseStream, long position, long length) : this(baseStream, (long?)position, length) { }

		public PartialStream(Stream baseStream, long? position, long length, bool keepOpen = false)
		{
			if (length + baseStream.Position > baseStream.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			BaseStream = baseStream;
			KeepOpen = keepOpen;

			if (position.HasValue)
				baseStream.Position = position.Value;

			_offset = baseStream.Position;
			_length = length;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int readCount = (int)Math.Min(count, Length - Position);

			if (readCount <= 0)
				return 0;

			int result = BaseStream.Read(buffer, offset, readCount);

			Position += result;

			return result;
		}

		public override int Read(Span<byte> buffer)
		{
			int readCount = (int)Math.Min(buffer.Length, Length - Position);

			if (readCount <= 0)
				return 0;

			int result = BaseStream.Read(buffer.Slice(0, readCount));

			Position += result;

			return result;
		}

		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			int readCount = (int)Math.Min(count, Length - Position);

			if (readCount <= 0)
				return 0;

			int result = await BaseStream.ReadAsync(buffer, offset, count, cancellationToken);

			Position += result;

			return result;
		}

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
		{
			int readCount = (int)Math.Min(buffer.Length, Length - Position);

			if (readCount <= 0)
				return 0;

			int result = await BaseStream.ReadAsync(buffer.Slice(0, readCount), cancellationToken);

			Position += result;

			return result;
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

			if (newPosition < 0 || newPosition > _length)
				throw new ArgumentOutOfRangeException(nameof(offset));

			BaseStream.Position = newPosition + _offset;
			_position = newPosition;

			return Position;
		}

		public override void Flush()
		{
			BaseStream.Flush();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void Close()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (KeepOpen)
				return;

			try
			{
				if (disposing)
				{
					BaseStream.Close();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}