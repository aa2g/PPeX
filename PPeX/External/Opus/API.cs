using System;
using System.Runtime.InteropServices;

namespace FragLabs.Audio.Codecs.Opus
{
    /// <summary>
    /// Wraps the Opus API.
    /// </summary>
    internal static class API
    {
        static readonly bool Is64Bit;

        const string dll32 = "opus32.dll";
        const string dll64 = "opus64.dll";

        static API()
        {
            Is64Bit = Environment.Is64BitProcess;
        }

        #region Mixed

        public static IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error)
        {
            if (Is64Bit)
                return opus_encoder_create_64(Fs, channels, application, out error);
            else
                return opus_encoder_create_32(Fs, channels, application, out error);
        }

        public static void opus_encoder_destroy(IntPtr encoder)
        {
            if (Is64Bit)
                opus_encoder_destroy_64(encoder);
            else
                opus_encoder_destroy_32(encoder);
        }

        public static int opus_encode(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes)
        {
            if (Is64Bit)
                return opus_encode_64(st, pcm, frame_size, data, max_data_bytes);
            else
                return opus_encode_32(st, pcm, frame_size, data, max_data_bytes);
        }

        public static int opus_encode_float(IntPtr st, float[] pcm, int frame_size, IntPtr data, int max_data_bytes)
        {
            if (Is64Bit)
                return opus_encode_float_64(st, pcm, frame_size, data, max_data_bytes);
            else
                return opus_encode_float_32(st, pcm, frame_size, data, max_data_bytes);
        }

        public static IntPtr opus_decoder_create(int Fs, int channels, out IntPtr error)
        {
            if (Is64Bit)
                return opus_decoder_create_64(Fs, channels, out error);
            else
                return opus_decoder_create_32(Fs, channels, out error);
        }

        public static void opus_decoder_destroy(IntPtr decoder)
        {
            if (Is64Bit)
                opus_decoder_destroy_64(decoder);
            else
                opus_decoder_destroy_32(decoder);
        }

        public static int opus_decode(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec)
        {
            if (Is64Bit)
                return opus_decode_64(st, data, len, pcm, frame_size, decode_fec);
            else
                return opus_decode_32(st, data, len, pcm, frame_size, decode_fec);
        }

        public static int opus_encoder_ctl(IntPtr st, Ctl request, int value)
        {
            if (Is64Bit)
                return opus_encoder_ctl_64(st, request, value);
            else
                return opus_encoder_ctl_32(st, request, value);
        }

        public static int opus_encoder_ctl(IntPtr st, Ctl request, out int value)
        {
            if (Is64Bit)
                return opus_encoder_ctl_64(st, request, out value);
            else
                return opus_encoder_ctl_32(st, request, out value);
        }

        public static int opus_packet_get_nb_channels(byte[] data)
        {
            if (Is64Bit)
                return opus_packet_get_nb_channels_64(data);
            else
                return opus_packet_get_nb_channels_32(data);

        }

        public static int opus_packet_get_nb_frames(byte[] data, int len)
        {
            if (Is64Bit)
                return opus_packet_get_nb_frames_64(data, len);
            else
                return opus_packet_get_nb_frames_32(data, len);
        }

        public static int opus_packet_get_nb_samples(byte[] data, int len, int freq)
        {
            if (Is64Bit)
                return opus_packet_get_nb_samples_64(data, len, freq);
            else
                return opus_packet_get_nb_samples_32(data, len, freq);
        }

        #endregion

        #region x86
        [DllImport(dll32, EntryPoint = "opus_encoder_create", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr opus_encoder_create_32(int Fs, int channels, int application, out IntPtr error);

        [DllImport(dll32, EntryPoint = "opus_encoder_destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void opus_encoder_destroy_32(IntPtr encoder);

        [DllImport(dll32, EntryPoint = "opus_encode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encode_32(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);
        
        [DllImport(dll32, EntryPoint = "opus_encode_float", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encode_float_32(IntPtr st, float[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport(dll32, EntryPoint = "opus_decoder_create", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr opus_decoder_create_32(int Fs, int channels, out IntPtr error);

        [DllImport(dll32, EntryPoint = "opus_decoder_destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void opus_decoder_destroy_32(IntPtr decoder);

        [DllImport(dll32, EntryPoint = "opus_decode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_decode_32(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport(dll32, EntryPoint = "opus_encoder_ctl", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encoder_ctl_32(IntPtr st, Ctl request, int value);

        [DllImport(dll32, EntryPoint = "opus_encoder_ctl", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encoder_ctl_32(IntPtr st, Ctl request, out int value);

        [DllImport(dll32, EntryPoint = "opus_packet_get_nb_channels", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_channels_32(byte[] data);

        [DllImport(dll32, EntryPoint = "opus_packet_get_nb_frames", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_frames_32(byte[] data, int len);

        [DllImport(dll32, EntryPoint = "opus_packet_get_nb_samples", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_samples_32(byte[] data, int len, int freq);
        #endregion

        #region x64
        [DllImport(dll64, EntryPoint = "opus_encoder_create", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr opus_encoder_create_64(int Fs, int channels, int application, out IntPtr error);

        [DllImport(dll64, EntryPoint = "opus_encoder_destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void opus_encoder_destroy_64(IntPtr encoder);

        [DllImport(dll64, EntryPoint = "opus_encode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encode_64(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport(dll64, EntryPoint = "opus_encode_float", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encode_float_64(IntPtr st, float[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport(dll64, EntryPoint = "opus_decoder_create", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr opus_decoder_create_64(int Fs, int channels, out IntPtr error);

        [DllImport(dll64, EntryPoint = "opus_decoder_destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void opus_decoder_destroy_64(IntPtr decoder);

        [DllImport(dll64, EntryPoint = "opus_decode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_decode_64(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport(dll64, EntryPoint = "opus_encoder_ctl", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encoder_ctl_64(IntPtr st, Ctl request, int value);

        [DllImport(dll64, EntryPoint = "opus_encoder_ctl", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encoder_ctl_64(IntPtr st, Ctl request, out int value);

        [DllImport(dll64, EntryPoint = "opus_packet_get_nb_channels", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_channels_64(byte[] data);

        [DllImport(dll64, EntryPoint = "opus_packet_get_nb_frames", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_frames_64(byte[] data, int len);

        [DllImport(dll64, EntryPoint = "opus_packet_get_nb_samples", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_samples_64(byte[] data, int len, int freq);
        #endregion
    }

    public enum Ctl : int
    {
        OPUS_SET_BITRATE_REQUEST = 4002,
        OPUS_GET_BITRATE_REQUEST = 4003,
        OPUS_SET_INBAND_FEC_REQUEST = 4012,
        OPUS_GET_INBAND_FEC_REQUEST = 4013,
        OPUS_SET_VBR_CONSTRAINT_REQUEST = 4020,
        OPUS_GET_VBR_CONSTRAINT_REQUEST = 4021,
        OPUS_SET_FORCE_CHANNELS_REQUEST = 4022,
        OPUS_GET_FORCE_CHANNELS_REQUEST = 4023,
        OPUS_SET_COMPLEXITY_REQUEST = 4010,
        OPUS_GET_COMPLEXITY_REQUEST = 4011
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
