using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using size_t = System.UIntPtr;

namespace ZstdNet
{
	internal static class ExternMethods
	{
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZSTD_Buffer
        {
            public IntPtr array;
            public size_t size;
            public size_t pos;

            /*IntPtr pointer;

            public static implicit operator IntPtr(ZSTD_Buffer buffer)
            {
                if (buffer.pointer != null)
                    return buffer.pointer;


            }*/
        }

        static readonly bool Is64Bit;

        static ExternMethods()
		{
            Is64Bit = Environment.Is64BitProcess;
        }

        #region Mixed
        public static size_t ZDICT_trainFromBuffer(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples)
        {
            if (Is64Bit)
                return ZDICT_trainFromBuffer_64(dictBuffer, dictBufferCapacity, samplesBuffer, samplesSizes, nbSamples);
            else
                return ZDICT_trainFromBuffer_32(dictBuffer, dictBufferCapacity, samplesBuffer, samplesSizes, nbSamples);
        }
        public static uint ZDICT_isError(size_t code)
        {
            if (Is64Bit)
                return ZDICT_isError_64(code);
            else
                return ZDICT_isError_32(code);
        }
        public static IntPtr ZDICT_getErrorName(size_t code)
        {
            if (Is64Bit)
                return ZDICT_getErrorName_64(code);
            else
                return ZDICT_getErrorName_32(code);
        }
        
        public static IntPtr ZSTD_createCCtx()
        {
            if (Is64Bit)
                return ZSTD_createCCtx_64();
            else
                return ZSTD_createCCtx_32();
        }
        public static size_t ZSTD_freeCCtx(IntPtr cctx)
        {
            if (Is64Bit)
                return ZSTD_freeCCtx_64(cctx);
            else
                return ZSTD_freeCCtx_32(cctx);
        }
        
        public static IntPtr ZSTD_createDCtx()
        {
            if (Is64Bit)
                return ZSTD_createDCtx_64();
            else
                return ZSTD_createDCtx_32();
        }
        public static size_t ZSTD_freeDCtx(IntPtr cctx)
        {
            if (Is64Bit)
                return ZSTD_freeDCtx_64(cctx);
            else
                return ZSTD_freeDCtx_32(cctx);
        }
        
        public static size_t ZSTD_compressCCtx(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel)
        {
            if (Is64Bit)
                return ZSTD_compressCCtx_64(ctx, dst, dstCapacity, src, srcSize, compressionLevel);
            else
                return ZSTD_compressCCtx_32(ctx, dst, dstCapacity, src, srcSize, compressionLevel);
        }
        public static size_t ZSTD_decompressDCtx(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize)
        {
            if (Is64Bit)
                return ZSTD_decompressDCtx_64(ctx, dst, dstCapacity, src, srcSize);
            else
                return ZSTD_decompressDCtx_32(ctx, dst, dstCapacity, src, srcSize);
        }
        
        public static IntPtr ZSTD_createCDict(byte[] dict, size_t dictSize, int compressionLevel)
        {
            if (Is64Bit)
                return ZSTD_createCDict_64(dict, dictSize, compressionLevel);
            else
                return ZSTD_createCDict_32(dict, dictSize, compressionLevel);
        }
        public static size_t ZSTD_freeCDict(IntPtr cdict)
        {
            if (Is64Bit)
                return ZSTD_freeCDict_64(cdict);
            else
                return ZSTD_freeCDict_32(cdict);
        }
        public static size_t ZSTD_compress_usingCDict(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict)
        {
            if (Is64Bit)
                return ZSTD_compress_usingCDict_64(cctx, dst, dstCapacity, src, srcSize, cdict);
            else
                return ZSTD_compress_usingCDict_32(cctx, dst, dstCapacity, src, srcSize, cdict);
        }
        
        public static IntPtr ZSTD_createDDict(byte[] dict, size_t dictSize)
        {
            if (Is64Bit)
                return ZSTD_createDDict_64(dict, dictSize);
            else
                return ZSTD_createDDict_32(dict, dictSize);
        }
        public static size_t ZSTD_freeDDict(IntPtr ddict)
        {
            if (Is64Bit)
                return ZSTD_freeDDict_64(ddict);
            else
                return ZSTD_freeDDict_32(ddict);
        }
        public static size_t ZSTD_decompress_usingDDict(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict)
        {
            if (Is64Bit)
                return ZSTD_decompress_usingDDict_64(dctx, dst, dstCapacity, src, srcSize, ddict);
            else
                return ZSTD_decompress_usingDDict_32(dctx, dst, dstCapacity, src, srcSize, ddict);
        }
        
