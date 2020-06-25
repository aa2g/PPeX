using System;

namespace PPeX.External.Zstandard
{
	public class ZstdCompressionOptions : IDisposable
	{
		public static int MaxCompressionLevel { get; } = ExternMethods.ZSTD_maxCLevel();

		public const int DefaultCompressionLevel = 3; // Used by zstd utility by default

		public unsafe ZstdCompressionOptions(byte[] dict, int compressionLevel)
		{
			Dictionary = dict;

			fixed (byte* ptr = dict)
			{
				if (dict != null)
					Cdict = ExternMethods.ZSTD_createCDict(ptr, (UIntPtr)dict.Length, compressionLevel).EnsureZstdSuccess();
				else
					GC.SuppressFinalize(this); // No unmanaged resources
			}
		}

		public ZstdCompressionOptions()
		{
			Dictionary = null;
		}

		~ZstdCompressionOptions()
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
			if (Cdict != IntPtr.Zero)
			{
				ExternMethods.ZSTD_freeCDict(Cdict);
				Cdict = IntPtr.Zero;
			}
		}

		public readonly byte[] Dictionary;

		internal IntPtr Cdict;
	}
}
