using System;

namespace PPeX.External.Zstandard
{
	public class ZstdDecompressionOptions : IDisposable
	{
		public unsafe ZstdDecompressionOptions(byte[] dict)
		{
			Dictionary = dict;

			fixed (byte* ptr = dict)
			{
				if (dict != null)
					Ddict = ExternMethods.ZSTD_createDDict(ptr, (UIntPtr)dict.Length).EnsureZstdSuccess();
				else
					GC.SuppressFinalize(this); // No unmanaged resources
			}
		}

		~ZstdDecompressionOptions()
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
			if (Ddict != IntPtr.Zero)
			{
				ExternMethods.ZSTD_freeDDict(Ddict);
				Ddict = IntPtr.Zero;
			}
		}

		public readonly byte[] Dictionary;

		internal IntPtr Ddict;
	}
}
