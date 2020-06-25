using System;

namespace PPeX.External.Opus
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
            if (inputSamplingRate != 8000
                && inputSamplingRate != 12000
                && inputSamplingRate != 16000
                && inputSamplingRate != 24000
                && inputSamplingRate != 48000)
                throw new ArgumentOutOfRangeException(nameof(inputSamplingRate));

            if (inputChannels != 1 && inputChannels != 2)
                throw new ArgumentOutOfRangeException(nameof(inputChannels));

            IntPtr encoder = OpusAPI.opus_encoder_create(inputSamplingRate, inputChannels, (int)application, out var error);

            if ((Errors)error != Errors.OK)
	            throw new Exception("Exception occured while creating encoder");

            return new OpusEncoder(encoder, inputSamplingRate, inputChannels, application);
        }

        private IntPtr EncoderInstance { get; set; }

        private OpusEncoder(IntPtr encoderInstance, int inputSamplingRate, int inputChannels, Application application)
        {
            EncoderInstance = encoderInstance;
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
        public unsafe byte[] Encode(Span<byte> inputPcmSamples, int sampleLength, out int encodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusEncoder");

            int maxDataBytes = sampleLength * InputChannels * sizeof(short);

            byte[] encoded = new byte[maxDataBytes];
            int length = 0;

            fixed (byte* inputPtr = inputPcmSamples)
            fixed (byte* outputPtr = encoded)
            {
	            length = OpusAPI.opus_encode(EncoderInstance, inputPtr, sampleLength, outputPtr, maxDataBytes);
            }

            encodedLength = length;
            if (length < 0)
            {
                var exception = new Exception("Encoding failed - " + (Errors)length);
                exception.Data.Add("inputPcmSamples.Length", inputPcmSamples.Length.ToString());
                exception.Data.Add("sampleLength", sampleLength.ToString());
                exception.Data.Add("MaxDataBytes", maxDataBytes.ToString());
                throw exception;
            }

            return encoded;
        }

        /// <summary>
        /// Produces Opus encoded audio from PCM samples.
        /// </summary>
        /// <param name="inputPcmSamples">PCM samples to encode.</param>
        /// <param name="sampleLength">How many samples to encode.</param>
        /// <param name="encodedLength">Set to length of encoded audio.</param>
        /// <returns>Opus encoded audio buffer.</returns>
        public unsafe void Encode(ReadOnlySpan<float> inputPcmSamples, Span<byte> outputSpan, int sampleLength, out int encodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusEncoder");
            
            int length = 0;

            fixed (float* inputPtr = inputPcmSamples)
            fixed (byte* outputPtr = outputSpan)
            {
	            length = OpusAPI.opus_encode_float(EncoderInstance, inputPtr, sampleLength, outputPtr, outputSpan.Length);
            }

            encodedLength = length;
            if (length < 0)
            {
                var exception = new Exception("Encoding failed - " + (Errors)length);
                exception.Data.Add("inputPcmSamples.Length", inputPcmSamples.Length);
                exception.Data.Add("sampleLength", sampleLength);
                exception.Data.Add("InputChannels", InputChannels);

                throw exception;
            }
        }

        /// <summary>
        /// Determines the number of frames in the PCM samples.
        /// </summary>
        /// <param name="pcmSamples"></param>
        /// <returns></returns>
        public int FrameCount(byte[] pcmSamples)
        {
            // seems like bitrate should be required
            const int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * InputChannels;
            return pcmSamples.Length / bytesPerSample;
        }

        /// <summary>
        /// Helper method to determine the buffer size upper bound required for encoding to work correctly.
        /// </summary>
        /// <param name="frameCount">Target frame size.</param>
        /// <returns></returns>
        public int FrameUpperBound(int frameSize)
        {
            return 2 * InputChannels * frameSize;
        }

        /// <summary>
        /// Gets the input sampling rate of the encoder.
        /// </summary>
        public int InputSamplingRate { get; private set; }

        private int GetCtl(Ctl ctl)
		{
			if (disposed)
				throw new ObjectDisposedException("OpusEncoder");

			var ret = OpusAPI.opus_encoder_ctl_out(EncoderInstance, ctl, out int result);

			if (ret < 0)
				throw new Exception("Encoder error - " + (Errors)ret);

			return result;
		}

        private void SetCtl(Ctl ctl, int value)
        {
	        if (disposed)
		        throw new ObjectDisposedException("OpusEncoder");

	        var ret = OpusAPI.opus_encoder_ctl(EncoderInstance, ctl, value);

	        if (ret < 0)
		        throw new Exception("Encoder error - " + (Errors)ret);
        }

        /// <summary>
        /// Gets the number of channels of the encoder.
        /// </summary>
        public int InputChannels
        {
	        get => GetCtl(Ctl.OPUS_GET_FORCE_CHANNELS_REQUEST);
            set => SetCtl(Ctl.OPUS_SET_FORCE_CHANNELS_REQUEST, value);
        }

        /// <summary>
        /// Gets the number of samples that need to be intially skipped.
        /// </summary>
        public int LookaheadSamples => GetCtl(Ctl.OPUS_GET_LOOKAHEAD_REQUEST);

        /// <summary>
        /// Gets the coding mode of the encoder.
        /// </summary>
        public Application Application
        {
	        get => (Application)GetCtl(Ctl.OPUS_GET_APPLICATION_REQUEST);
	        set => SetCtl(Ctl.OPUS_SET_APPLICATION_REQUEST, (int)value);
        }

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
	        get => GetCtl(Ctl.OPUS_GET_BITRATE_REQUEST);
	        set => SetCtl(Ctl.OPUS_SET_BITRATE_REQUEST, value);
        }

        /// <summary>
        /// Gets or sets whether Forward Error Correction is enabled.
        /// </summary>
        public bool ForwardErrorCorrection
        {
	        get => GetCtl(Ctl.OPUS_GET_INBAND_FEC_REQUEST) != 0;
	        set => SetCtl(Ctl.OPUS_SET_INBAND_FEC_REQUEST, value ? 1 : 0);
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

            if (EncoderInstance != IntPtr.Zero)
            {
                OpusAPI.opus_encoder_destroy(EncoderInstance);
                EncoderInstance = IntPtr.Zero;
            }

            disposed = true;
        }
    }
}