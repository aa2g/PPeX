using System;

namespace PPeX.External.Opus
{
    /// <summary>
    /// Opus audio decoder.
    /// </summary>
    public class OpusDecoder : IDisposable
    {
        /// <summary>
        /// Creates a new Opus decoder.
        /// </summary>
        /// <param name="outputSampleRate">Sample rate to decode at (Hz). This must be one of 8000, 12000, 16000, 24000, or 48000.</param>
        /// <param name="outputChannels">Number of channels to decode.</param>
        /// <returns>A new <c>OpusDecoder</c>.</returns>
        public static OpusDecoder Create(int outputSampleRate, int outputChannels)
        {
            if (outputSampleRate != 8000
                && outputSampleRate != 12000
                && outputSampleRate != 16000
                && outputSampleRate != 24000
                && outputSampleRate != 48000)
                throw new ArgumentOutOfRangeException(nameof(outputSampleRate));

            if (outputChannels != 1 && outputChannels != 2)
                throw new ArgumentOutOfRangeException(nameof(outputChannels));

            IntPtr decoder = OpusAPI.opus_decoder_create(outputSampleRate, outputChannels, out var error);

            if ((Errors)error != Errors.OK)
	            throw new Exception("Exception occured while creating decoder");

            return new OpusDecoder(decoder, outputSampleRate, outputChannels);
        }

        private OpusDecoder(IntPtr decoderInstance, int outputSamplingRate, int outputChannels)
        {
            DecoderInstance = decoderInstance;
            OutputSamplingRate = outputSamplingRate;
            OutputChannels = outputChannels;
        }

        private IntPtr DecoderInstance { get; set; }

        /// <summary>
        /// Gets the output sampling rate of the decoder.
        /// </summary>
        public int OutputSamplingRate { get; private set; }

        /// <summary>
        /// Gets the number of channels of the decoder.
        /// </summary>
        public int OutputChannels { get; private set; }

        /// <summary>
        /// The base size of memory allocated for decoding data.
        /// </summary>
        public const int MaxDataBytes = 5760;

        /// <summary>
        /// Gets or sets whether forward error correction is enabled or not.
        /// </summary>
        public bool ForwardErrorCorrection { get; set; }

        /// <summary>
        /// Produces PCM samples from Opus encoded data.
        /// </summary>
        /// <param name="inputOpusData">Opus encoded data to decode, null for dropped packet.</param>
        /// <param name="dataLength">Length of data to decode.</param>
        /// <param name="decodedLength">Set to the length of the decoded sample data.</param>
        /// <returns>PCM audio samples.</returns>
        public unsafe byte[] Decode(Span<byte> inputOpusData, int dataLength, out int decodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusDecoder");

            int frameCount = MaxDataBytes * OutputChannels;
            byte[] decoded = new byte[frameCount * 2];
            int length = 0;

            fixed (byte* inputPtr = inputOpusData)
            fixed (byte* outputPtr = decoded)
            {
	            if (inputOpusData != null)
	                length = OpusAPI.opus_decode(DecoderInstance, inputPtr, dataLength, outputPtr, frameCount, 0);
	            else
	                length = OpusAPI.opus_decode(DecoderInstance, null, 0, outputPtr, frameCount, (ForwardErrorCorrection) ? 1 : 0);

	            decodedLength = length * 2 * OutputChannels;
	            if (length < 0)
	                throw new Exception("Decoding failed - " + ((Errors)length));

	            return decoded;
            }
        }

        /// <summary>
        /// Produces PCM samples from Opus encoded data.
        /// </summary>
        /// <param name="inputOpusData">Opus encoded data to decode, null for dropped packet.</param>
        /// <param name="dataLength">Length of data to decode.</param>
        /// <param name="decodedLength">Set to the length of the decoded sample data.</param>
        /// <returns>PCM audio samples.</returns>
        public unsafe float[] DecodeFloat(ReadOnlySpan<byte> inputOpusData)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusDecoder");

            int frameCount = GetSamples(inputOpusData) * OutputChannels;
            float[] decoded = new float[frameCount];
            int length = 0;

            fixed (byte* inputPtr = inputOpusData)
            fixed (float* outputPtr = decoded)
            {
                if (inputOpusData != null)
	                length = OpusAPI.opus_decode_float(DecoderInstance, inputPtr, inputOpusData.Length, outputPtr, frameCount, 0);
	            else
	                length = OpusAPI.opus_decode_float(DecoderInstance, null, 0, outputPtr, frameCount, (ForwardErrorCorrection) ? 1 : 0);
            }

            if (length < 0)
                throw new Exception("Decoding failed - " + (Errors)length);

            //decodedCount = length * OutputChannels;
            Array.Resize(ref decoded, length * OutputChannels);

            return decoded;
        }

        /// <summary>
        /// Produces PCM samples from Opus encoded data.
        /// </summary>
        /// <param name="inputOpusData">Opus encoded data to decode, null for dropped packet.</param>
        /// <param name="dataLength">Length of data to decode.</param>
        /// <param name="decodedLength">Set to the length of the decoded sample data.</param>
        /// <returns>PCM audio samples.</returns>
        public unsafe void DecodeFloat(ReadOnlySpan<byte> inputOpusData, Span<float> outputFloat, out int dataLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusDecoder");

            int frameCount = GetSamples(inputOpusData) * OutputChannels;
            int length;

            fixed (byte* inputPtr = inputOpusData)
            fixed (float* outputPtr = outputFloat)
            {
                if (inputOpusData != null)
	                length = OpusAPI.opus_decode_float(DecoderInstance, inputPtr, inputOpusData.Length, outputPtr, frameCount, 0);
	            else
	                length = OpusAPI.opus_decode_float(DecoderInstance, null, 0, outputPtr, frameCount, (ForwardErrorCorrection) ? 1 : 0);
            }

            if (length < 0)
                throw new Exception("Decoding failed - " + (Errors)length);

            dataLength = length * OutputChannels;
        }

        /// <summary>
        /// Determines the number of frames that can fit into a buffer of the given size.
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public int FrameCount(int bufferSize)
        {
            // seems like bitrate should be required
            const int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * OutputChannels;
            return bufferSize / bytesPerSample;
        }

        public unsafe int GetChannels(byte[] data)
        {
            fixed (byte* ptr = data)
				return OpusAPI.opus_packet_get_nb_channels(ptr);
        }

        public unsafe int GetFrames(byte[] data)
        {
            fixed (byte* ptr = data)
				return OpusAPI.opus_packet_get_nb_frames(ptr, data.Length);
        }

        public unsafe int GetSamples(ReadOnlySpan<byte> data)
        {
            fixed (byte* ptr = data)
				return OpusAPI.opus_packet_get_nb_samples(ptr, data.Length, OutputSamplingRate);
        }

        ~OpusDecoder()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            if (DecoderInstance != IntPtr.Zero)
            {
                OpusAPI.opus_decoder_destroy(DecoderInstance);
                DecoderInstance = IntPtr.Zero;
            }

            disposed = true;
        }
    }
}