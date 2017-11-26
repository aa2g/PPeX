using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.External.libresample
{
    internal static class API
    {
        private const string dll64 = "libresample64.dll";
        private const string dll32 = "libresample32.dll";
        private const string linuxso = "libresample.so";

        private enum Platform
        {
            win32,
            win64,
            linux
        }

        private static readonly Platform currentPlatform;

        static API()
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

        public static IntPtr resample_open(int highQuality, double minFactor, double maxFactor)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return resample_open_32(highQuality, minFactor, maxFactor);
                case Platform.win64:
                    return resample_open_64(highQuality, minFactor, maxFactor);
                case Platform.linux:
                    return resample_open_so(highQuality, minFactor, maxFactor);
            }

            throw new PlatformNotSupportedException();
        }

        public static int resample_process(IntPtr handle, double factor, float[] inBuffer, int inBufferLen, int lastFlag, out int inBufferUsed, float[] outBuffer, int outBufferLen)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return resample_process_32(handle, factor, inBuffer, inBufferLen, lastFlag, out inBufferUsed, outBuffer, outBufferLen);
                case Platform.win64:
                    return resample_process_64(handle, factor, inBuffer, inBufferLen, lastFlag, out inBufferUsed, outBuffer, outBufferLen);
                case Platform.linux:
                    return resample_process_so(handle, factor, inBuffer, inBufferLen, lastFlag, out inBufferUsed, outBuffer, outBufferLen);
            }

            throw new PlatformNotSupportedException();
        }

        public static void resample_close(IntPtr handle)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    resample_close_32(handle);
                    return;
                case Platform.win64:
                    resample_close_64(handle);
                    return;
                case Platform.linux:
                    resample_close_so(handle);
                    return;
            }

            throw new PlatformNotSupportedException();
        }

        #endregion

        #region Win32

        [DllImport(dll32, EntryPoint = "resample_open", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr resample_open_32(int highQuality, double minFactor, double maxFactor);

        [DllImport(dll32, EntryPoint = "resample_process", CallingConvention = CallingConvention.Cdecl)]
        static extern int resample_process_32(IntPtr handle, double factor, float[] inBuffer, int inBufferLen, int lastFlag, out int inBufferUsed, float[] outBuffer, int outBufferLen);

        [DllImport(dll32, EntryPoint = "resample_close", CallingConvention = CallingConvention.Cdecl)]
        static extern void resample_close_32(IntPtr handle);

        #endregion

        #region Win64

        [DllImport(dll64, EntryPoint = "resample_open", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr resample_open_64(int highQuality, double minFactor, double maxFactor);

        [DllImport(dll64, EntryPoint = "resample_process", CallingConvention = CallingConvention.Cdecl)]
        static extern int resample_process_64(IntPtr handle, double factor, float[] inBuffer, int inBufferLen, int lastFlag, out int inBufferUsed, float[] outBuffer, int outBufferLen);

        [DllImport(dll64, EntryPoint = "resample_close", CallingConvention = CallingConvention.Cdecl)]
        static extern void resample_close_64(IntPtr handle);

        #endregion

        #region Linux

        [DllImport(linuxso, EntryPoint = "resample_open", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr resample_open_so(int highQuality, double minFactor, double maxFactor);

        [DllImport(linuxso, EntryPoint = "resample_process", CallingConvention = CallingConvention.Cdecl)]
        static extern int resample_process_so(IntPtr handle, double factor, float[] inBuffer, int inBufferLen, int lastFlag, out int inBufferUsed, float[] outBuffer, int outBufferLen);

        [DllImport(linuxso, EntryPoint = "resample_close", CallingConvention = CallingConvention.Cdecl)]
        static extern void resample_close_so(IntPtr handle);

        #endregion
    }
}
