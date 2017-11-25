﻿using System;
using System.Collections.Generic;
using FragLabs.Audio.Codecs.Opus;

namespace FragLabs.Audio.Codecs
{
    /// <summary>
    /// Opus codec wrapper.
    /// </summary>
    public class OpusEncoder : IDisposable
    {
        /// <summary>
        /// Creates a new Opus encoder.
        /// </summary>
        /// <param name="inputSamplingRate">Sampling rate of the input signal (Hz). This must be one of 8000, 12000, 16000, 24000, or 48000.</param>
        /// <param name="inputChannels">Number of channels (1 or 2) in input signal.</param>
        /// <param name="application">Coding mode.</param>
        /// <returns>A new <c>OpusEncoder</c></returns>
        public static OpusEncoder Create(int inputSamplingRate, int inputChannels, Application application)
        {
            if (inputSamplingRate != 8000 &&
                inputSamplingRate != 12000 &&
                inputSamplingRate != 16000 &&
                inputSamplingRate != 24000 &&
                inputSamplingRate != 48000)
                throw new ArgumentOutOfRangeException("inputSamplingRate");
            if (inputChannels != 1 && inputChannels != 2)
                throw new ArgumentOutOfRangeException("inputChannels");

            IntPtr error;
            IntPtr encoder = API.opus_encoder_create(inputSamplingRate, inputChannels, (int)application, out error);
            if ((Errors)error != Errors.OK)
            {
                throw new Exception("Exception occured while creating encoder");
            }
            return new OpusEncoder(encoder, inputSamplingRate, inputChannels, application);
        }

        private IntPtr _encoder;

        private OpusEncoder(IntPtr encoder, int inputSamplingRate, int inputChannels, Application application)
        {
            _encoder = encoder;
            InputSamplingRate = inputSamplingRate;
            InputChannels = inputChannels;
            Application = application;
        }

        /// <summary>
        /// Produces Opus encoded audio from PCM samples.
        /// </summary>
        /// <param name="inputPcmSamples">PCM samples to encode.</param>
        /// <param name="sampleLength">How many bytes to encode.</param>
        /// <param name="encodedLength">Set to length of encoded audio.</param>
        /// <returns>Opus encoded audio buffer.</returns>
        public unsafe byte[] Encode(byte[] inputPcmSamples, int sampleLength, out int encodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusEncoder");

            int MaxDataBytes = sampleLength * InputChannels * sizeof(short);

            int frames = FrameCount(inputPcmSamples);
            IntPtr encodedPtr;
            byte[] encoded = new byte[MaxDataBytes];
            int length = 0;
            fixed (byte* benc = encoded)
            {
                encodedPtr = new IntPtr((void*)benc);
                length = API.opus_encode(_encoder, inputPcmSamples, sampleLength, encodedPtr, MaxDataBytes);
            }
            encodedLength = length;
            if (length < 0)
            {
                var exception = new Exception("Encoding failed - " + ((Errors)length).ToString());
                exception.Data.Add("inputPcmSamples.Length", inputPcmSamples.Length.ToString());
                exception.Data.Add("sampleLength", sampleLength.ToString());
                exception.Data.Add("MaxDataBytes", MaxDataBytes.ToString());
                exception.Data.Add("encodedPtr", BitConverter.ToString(BitConverter.GetBytes(encodedPtr.ToInt64())));
                throw exception;
            }

            return encoded;
        }

        /// <summary>
        /// Produces Opus encoded audio from PCM samples.
        /// </summary>
        /// <param name="inputPcmSamples">PCM samples to encode.</param>
        /// <param name="encodedLength">Set to length of encoded audio.</param>
        /// <returns>Opus encoded audio buffer.</returns>
        public unsafe byte[] Encode(float[] inputPcmSamples, int sampleLength, out int encodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusEncoder");
            
            int MaxDataBytes = sampleLength * InputChannels * sizeof(float);

            IntPtr encodedPtr;
            byte[] encoded = new byte[MaxDataBytes];
            int length = 0;
            fixed (byte* benc = encoded)
            {
                encodedPtr = new IntPtr((void*)benc);
                length = API.opus_encode_float(_encoder, inputPcmSamples, sampleLength, encodedPtr, MaxDataBytes);
            }
            encodedLength = length;
            if (length < 0)
            {
                var exception = new Exception("Encoding failed - " + ((Errors)length));
                exception.Data.Add("inputPcmSamples.Length", inputPcmSamples.Length);
                exception.Data.Add("sampleLength", sampleLength);
                exception.Data.Add("MaxDataBytes", MaxDataBytes);
                exception.Data.Add("InputChannels", InputChannels);
                exception.Data.Add("encodedPtr", BitConverter.ToString(BitConverter.GetBytes(encodedPtr.ToInt64())));

                throw exception;
            }

            return encoded;
        }

