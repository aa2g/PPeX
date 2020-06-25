using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PPeX.External.Opus
{
    internal static unsafe class OpusAPI
    {
        static OpusAPI()
        {
	        var libopus = new Dictionary<string, List<DynDllMapping>>
	        {
		        ["libopus"] = new List<DynDllMapping>
		        {
                    "opus32.dll",
			        "opus64.dll",
			        "libopus.so.0"
		        }
	        };

	        typeof(OpusAPI).ResolveDynDllImports(libopus);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr OpusEncoderCreateDelegate(int Fs, int channels, int application, out IntPtr error);
        [DynDllImport("libopus")]
        public static OpusEncoderCreateDelegate opus_encoder_create;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OpusEncoderDestroyDelegate(IntPtr encoder);
        [DynDllImport("libopus")]
        public static OpusEncoderDestroyDelegate opus_encoder_destroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusEncodeDelegate(IntPtr st, byte* pcm, int frame_size, byte* data, int max_data_bytes);
        [DynDllImport("libopus")]
        public static OpusEncodeDelegate opus_encode;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusEncodeFloatDelegate(IntPtr st, float* pcm, int frame_size, byte* data, int max_data_bytes);
        [DynDllImport("libopus")]
        public static OpusEncodeFloatDelegate opus_encode_float;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr OpusDecoderCreateDelegate(int Fs, int channels, out IntPtr error);
        [DynDllImport("libopus")]
        public static OpusDecoderCreateDelegate opus_decoder_create;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OpusDecoderDestroyDelegate(IntPtr decoder);
        [DynDllImport("libopus")]
        public static OpusDecoderDestroyDelegate opus_decoder_destroy;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusDecodeDelegate(IntPtr st, byte* data, int len, byte* pcm, int frame_size, int decode_fec);
        [DynDllImport("libopus")]
        public static OpusDecodeDelegate opus_decode;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusDecodeFloatDelegate(IntPtr st, byte* data, int len, float* pcm, int frame_size, int decode_fec);
        [DynDllImport("libopus")]
        public static OpusDecodeFloatDelegate opus_decode_float;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusEncoderCtlDelegate(IntPtr st, Ctl request, int value);
        [DynDllImport("libopus", "opus_encoder_ctl")]
        public static OpusEncoderCtlDelegate opus_encoder_ctl;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusEncoderCtlOutDelegate(IntPtr st, Ctl request, out int value);
        [DynDllImport("libopus", "opus_encoder_ctl")]
        public static OpusEncoderCtlOutDelegate opus_encoder_ctl_out;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusPacketGetNbChannelsDelegate(byte* data);
        [DynDllImport("libopus")]
        public static OpusPacketGetNbChannelsDelegate opus_packet_get_nb_channels;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusPacketGetNbFramesDelegate(byte* data, int len);
        [DynDllImport("libopus")]
        public static OpusPacketGetNbFramesDelegate opus_packet_get_nb_frames;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OpusPacketGetNbSamplesDelegate(byte* data, int len, int freq);
        [DynDllImport("libopus")]
        public static OpusPacketGetNbSamplesDelegate opus_packet_get_nb_samples;
    }

    public enum Ctl : int
    {
        OPUS_SET_APPLICATION_REQUEST = 4000,
        OPUS_GET_APPLICATION_REQUEST = 4001,
        OPUS_SET_BITRATE_REQUEST = 4002,
        OPUS_GET_BITRATE_REQUEST = 4003,
        OPUS_SET_INBAND_FEC_REQUEST = 4012,
        OPUS_GET_INBAND_FEC_REQUEST = 4013,
        OPUS_SET_VBR_CONSTRAINT_REQUEST = 4020,
        OPUS_GET_VBR_CONSTRAINT_REQUEST = 4021,
        OPUS_SET_FORCE_CHANNELS_REQUEST = 4022,
        OPUS_GET_FORCE_CHANNELS_REQUEST = 4023,
        OPUS_SET_COMPLEXITY_REQUEST = 4010,
        OPUS_GET_COMPLEXITY_REQUEST = 4011,
        OPUS_GET_LOOKAHEAD_REQUEST = 4027
    }

    /// <summary>
    /// Supported coding modes.
    /// </summary>
    public enum Application
    {
        /// <summary>
        /// Best for most VoIP/videoconference applications where listening quality and intelligibility matter most.
        /// </summary>
        Voip = 2048,
        /// <summary>
        /// Best for broadcast/high-fidelity application where the decoded audio should be as close as possible to input.
        /// </summary>
        Audio = 2049,
        /// <summary>
        /// Only use when lowest-achievable latency is what matters most. Voice-optimized modes cannot be used.
        /// </summary>
        Restricted_LowLatency = 2051
    }

    public enum Errors
    {
        /// <summary>
        /// No error.
        /// </summary>
        OK              = 0,
        /// <summary>
        /// One or more invalid/out of range arguments.
        /// </summary>
        BadArg          = -1,
        /// <summary>
        /// The mode struct passed is invalid.
        /// </summary>
        BufferTooSmall   = -2,
        /// <summary>
        /// An internal error was detected.
        /// </summary>
        InternalError   = -3,
        /// <summary>
        /// The compressed data passed is corrupted.
        /// </summary>
        InvalidPacket   = -4,
        /// <summary>
        /// Invalid/unsupported request number.
        /// </summary>
        Unimplemented   = -5,
        /// <summary>
        /// An encoder or decoder structure is invalid or already freed.
        /// </summary>
        InvalidState    = -6,
        /// <summary>
        /// Memory allocation has failed.
        /// </summary>
        AllocFail       = -7
    }
}
