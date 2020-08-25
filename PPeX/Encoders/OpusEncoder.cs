using System;
using System.Buffers;
using System.IO;
using PPeX.External.libresample;
using PPeX.External.Ogg;
using PPeX.External.Opus;
using PPeX.External.Wave;

namespace PPeX.Encoders
{
    public class OpusEncoder : IDisposable
    {
        public void Encode(Stream input, Stream output)
        {
			using var bufferedWaveInput = new BufferedStream(input);
            using var wav = new WaveReader(bufferedWaveInput);

            byte channels = (byte) wav.Channels;

            int resampleRate = wav.SampleRate < 24000 ? 24000 : 48000;

            using var opus = External.Opus.OpusEncoder.Create(resampleRate, channels, channels > 1 ? Application.Audio : Application.Voip);
            using var wrapper = new OggWrapper(output, channels, (ushort) opus.LookaheadSamples, true);
            using var resampler = new LibResampler(wav.SampleRate, resampleRate, channels);


            opus.Bitrate = channels > 1 ? Core.Settings.OpusMusicBitrate : Core.Settings.OpusVoiceBitrate;



            int rawSampleCount = (int) Math.Round(resampleRate * Core.Settings.OpusFrameSize);
            int samplesPerFrame = rawSampleCount * channels;



            using var tempFloatBuffer = MemoryPool<float>.Shared.Rent(8192);
			Span<float> tempFloatBufferSpan = tempFloatBuffer.Memory.Span;

            using var tempByteBuffer = MemoryPool<byte>.Shared.Rent(8192);
			Span<byte> tempByteBufferSpan = tempByteBuffer.Memory.Span;


            int outputSamplesUpperBound = resampler.ResampleUpperBound(samplesPerFrame);

            using var outputSampleBuffer = MemoryPool<float>.Shared.Rent(outputSamplesUpperBound * 3);
            Span<float> outputSampleSpan = outputSampleBuffer.Memory.Span.Slice(0, outputSamplesUpperBound * 3);

            int outputSpanStackPointer = 0;



            bool AppendToOutputSpan(Span<float> inputSampleSpan, Span<float> outputSampleSpan)
            {
	            int result = wav.Read(inputSampleSpan);

	            bool lastBuffer = false;

	            if (result < inputSampleSpan.Length)
	            {
		            lastBuffer = true;

		            int newSize = result - (result % channels);

		            inputSampleSpan = inputSampleSpan.Slice(0, newSize);
	            }

	            resampler.Resample(inputSampleSpan, outputSampleSpan.Slice(outputSpanStackPointer), lastBuffer, out var outputLength);

	            outputSpanStackPointer += outputLength;

	            return !lastBuffer;
            }

            void MoveOutputToEncoded(bool lastFrame, Span<float> outputSampleSpan, Span<byte> encodedSampleSpan)
            {
	            if (lastFrame)
	            {
                    outputSampleSpan.Slice(outputSpanStackPointer, samplesPerFrame - outputSpanStackPointer).Clear();
					// TODO: Tweak the frame size for end frames to minimize silent time
	            }

	            opus.Encode(outputSampleSpan.Slice(0, samplesPerFrame), encodedSampleSpan, rawSampleCount, out int outlen);

	            wrapper.WritePacket(tempByteBuffer.Memory.Slice(0, outlen),
		            rawSampleCount,
		            lastFrame);

	            if (!lastFrame)
	            {
		            outputSampleSpan.Slice(samplesPerFrame, outputSpanStackPointer - samplesPerFrame).CopyTo(outputSampleSpan);

		            outputSpanStackPointer -= samplesPerFrame;
	            }
            }

			while (true)
			{
				bool lastFrame = false;

				while (outputSpanStackPointer < samplesPerFrame && !lastFrame)
				{
					lastFrame = !AppendToOutputSpan(tempFloatBufferSpan, outputSampleSpan);
				}

				while (outputSpanStackPointer >= samplesPerFrame)
				{
					MoveOutputToEncoded(false, outputSampleSpan, tempByteBufferSpan);
				}

				if (lastFrame)
				{
					MoveOutputToEncoded(true, outputSampleSpan, tempByteBufferSpan);
					break;
				}
			}
        }

        public void Decode(Stream input, Stream output, bool resample)
		{
            int resampleRate = 44100;

            using OggReader reader = new OggReader(input);
            using BinaryWriter writer = new BinaryWriter(output, System.Text.Encoding.ASCII, true);
            using var decoder = OpusDecoder.Create(48000, reader.Channels);

            LibResampler resampler = null;

            if (resample)
				resampler = new LibResampler(48000, resampleRate, reader.Channels);

            bool isFirst = true;

            long headerPosition = output.Position;

            output.Position += 44;

            Span<float> inputBuffer = default;

            while (!reader.IsStreamFinished)
            {
	            var frame = reader.ReadPacket();

                if (isFirst)
                {
	                inputBuffer = new float[decoder.GetSamples(frame.Span) * decoder.OutputChannels];
                }

                decoder.DecodeFloat(frame.Span, inputBuffer, out int dataLength);

                var outputSamples = inputBuffer.Slice(0, dataLength);
                    
	            if (isFirst)
	            {
		            //remove preskip
		            outputSamples = outputSamples.Slice(reader.Preskip);

		            isFirst = false;
	            }

	            Span<float> outputSpan = outputSamples;

                if (resample)
				{
					float[] resampledSamples = new float[resampler.ResampleUpperBound(outputSamples.Length)];


					resampler.Resample(outputSamples, resampledSamples, reader.IsStreamFinished, out int outputLength);

					outputSpan = resampledSamples.AsSpan(0, outputLength);
                }

                foreach (float sample in outputSpan)
	            {
		            writer.Write(WaveWriter.ConvertSample(sample));
	            }
            }

            long endPosition = output.Position;
            output.Position = headerPosition;
                
            WaveWriter.WriteWAVHeader(output, reader.Channels, (int)(endPosition - headerPosition), resample ? resampleRate : 48000, 16);

            output.Position = endPosition;
		}

        public string RealNameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.opus";
        }

        public void Dispose() { }

        public static RequestedConversion CreateConversionArgs(int musicBitrate = 44000, int voiceBitrate = 32000, bool resample = true)
        {
			return new RequestedConversion(ArchiveFileType.OpusAudio, new OpusEncoderArguments(musicBitrate, voiceBitrate, resample));
        }

		public class OpusEncoderArguments
		{
			public int MusicBitrate { get; set; }
			public int VoiceBitrate { get; set; }
			public bool Resample { get; set; }

			public OpusEncoderArguments(int musicBitrate, int voiceBitrate, bool resample)
			{
				MusicBitrate = musicBitrate;
				VoiceBitrate = voiceBitrate;
				Resample = resample;
			}
		}
    }
}