        /// <summary>
        /// Determines the number of frames in the PCM samples.
        /// </summary>
        /// <param name="pcmSamples"></param>
        /// <returns></returns>
        public int FrameCount(byte[] pcmSamples)
        {
            //  seems like bitrate should be required
            int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * InputChannels;
            return pcmSamples.Length / bytesPerSample;
        }

        /// <summary>
        /// Helper method to determine how many bytes are required for encoding to work.
        /// </summary>
        /// <param name="frameCount">Target frame size.</param>
        /// <returns></returns>
        public int FrameByteCount(int frameCount)
        {
            int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * InputChannels;
            return frameCount * bytesPerSample;
        }

        /// <summary>
        /// Gets the input sampling rate of the encoder.
        /// </summary>
        public int InputSamplingRate { get; private set; }

        /// <summary>
        /// Gets the number of channels of the encoder.
        /// </summary>
        public int InputChannels
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException("OpusEncoder");
                int bitrate;
                var ret = API.opus_encoder_ctl(_encoder, Ctl.OPUS_GET_FORCE_CHANNELS_REQUEST, out bitrate);
                if (ret < 0)
                    throw new Exception("Encoder error - " + ((Errors)ret).ToString());
                return bitrate;
            }
            set
            {
                if (disposed)
                    throw new ObjectDisposedException("OpusEncoder");
                var ret = API.opus_encoder_ctl(_encoder, Ctl.OPUS_SET_FORCE_CHANNELS_REQUEST, value);
                if (ret < 0)
                    throw new Exception("Encoder error - " + ((Errors)ret).ToString());
            }
        }

/// <summary>
/// Gets the coding mode of the encoder.
/// </summary>
public Application Application { get; private set; }

        /// <summary>
        /// Gets or sets the size of memory allocated for reading encoded data.
        /// 4000 is recommended.
        /// </summary>
        public int MaxDataBytes { get; set; }

        /// <summary>
        /// Gets or sets the bitrate setting of the encoding.
        /// </summary>
        public int Bitrate
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException("OpusEncoder");
                int bitrate;
                var ret = API.opus_encoder_ctl(_encoder, Ctl.OPUS_GET_BITRATE_REQUEST, out bitrate);
                if (ret < 0)
                    throw new Exception("Encoder error - " + ((Errors)ret).ToString());
                return bitrate;
            }
            set
            {
                if (disposed)
                    throw new ObjectDisposedException("OpusEncoder");
                var ret = API.opus_encoder_ctl(_encoder, Ctl.OPUS_SET_BITRATE_REQUEST, value);
                if (ret < 0)
                    throw new Exception("Encoder error - " + ((Errors)ret).ToString());
            }
        }

        /// <summary>
        /// Gets or sets whether Forward Error Correction is enabled.
        /// </summary>
        public bool ForwardErrorCorrection
        {
            get
            {
                if (_encoder == IntPtr.Zero)
                    throw new ObjectDisposedException("OpusEncoder");

                int fec;
                int ret = API.opus_encoder_ctl(_encoder, Ctl.OPUS_GET_INBAND_FEC_REQUEST, out fec);
                if (ret < 0)
                    throw new Exception("Encoder error - " + ((Errors) ret).ToString());

                return fec > 0;
            }

            set
            {
                if (_encoder == IntPtr.Zero)
                    throw new ObjectDisposedException("OpusEncoder");

                var ret = API.opus_encoder_ctl(_encoder, Ctl.OPUS_SET_INBAND_FEC_REQUEST, value ? 1 : 0);
                if (ret < 0)
                    throw new Exception("Encoder error - " + ((Errors) ret).ToString());
            }
        }

        ~OpusEncoder()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            if (_encoder != IntPtr.Zero)
            {
                API.opus_encoder_destroy(_encoder);
                _encoder = IntPtr.Zero;
            }

            disposed = true;
        }
    }
}
