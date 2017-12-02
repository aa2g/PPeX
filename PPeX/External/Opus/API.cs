using System;
using System.Runtime.InteropServices;

namespace FragLabs.Audio.Codecs.Opus
{
    /// <summary>
    /// Wraps the Opus API.
    /// </summary>
    internal static class API
    {
        const string dll32 = "opus32.dll";
        const string dll64 = "opus64.dll";
        const string linuxso = "libopus.so";

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
        public static IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_encoder_create_32(Fs, channels, application, out error);
                case Platform.win64:
                    return opus_encoder_create_64(Fs, channels, application, out error);
                case Platform.linux:
                    return opus_encoder_create_so(Fs, channels, application, out error);
            }

            throw new PlatformNotSupportedException();
        }

        public static void opus_encoder_destroy(IntPtr encoder)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    opus_encoder_destroy_32(encoder);
                    return;
                case Platform.win64:
                    opus_encoder_destroy_64(encoder);
                    return;
                case Platform.linux:
                    opus_encoder_destroy_so(encoder);
                    return;
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_encode(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_encode_32(st, pcm, frame_size, data, max_data_bytes);
                case Platform.win64:
                    return opus_encode_64(st, pcm, frame_size, data, max_data_bytes);
                case Platform.linux:
                    return opus_encode_so(st, pcm, frame_size, data, max_data_bytes);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_encode_float(IntPtr st, float[] pcm, int frame_size, IntPtr data, int max_data_bytes)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_encode_float_32(st, pcm, frame_size, data, max_data_bytes);
                case Platform.win64:
                    return opus_encode_float_64(st, pcm, frame_size, data, max_data_bytes);
                case Platform.linux:
                    return opus_encode_float_so(st, pcm, frame_size, data, max_data_bytes);
            }

            throw new PlatformNotSupportedException();
        }

        public static IntPtr opus_decoder_create(int Fs, int channels, out IntPtr error)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_decoder_create_32(Fs, channels, out error);
                case Platform.win64:
                    return opus_decoder_create_64(Fs, channels, out error);
                case Platform.linux:
                    return opus_decoder_create_so(Fs, channels, out error);
            }

            throw new PlatformNotSupportedException();
        }

        public static void opus_decoder_destroy(IntPtr decoder)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    opus_decoder_destroy_32(decoder);
                    return;
                case Platform.win64:
                    opus_decoder_destroy_64(decoder);
                    return;
                case Platform.linux:
                    opus_decoder_destroy_so(decoder);
                    return;
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_decode(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_decode_32(st, data, len, pcm, frame_size, decode_fec);
                case Platform.win64:
                    return opus_decode_64(st, data, len, pcm, frame_size, decode_fec);
                case Platform.linux:
                    return opus_decode_so(st, data, len, pcm, frame_size, decode_fec);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_decode_float(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_decode_float_32(st, data, len, pcm, frame_size, decode_fec);
                case Platform.win64:
                    return opus_decode_float_64(st, data, len, pcm, frame_size, decode_fec);
                case Platform.linux:
                    return opus_decode_float_so(st, data, len, pcm, frame_size, decode_fec);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_encoder_ctl(IntPtr st, Ctl request, int value)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_encoder_ctl_32(st, request, value);
                case Platform.win64:
                    return opus_encoder_ctl_64(st, request, value);
                case Platform.linux:
                    return opus_encoder_ctl_so(st, request, value);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_encoder_ctl(IntPtr st, Ctl request, out int value)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_encoder_ctl_32(st, request, out value);
                case Platform.win64:
                    return opus_encoder_ctl_64(st, request, out value);
                case Platform.linux:
                    return opus_encoder_ctl_so(st, request, out value);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_packet_get_nb_channels(byte[] data)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_packet_get_nb_channels_32(data);
                case Platform.win64:
                    return opus_packet_get_nb_channels_64(data);
                case Platform.linux:
                    return opus_packet_get_nb_channels_so(data);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_packet_get_nb_frames(byte[] data, int len)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_packet_get_nb_frames_32(data, len);
                case Platform.win64:
                    return opus_packet_get_nb_frames_64(data, len);
                case Platform.linux:
                    return opus_packet_get_nb_frames_so(data, len);
            }

            throw new PlatformNotSupportedException();
        }

        public static int opus_packet_get_nb_samples(byte[] data, int len, int freq)
        {
            switch (currentPlatform)
            {
                case Platform.win32:
                    return opus_packet_get_nb_samples_32(data, len, freq);
                case Platform.win64:
                    return opus_packet_get_nb_samples_64(data, len, freq);
                case Platform.linux:
                    return opus_packet_get_nb_samples_so(data, len, freq);
            }

            throw new PlatformNotSupportedException();
        }
        #endregion

        #region Win32
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

        [DllImport(dll32, EntryPoint = "opus_decode_float", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_decode_float_32(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

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

        #region Win64
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

        [DllImport(dll64, EntryPoint = "opus_decode_float", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_decode_float_64(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

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

        #region Linux
        [DllImport(linuxso, EntryPoint = "opus_encoder_create", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr opus_encoder_create_so(int Fs, int channels, int application, out IntPtr error);

        [DllImport(linuxso, EntryPoint = "opus_encoder_destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void opus_encoder_destroy_so(IntPtr encoder);

        [DllImport(linuxso, EntryPoint = "opus_encode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encode_so(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport(linuxso, EntryPoint = "opus_encode_float", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encode_float_so(IntPtr st, float[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport(linuxso, EntryPoint = "opus_decoder_create", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr opus_decoder_create_so(int Fs, int channels, out IntPtr error);

        [DllImport(linuxso, EntryPoint = "opus_decoder_destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void opus_decoder_destroy_so(IntPtr decoder);

        [DllImport(linuxso, EntryPoint = "opus_decode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_decode_so(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport(linuxso, EntryPoint = "opus_decode_float", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_decode_float_so(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport(linuxso, EntryPoint = "opus_encoder_ctl", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encoder_ctl_so(IntPtr st, Ctl request, int value);

        [DllImport(linuxso, EntryPoint = "opus_encoder_ctl", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_encoder_ctl_so(IntPtr st, Ctl request, out int value);

        [DllImport(linuxso, EntryPoint = "opus_packet_get_nb_channels", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_channels_so(byte[] data);

        [DllImport(linuxso, EntryPoint = "opus_packet_get_nb_frames", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_frames_so(byte[] data, int len);

        [DllImport(linuxso, EntryPoint = "opus_packet_get_nb_samples", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int opus_packet_get_nb_samples_so(byte[] data, int len, int freq);
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