        public static ulong ZSTD_getDecompressedSize(IntPtr src, size_t srcSize)
        {
            if (Is64Bit)
                return ZSTD_getDecompressedSize_64(src, srcSize);
            else
                return ZSTD_getDecompressedSize_32(src, srcSize);
        }
        
        public static int ZSTD_maxCLevel()
        {
            if (Is64Bit)
                return ZSTD_maxCLevel_64();
            else
                return ZSTD_maxCLevel_32();
        }
        public static size_t ZSTD_compressBound(size_t srcSize)
        {
            if (Is64Bit)
                return ZSTD_compressBound_64(srcSize);
            else
                return ZSTD_compressBound_32(srcSize);
        }
        public static uint ZSTD_isError(size_t code)
        {
            if (Is64Bit)
                return ZSTD_isError_64(code);
            else
                return ZSTD_isError_32(code);
        }
        public static IntPtr ZSTD_getErrorName(size_t code)
        {
            if (Is64Bit)
                return ZSTD_getErrorName_64(code);
            else
                return ZSTD_getErrorName_32(code);
        }
        #endregion

        #region x64

        [DllImport("libzstd64.dll", EntryPoint = "ZDICT_trainFromBuffer", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZDICT_trainFromBuffer_64(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples);
        [DllImport("libzstd64.dll", EntryPoint = "ZDICT_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZDICT_isError_64(size_t code);
        [DllImport("libzstd64.dll", EntryPoint = "ZDICT_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZDICT_getErrorName_64(size_t code);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_createCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCCtx_64();
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_freeCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCCtx_64(IntPtr cctx);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_createDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDCtx_64();
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_freeDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDCtx_64(IntPtr cctx);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_compressCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressCCtx_64(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_decompressDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompressDCtx_64(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_createCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCDict_64(byte[] dict, size_t dictSize, int compressionLevel);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_freeCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCDict_64(IntPtr cdict);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_compress_usingCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compress_usingCDict_64(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_createDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDDict_64(byte[] dict, size_t dictSize);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_freeDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDDict_64(IntPtr ddict);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_decompress_usingDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompress_usingDDict_64(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_getDecompressedSize", CallingConvention = CallingConvention.Cdecl)]
        static extern ulong ZSTD_getDecompressedSize_64(IntPtr src, size_t srcSize);

        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_maxCLevel", CallingConvention = CallingConvention.Cdecl)]
        static extern int ZSTD_maxCLevel_64();
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_compressBound", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressBound_64(size_t srcSize);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZSTD_isError_64(size_t code);
        [DllImport("libzstd64.dll", EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_getErrorName_64(size_t code);

        #endregion

        #region x86

        [DllImport("libzstd32.dll", EntryPoint = "ZDICT_trainFromBuffer", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZDICT_trainFromBuffer_32(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples);
        [DllImport("libzstd32.dll", EntryPoint = "ZDICT_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZDICT_isError_32(size_t code);
        [DllImport("libzstd32.dll", EntryPoint = "ZDICT_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZDICT_getErrorName_32(size_t code);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_createCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCCtx_32();
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_freeCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCCtx_32(IntPtr cctx);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_createDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDCtx_32();
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_freeDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDCtx_32(IntPtr cctx);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_compressCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressCCtx_32(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_decompressDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompressDCtx_32(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_createCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCDict_32(byte[] dict, size_t dictSize, int compressionLevel);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_freeCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCDict_32(IntPtr cdict);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_compress_usingCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compress_usingCDict_32(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_createDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDDict_32(byte[] dict, size_t dictSize);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_freeDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDDict_32(IntPtr ddict);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_decompress_usingDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompress_usingDDict_32(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_getDecompressedSize", CallingConvention = CallingConvention.Cdecl)]
        static extern ulong ZSTD_getDecompressedSize_32(IntPtr src, size_t srcSize);

        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_maxCLevel", CallingConvention = CallingConvention.Cdecl)]
        static extern int ZSTD_maxCLevel_32();
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_compressBound", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressBound_32(size_t srcSize);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZSTD_isError_32(size_t code);
        [DllImport("libzstd32.dll", EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_getErrorName_32(size_t code);

        #endregion
    }
}
