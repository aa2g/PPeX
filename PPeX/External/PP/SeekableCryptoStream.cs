using System.IO;
using System.Security.Cryptography;

namespace PPeX.External.PP
{
	public class SeekableCryptoStream : CryptoStream
	{
		public Stream BaseStream { get; set; }

		public SeekableCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode) : base(stream,
			transform, mode)
		{
			BaseStream = stream;
		}

		public override long Length => BaseStream.Length;
		public override bool CanSeek => true;

		public override long Position
		{
			get => BaseStream.Position;
			set => BaseStream.Position = value;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return BaseStream.Seek(offset, origin);
		}
	}
}