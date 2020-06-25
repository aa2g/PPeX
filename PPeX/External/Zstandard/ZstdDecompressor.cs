using System;

namespace PPeX.External.Zstandard
{
	public class ZstdDecompressor : IDisposable
	{
		public ZstdDecompressor()
			: this(new ZstdDecompressionOptions(null))
		{
		}

		public ZstdDecompressor(ZstdDecompressionOptions options)
		{
			Options = options;

			dctx = ExternMethods.ZSTD_createDCtx().EnsureZstdSuccess();
		}

		~ZstdDecompressor()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposed)
				return;

			ExternMethods.ZSTD_freeDCtx(dctx);

			disposed = true;
		}

		private bool disposed = false;


		public byte[] Unwrap(ReadOnlySpan<byte> src, int maxDecompressedSize = int.MaxValue)
		{
			if (src.Length == 0)
				return new byte[0];

			var expectedDstSize = GetDecompressedSize(src);

			if (expectedDstSize == 0)
				throw new ZstdException("Can't create buffer for data with unspecified decompressed size (provide your own buffer to Unwrap instead)");

			if (expectedDstSize > (ulong) maxDecompressedSize)
				throw new ArgumentOutOfRangeException($"Decompressed size is too big ({expectedDstSize} bytes > authorized {maxDecompressedSize} bytes)");

			var dst = new byte[expectedDstSize];

			int dstSize;
			try
			{
				dstSize = Unwrap(src, dst, false);
			}
			catch (InsufficientMemoryException)
			{
				throw new ZstdException("Invalid decompressed size");
			}

			if ((int) expectedDstSize != dstSize)
				throw new ZstdException("Invalid decompressed size specified in the data");

			return dst;
		}

		public static unsafe ulong GetDecompressedSize(ReadOnlySpan<byte> src)
		{
			fixed (byte* srcPtr = src)
				return ExternMethods.ZSTD_getDecompressedSize(srcPtr, (UIntPtr)src.Length);
		}

		public unsafe int Unwrap(ReadOnlySpan<byte> src, Span<byte> dst, bool bufferSizePrecheck = true)
		{
			if (src.Length == 0)
				return 0;

			if (bufferSizePrecheck)
			{
				if (GetDecompressedSize(src) > (ulong)dst.Length)
					throw new InsufficientMemoryException(
						"Buffer size is less than specified decompressed data size");
			}

			UIntPtr dstSize;


			fixed (byte* dstPtr = dst)
			fixed (byte* srcPtr = src)
			{
				if (Options.Ddict == IntPtr.Zero)
					dstSize = ExternMethods.ZSTD_decompressDCtx(dctx, dstPtr, (UIntPtr)dst.Length, srcPtr,
						(UIntPtr)src.Length);
				else
					dstSize = ExternMethods.ZSTD_decompress_usingDDict(dctx, dstPtr, (UIntPtr)dst.Length, srcPtr,
						(UIntPtr)src.Length, Options.Ddict);
			}

			ReturnValueExtensions.EnsureZstdSuccess(dstSize);
			return (int)dstSize;
		}

		public readonly ZstdDecompressionOptions Options;

		private readonly IntPtr dctx;
	}
}