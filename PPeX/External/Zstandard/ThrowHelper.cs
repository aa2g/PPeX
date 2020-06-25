using System;
using System.Runtime.InteropServices;

namespace PPeX.External.Zstandard
{
	internal static class ReturnValueExtensions
	{
		internal static UIntPtr EnsureZdictSuccess(UIntPtr returnValue)
		{
			if(ExternMethods.ZDICT_isError(returnValue) != 0)
				ThrowException(returnValue, Marshal.PtrToStringAnsi(ExternMethods.ZDICT_getErrorName(returnValue)));
			return returnValue;
		}

		internal static UIntPtr EnsureZstdSuccess(UIntPtr returnValue)
		{
			if(ExternMethods.ZSTD_isError(returnValue) != 0)
				ThrowException(returnValue, Marshal.PtrToStringAnsi(ExternMethods.ZSTD_getErrorName(returnValue)));
			return returnValue;
		}

		private static void ThrowException(UIntPtr returnValue, string message)
		{
			var code = unchecked(0 - (uint) (ulong) returnValue); // Negate returnValue (UintPtr)
			if(code == ZSTD_error_dstUIntPtrooSmall)
				throw new InsufficientMemoryException(message);
			throw new ZstdException(message);
		}

		private const int ZSTD_error_dstUIntPtrooSmall = 70;

		public static IntPtr EnsureZstdSuccess(this IntPtr returnValue)
		{
			if (returnValue == IntPtr.Zero)
				throw new ZstdException("Failed to create a structure");
			return returnValue;
		}
	}

	public class ZstdException : Exception
	{
		public ZstdException(string message): base(message) { }
	}
}
