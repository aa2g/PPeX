﻿using System;
using System.Buffers;
using System.IO;
using PPeX.External.libresample;
using PPeX.External.Ogg;
using PPeX.External.Opus;
using PPeX.External.Wave;

namespace PPeX.Encoders
{
    public class OpusEncoder : IEncoder
    {
        public ArchiveFileType Encoding => ArchiveFileType.OpusAudio;

        public ArchiveDataType DataType => ArchiveDataType.Audio;

        public void Encode(Stream input, Stream output)
        {
			using var bufferedWaveInput = new BufferedStream(input);
            using var wav = new WaveReader(bufferedWaveInput);

            byte channels = (byte) wav.Channels;

            int resampleRate = wav.SampleRate < 24000 ? 24000 : 48000;

            var application = channels > 1 ? Application.Audio : Application.Voip;

            using var opus = External.Opus.OpusEncoder.Create(resampleRate, channels, application);
            using var wrapper = new OggWrapper(output, channels, (ushort) opus.LookaheadSamples, true);
            using var resampler = new LibResampler(wav.SampleRate, resampleRate, channels);

            opus.Bitrate = channels > 1 ? Core.Settings.OpusMusicBitrate : Core.Settings.OpusVoiceBitrate;

            int rawSampleCount = (int) Math.Round(resampleRate * Core.Settings.OpusFrameSize);
            int inputSampleCount = (int) Math.Round(wav.SampleRate * Core.Settings.OpusFrameSize);
            int samplesToRead = rawSampleCount * channels;
            int inputSamplesToRead = inputSampleCount * channels;

            using var inputSampleBuffer = MemoryPool<float>.Shared.Rent(inputSamplesToRead);
			Span<float> inputSampleSpan = inputSampleBuffer.Memory.Span.Slice(0, inputSamplesToRead);

            int packetCount = 0;

            int outputSamplesUpperBound = resampler.ResampleUpperBound(samplesToRead);
            using var outputSampleBuffer = MemoryPool<float>.Shared.Rent(outputSamplesUpperBound);
            Span<float> outputSampleSpan = inputSampleBuffer.Memory.Span.Slice(0, outputSamplesUpperBound);

            using var encodedSampleBuffer = MemoryPool<byte>.Shared.Rent(outputSamplesUpperBound);
            var encodedSampleSpan = encodedSampleBuffer.Memory.Span;

			while (true)
            {
	            int result = wav.Read(inputSampleSpan);

	            if (result < inputSamplesToRead)
	            {
		            int newSize = result - (result % channels);

		            inputSampleSpan = inputSampleSpan.Slice(0, newSize);
	            }

	            resampler.Resample(inputSampleSpan, outputSampleSpan, result < inputSamplesToRead, out _);

	            opus.Encode(outputSampleSpan, encodedSampleSpan, rawSampleCount, out int outlen);

	            wrapper.WritePacket(encodedSampleBuffer.Memory.Slice(0, outlen),
		            (int)(48000 * Core.Settings.OpusFrameSize),
		            result < inputSamplesToRead);

	            if (result < inputSamplesToRead)
		            break;
            }
        }

        public void Decode(Stream input, Stream output)
		{
            int resampleRate = 44100;

            using OggReader reader = new OggReader(input);
            using BinaryWriter writer = new BinaryWriter(output, System.Text.Encoding.ASCII, false);
            using var decoder = OpusDecoder.Create(48000, reader.Channels);
            using LibResampler resampler = new LibResampler(48000, resampleRate, reader.Channels);

            bool isFirst = true;

            long headerPosition = output.Position;

            output.Position += 44;

            while (!reader.IsStreamFinished)
            {
	            var frame = reader.ReadPacket();

	            float[] outputSamples = decoder.DecodeFloat(frame.Span, frame.Length);
                    
	            if (isFirst)
	            {
		            //remove preskip
		            int preskip = reader.Preskip;

		            float[] newSamples = new float[outputSamples.Length - preskip];
		            Array.Copy(outputSamples, preskip, newSamples, 0, outputSamples.Length - preskip);

		            outputSamples = newSamples;

		            isFirst = false;
	            }

	            float[] resampledSamples = new float[resampler.ResampleUpperBound(frame.Length)];


                resampler.Resample(outputSamples, resampledSamples, reader.IsStreamFinished, out int outputLength);

	            foreach (float sample in resampledSamples)
	            {
		            writer.Write(WaveWriter.ConvertSample(sample));
	            }
            }

            long endPosition = output.Position;
            output.Position = headerPosition;
                
            WaveWriter.WriteWAVHeader(output, reader.Channels, (int)(endPosition - headerPosition), resampleRate, 16);

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