using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PPeX.External.libresample
{
    internal static class LibResampleAPI
    {
        static LibResampleAPI()
        {
	        var libresample = new Dictionary<string, List<DynDllMapping>>
	        {
		        ["libresample"] = new List<DynDllMapping>
		        {
                    "libresample64.dll",
                    "libresample32.dll",
                    "libresample.so.1"
                }
	        };

	        typeof(LibResampleAPI).ResolveDynDllImports(libresample);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr LibResampleCreateDelegate(int highQuality, double minFactor, double maxFactor);
        [DynDllImport("libresample")]
        public static LibResampleCreateDelegate resample_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int LibResampleProcessDelegate(IntPtr handle, double factor, float* inBuffer, int inBufferLen, int lastFlag, out int inBufferUsed, float* outBuffer, int outBufferLen);
        [DynDllImport("libresample")]
        public static LibResampleProcessDelegate resample_process;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LibResampleCloseDelegate(IntPtr handle);
        [DynDllImport("libresample")]
        public static LibResampleCloseDelegate resample_close;
    }
}