using System;

namespace PPeX.External.Zstandard
{
	public class ZstdCompressor : IDisposable
	{
		public ZstdCompressor() : this(new ZstdCompressionOptions())
		{
		}

		public ZstdCompressor(ZstdCompressionOptions options)
		{
			Options = options;

			cctx = ExternMethods.ZSTD_createCCtx().EnsureZstdSuccess();
		}

		~ZstdCompressor()
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

			ExternMethods.ZSTD_freeCCtx(cctx);

			disposed = true;
		}

		private bool disposed = false;

		public Memory<byte> Wrap(ReadOnlySpan<byte> src, int compressionLevel = 3, bool resizeOutput = true)
		{
			if (src.Length == 0)
				return new byte[0];

			var dstCapacity = GetCompressBound(src.Length);
			var dst = new byte[dstCapacity];

			var dstSize = Wrap(src, dst, compressionLevel);

			if (dstCapacity == dstSize)
				return dst;

			if (!resizeOutput)
				return new Memory<byte>(dst, 0, dstSize);

			var result = new byte[dstSize];

			Buffer.BlockCopy(dst, 0, result, 0, dstSize);

			return result;
		}

		public static int GetCompressBound(int size)
		{
			return (int) ExternMethods.ZSTD_compressBound((UIntPtr) size);
		}

		public unsafe int Wrap(ReadOnlySpan<byte> src, Span<byte> dst, int compressionLevel = 3)
		{
			if (src.Length == 0)
				return 0;

			UIntPtr dstSize;

			fixed (byte* dstPtr = dst)
			fixed (byte* srcPtr = src)
			{
				if (Options.Cdict == IntPtr.Zero)
					dstSize = ExternMethods.ZSTD_compressCCtx(cctx, dstPtr, (UIntPtr)dst.Length, srcPtr, (UIntPtr)src.Length,
						compressionLevel);
				else
					dstSize = ExternMethods.ZSTD_compress_usingCDict(cctx, dstPtr, (UIntPtr)dst.Length, srcPtr,
						(UIntPtr)src.Length, Options.Cdict);
			}

			ReturnValueExtensions.EnsureZdictSuccess(dstSize);
			return (int) dstSize;
		}

		public readonly ZstdCompressionOptions Options;

		private readonly IntPtr cctx;
	}
}