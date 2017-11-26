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

        const string dll32 = "libzstd32.dll";
        const string dll64 = "libzstd64.dll";
        const string linuxso = "libzstd.so";

        private enum Platform
        {
            win32,
            win64,
            linux
        }

        private static readonly Platform currentPlatform;

        static ExternMethods()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                if (Environment.Is64BitProcess)
                    currentPlatform = Platform.win64;
                else
                    currentPlatform = Platform.win32;
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
                currentPlatform = Platform.linux;
            else
                throw new PlatformNotSupportedException();
        }

        #region Auto
        public static size_t ZDICT_trainFromBuffer(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZDICT_trainFromBuffer_32(dictBuffer, dictBufferCapacity, samplesBuffer, samplesSizes, nbSamples);
                case Platform.win64:
                    return ZDICT_trainFromBuffer_64(dictBuffer, dictBufferCapacity, samplesBuffer, samplesSizes, nbSamples);
                case Platform.linux:
                    return ZDICT_trainFromBuffer_so(dictBuffer, dictBufferCapacity, samplesBuffer, samplesSizes, nbSamples);
            }

            throw new PlatformNotSupportedException();
        }
        public static uint ZDICT_isError(size_t code)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZDICT_isError_32(code);
                case Platform.win64:
                    return ZDICT_isError_64(code);
                case Platform.linux:
                    return ZDICT_isError_so(code);
            }

            throw new PlatformNotSupportedException();
        }
        public static IntPtr ZDICT_getErrorName(size_t code)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZDICT_getErrorName_32(code);
                case Platform.win64:
                    return ZDICT_getErrorName_64(code);
                case Platform.linux:
                    return ZDICT_getErrorName_so(code);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static IntPtr ZSTD_createCCtx()
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_createCCtx_32();
                case Platform.win64:
                    return ZSTD_createCCtx_64();
                case Platform.linux:
                    return ZSTD_createCCtx_so();
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_freeCCtx(IntPtr cctx)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_freeCCtx_32(cctx);
                case Platform.win64:
                    return ZSTD_freeCCtx_64(cctx);
                case Platform.linux:
                    return ZSTD_freeCCtx_so(cctx);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static IntPtr ZSTD_createDCtx()
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_createDCtx_32();
                case Platform.win64:
                    return ZSTD_createDCtx_64();
                case Platform.linux:
                    return ZSTD_createDCtx_so();
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_freeDCtx(IntPtr cctx)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_freeDCtx_32(cctx);
                case Platform.win64:
                    return ZSTD_freeDCtx_64(cctx);
                case Platform.linux:
                    return ZSTD_freeDCtx_so(cctx);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static size_t ZSTD_compressCCtx(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_compressCCtx_32(ctx, dst, dstCapacity, src, srcSize, compressionLevel);
                case Platform.win64:
                    return ZSTD_compressCCtx_64(ctx, dst, dstCapacity, src, srcSize, compressionLevel);
                case Platform.linux:
                    return ZSTD_compressCCtx_so(ctx, dst, dstCapacity, src, srcSize, compressionLevel);
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_decompressDCtx(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_decompressDCtx_32(ctx, dst, dstCapacity, src, srcSize);
                case Platform.win64:
                    return ZSTD_decompressDCtx_64(ctx, dst, dstCapacity, src, srcSize);
                case Platform.linux:
                    return ZSTD_decompressDCtx_so(ctx, dst, dstCapacity, src, srcSize);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static IntPtr ZSTD_createCDict(byte[] dict, size_t dictSize, int compressionLevel)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_createCDict_32(dict, dictSize, compressionLevel);
                case Platform.win64:
                    return ZSTD_createCDict_64(dict, dictSize, compressionLevel);
                case Platform.linux:
                    return ZSTD_createCDict_so(dict, dictSize, compressionLevel);
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_freeCDict(IntPtr cdict)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_freeCDict_32(cdict);
                case Platform.win64:
                    return ZSTD_freeCDict_64(cdict);
                case Platform.linux:
                    return ZSTD_freeCDict_so(cdict);
            }
            
            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_compress_usingCDict(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_compress_usingCDict_32(cctx, dst, dstCapacity, src, srcSize, cdict);
                case Platform.win64:
                    return ZSTD_compress_usingCDict_64(cctx, dst, dstCapacity, src, srcSize, cdict);
                case Platform.linux:
                    return ZSTD_compress_usingCDict_so(cctx, dst, dstCapacity, src, srcSize, cdict);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static IntPtr ZSTD_createDDict(byte[] dict, size_t dictSize)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_createDDict_32(dict, dictSize);
                case Platform.win64:
                    return ZSTD_createDDict_64(dict, dictSize);
                case Platform.linux:
                    return ZSTD_createDDict_so(dict, dictSize);
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_freeDDict(IntPtr ddict)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_freeDDict_32(ddict);
                case Platform.win64:
                    return ZSTD_freeDDict_64(ddict);
                case Platform.linux:
                    return ZSTD_freeDDict_so(ddict);
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_decompress_usingDDict(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_decompress_usingDDict_32(dctx, dst, dstCapacity, src, srcSize, ddict);
                case Platform.win64:
                    return ZSTD_decompress_usingDDict_64(dctx, dst, dstCapacity, src, srcSize, ddict);
                case Platform.linux:
                    return ZSTD_decompress_usingDDict_so(dctx, dst, dstCapacity, src, srcSize, ddict);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static ulong ZSTD_getDecompressedSize(IntPtr src, size_t srcSize)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_getDecompressedSize_32(src, srcSize);
                case Platform.win64:
                    return ZSTD_getDecompressedSize_64(src, srcSize);
                case Platform.linux:
                    return ZSTD_getDecompressedSize_so(src, srcSize);
            }

            throw new PlatformNotSupportedException();
        }
        
        public static int ZSTD_maxCLevel()
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_maxCLevel_32();
                case Platform.win64:
                    return ZSTD_maxCLevel_64();
                case Platform.linux:
                    return ZSTD_maxCLevel_so();
            }

            throw new PlatformNotSupportedException();
        }
        public static size_t ZSTD_compressBound(size_t srcSize)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_compressBound_32(srcSize);
                case Platform.win64:
                    return ZSTD_compressBound_64(srcSize);
                case Platform.linux:
                    return ZSTD_compressBound_so(srcSize);
            }

            throw new PlatformNotSupportedException();
        }
        public static uint ZSTD_isError(size_t code)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_isError_32(code);
                case Platform.win64:
                    return ZSTD_isError_64(code);
                case Platform.linux:
                    return ZSTD_isError_so(code);
            }

            throw new PlatformNotSupportedException();
        }
        public static IntPtr ZSTD_getErrorName(size_t code)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return ZSTD_getErrorName_32(code);
                case Platform.win64:
                    return ZSTD_getErrorName_64(code);
                case Platform.linux:
                    return ZSTD_getErrorName_so(code);
            }

            throw new PlatformNotSupportedException();
        }
        #endregion

        #region x64

        [DllImport(dll64, EntryPoint = "ZDICT_trainFromBuffer", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZDICT_trainFromBuffer_64(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples);
        [DllImport(dll64, EntryPoint = "ZDICT_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZDICT_isError_64(size_t code);
        [DllImport(dll64, EntryPoint = "ZDICT_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZDICT_getErrorName_64(size_t code);

        [DllImport(dll64, EntryPoint = "ZSTD_createCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCCtx_64();
        [DllImport(dll64, EntryPoint = "ZSTD_freeCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCCtx_64(IntPtr cctx);

        [DllImport(dll64, EntryPoint = "ZSTD_createDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDCtx_64();
        [DllImport(dll64, EntryPoint = "ZSTD_freeDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDCtx_64(IntPtr cctx);

        [DllImport(dll64, EntryPoint = "ZSTD_compressCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressCCtx_64(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel);
        [DllImport(dll64, EntryPoint = "ZSTD_decompressDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompressDCtx_64(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize);

        [DllImport(dll64, EntryPoint = "ZSTD_createCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCDict_64(byte[] dict, size_t dictSize, int compressionLevel);
        [DllImport(dll64, EntryPoint = "ZSTD_freeCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCDict_64(IntPtr cdict);
        [DllImport(dll64, EntryPoint = "ZSTD_compress_usingCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compress_usingCDict_64(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict);

        [DllImport(dll64, EntryPoint = "ZSTD_createDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDDict_64(byte[] dict, size_t dictSize);
        [DllImport(dll64, EntryPoint = "ZSTD_freeDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDDict_64(IntPtr ddict);
        [DllImport(dll64, EntryPoint = "ZSTD_decompress_usingDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompress_usingDDict_64(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict);

        [DllImport(dll64, EntryPoint = "ZSTD_getDecompressedSize", CallingConvention = CallingConvention.Cdecl)]
        static extern ulong ZSTD_getDecompressedSize_64(IntPtr src, size_t srcSize);

        [DllImport(dll64, EntryPoint = "ZSTD_maxCLevel", CallingConvention = CallingConvention.Cdecl)]
        static extern int ZSTD_maxCLevel_64();
        [DllImport(dll64, EntryPoint = "ZSTD_compressBound", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressBound_64(size_t srcSize);
        [DllImport(dll64, EntryPoint = "ZSTD_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZSTD_isError_64(size_t code);
        [DllImport(dll64, EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_getErrorName_64(size_t code);

        #endregion

        #region x86

        [DllImport(dll32, EntryPoint = "ZDICT_trainFromBuffer", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZDICT_trainFromBuffer_32(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples);
        [DllImport(dll32, EntryPoint = "ZDICT_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZDICT_isError_32(size_t code);
        [DllImport(dll32, EntryPoint = "ZDICT_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZDICT_getErrorName_32(size_t code);

        [DllImport(dll32, EntryPoint = "ZSTD_createCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCCtx_32();
        [DllImport(dll32, EntryPoint = "ZSTD_freeCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCCtx_32(IntPtr cctx);

        [DllImport(dll32, EntryPoint = "ZSTD_createDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDCtx_32();
        [DllImport(dll32, EntryPoint = "ZSTD_freeDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDCtx_32(IntPtr cctx);

        [DllImport(dll32, EntryPoint = "ZSTD_compressCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressCCtx_32(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel);
        [DllImport(dll32, EntryPoint = "ZSTD_decompressDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompressDCtx_32(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize);

        [DllImport(dll32, EntryPoint = "ZSTD_createCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCDict_32(byte[] dict, size_t dictSize, int compressionLevel);
        [DllImport(dll32, EntryPoint = "ZSTD_freeCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCDict_32(IntPtr cdict);
        [DllImport(dll32, EntryPoint = "ZSTD_compress_usingCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compress_usingCDict_32(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict);

        [DllImport(dll32, EntryPoint = "ZSTD_createDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDDict_32(byte[] dict, size_t dictSize);
        [DllImport(dll32, EntryPoint = "ZSTD_freeDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDDict_32(IntPtr ddict);
        [DllImport(dll32, EntryPoint = "ZSTD_decompress_usingDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompress_usingDDict_32(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict);

        [DllImport(dll32, EntryPoint = "ZSTD_getDecompressedSize", CallingConvention = CallingConvention.Cdecl)]
        static extern ulong ZSTD_getDecompressedSize_32(IntPtr src, size_t srcSize);

        [DllImport(dll32, EntryPoint = "ZSTD_maxCLevel", CallingConvention = CallingConvention.Cdecl)]
        static extern int ZSTD_maxCLevel_32();
        [DllImport(dll32, EntryPoint = "ZSTD_compressBound", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressBound_32(size_t srcSize);
        [DllImport(dll32, EntryPoint = "ZSTD_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZSTD_isError_32(size_t code);
        [DllImport(dll32, EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_getErrorName_32(size_t code);

        #endregion

        #region Linux
        [DllImport(linuxso, EntryPoint = "ZDICT_trainFromBuffer", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZDICT_trainFromBuffer_so(byte[] dictBuffer, size_t dictBufferCapacity, byte[] samplesBuffer, size_t[] samplesSizes, uint nbSamples);
        [DllImport(linuxso, EntryPoint = "ZDICT_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZDICT_isError_so(size_t code);
        [DllImport(linuxso, EntryPoint = "ZDICT_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZDICT_getErrorName_so(size_t code);

        [DllImport(linuxso, EntryPoint = "ZSTD_createCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCCtx_so();
        [DllImport(linuxso, EntryPoint = "ZSTD_freeCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCCtx_so(IntPtr cctx);

        [DllImport(linuxso, EntryPoint = "ZSTD_createDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDCtx_so();
        [DllImport(linuxso, EntryPoint = "ZSTD_freeDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDCtx_so(IntPtr cctx);

        [DllImport(linuxso, EntryPoint = "ZSTD_compressCCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressCCtx_so(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, int compressionLevel);
        [DllImport(linuxso, EntryPoint = "ZSTD_decompressDCtx", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompressDCtx_so(IntPtr ctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize);

        [DllImport(linuxso, EntryPoint = "ZSTD_createCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createCDict_so(byte[] dict, size_t dictSize, int compressionLevel);
        [DllImport(linuxso, EntryPoint = "ZSTD_freeCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeCDict_so(IntPtr cdict);
        [DllImport(linuxso, EntryPoint = "ZSTD_compress_usingCDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compress_usingCDict_so(IntPtr cctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr cdict);

        [DllImport(linuxso, EntryPoint = "ZSTD_createDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_createDDict_so(byte[] dict, size_t dictSize);
        [DllImport(linuxso, EntryPoint = "ZSTD_freeDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_freeDDict_so(IntPtr ddict);
        [DllImport(linuxso, EntryPoint = "ZSTD_decompress_usingDDict", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_decompress_usingDDict_so(IntPtr dctx, IntPtr dst, size_t dstCapacity, IntPtr src, size_t srcSize, IntPtr ddict);

        [DllImport(linuxso, EntryPoint = "ZSTD_getDecompressedSize", CallingConvention = CallingConvention.Cdecl)]
        static extern ulong ZSTD_getDecompressedSize_so(IntPtr src, size_t srcSize);

        [DllImport(linuxso, EntryPoint = "ZSTD_maxCLevel", CallingConvention = CallingConvention.Cdecl)]
        static extern int ZSTD_maxCLevel_so();
        [DllImport(linuxso, EntryPoint = "ZSTD_compressBound", CallingConvention = CallingConvention.Cdecl)]
        static extern size_t ZSTD_compressBound_so(size_t srcSize);
        [DllImport(linuxso, EntryPoint = "ZSTD_isError", CallingConvention = CallingConvention.Cdecl)]
        static extern uint ZSTD_isError_so(size_t code);
        [DllImport(linuxso, EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ZSTD_getErrorName_so(size_t code);
        #endregion
    }
}
