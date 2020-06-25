using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PPeX.External.Zstandard
{
	internal static unsafe class ExternMethods
	{
        static ExternMethods()
        {
	        var libzstd = new Dictionary<string, List<DynDllMapping>>
	        {
		        ["libzstd"] = new List<DynDllMapping>
		        {
                    "libzstd32.dll",
                    "libzstd64.dll",
                    "libzstd.so.1"
                }
	        };

	        typeof(ExternMethods).ResolveDynDllImports(libzstd);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdTrainDictDelegate(byte[] dictBuffer, UIntPtr dictBufferCapacity, byte[] samplesBuffer, UIntPtr[] samplesSizes, uint nbSamples);
        [DynDllImport("libzstd")]
        public static ZstdTrainDictDelegate ZDICT_trainFromBuffer;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint ZstdIsErrorDictDelegate(UIntPtr code);
        [DynDllImport("libzstd")]
        public static ZstdIsErrorDictDelegate ZDICT_isError;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZstdGetErrorNameDictDelegate(UIntPtr code);
        [DynDllImport("libzstd")]
        public static ZstdGetErrorNameDictDelegate ZDICT_getErrorName;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZstdCreateCompressorDelegate();
        [DynDllImport("libzstd")]
        public static ZstdCreateCompressorDelegate ZSTD_createCCtx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdFreeCompressorDelegate(IntPtr cctx);
        [DynDllImport("libzstd")]
        public static ZstdFreeCompressorDelegate ZSTD_freeCCtx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZstdCreateDecompressorDelegate();
        [DynDllImport("libzstd")]
        public static ZstdCreateDecompressorDelegate ZSTD_createDCtx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdFreeDecompressorDelegate(IntPtr cctx);
        [DynDllImport("libzstd")]
        public static ZstdFreeDecompressorDelegate ZSTD_freeDCtx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdCompressDelegate(IntPtr ctx, byte* dst, UIntPtr dstCapacity, byte* src, UIntPtr srcSize, int compressionLevel);
        [DynDllImport("libzstd")]
        public static ZstdCompressDelegate ZSTD_compressCCtx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdDecompressDelegate(IntPtr ctx, byte* dst, UIntPtr dstCapacity, byte* src, UIntPtr srcSize);
        [DynDllImport("libzstd")]
        public static ZstdDecompressDelegate ZSTD_decompressDCtx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZstdCreateDictDelegate(byte* dict, UIntPtr dictSize, int compressionLevel);
        [DynDllImport("libzstd")]
        public static ZstdCreateDictDelegate ZSTD_createCDict;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdFreeDictDelegate(IntPtr cdict);
        [DynDllImport("libzstd")]
        public static ZstdFreeDictDelegate ZSTD_freeCDict;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdCompressWithDictDelegate(IntPtr cctx, byte* dst, UIntPtr dstCapacity, byte* src, UIntPtr srcSize, IntPtr cdict);
        [DynDllImport("libzstd")]
        public static ZstdCompressWithDictDelegate ZSTD_compress_usingCDict;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZstdCreateDecompDictDelegate(byte* dict, UIntPtr dictSize);
        [DynDllImport("libzstd")]
        public static ZstdCreateDecompDictDelegate ZSTD_createDDict;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdFreeDecompDictDelegate(IntPtr ddict);
        [DynDllImport("libzstd")]
        public static ZstdFreeDecompDictDelegate ZSTD_freeDDict;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdDecompressWithDictDelegate(IntPtr dctx, byte* dst, UIntPtr dstCapacity, byte* src, UIntPtr srcSize, IntPtr ddict);
        [DynDllImport("libzstd")]
        public static ZstdDecompressWithDictDelegate ZSTD_decompress_usingDDict;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate ulong ZstdGetDecompressedSizeDelegate(byte* src, UIntPtr srcSize);
        [DynDllImport("libzstd")]
        public static ZstdGetDecompressedSizeDelegate ZSTD_getDecompressedSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZstdGetMaxCompLevelDelegate();
        [DynDllImport("libzstd")]
        public static ZstdGetMaxCompLevelDelegate ZSTD_maxCLevel;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr ZstdGetCompressionBoundDelegate(UIntPtr srcSize);
        [DynDllImport("libzstd")]
        public static ZstdGetCompressionBoundDelegate ZSTD_compressBound;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint ZstdIsErrorDelegate(UIntPtr code);
        [DynDllImport("libzstd")]
        public static ZstdIsErrorDelegate ZSTD_isError;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZstdGetErrorNameDelegate(UIntPtr code);
        [DynDllImport("libzstd")]
        public static ZstdGetErrorNameDelegate ZSTD_getErrorName;
    }
}