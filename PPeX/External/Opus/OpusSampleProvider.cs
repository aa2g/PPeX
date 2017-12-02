using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.IO;
using PPeX.External.libresample;

namespace FragLabs.Audio.Codecs
{
    public class OpusSampleProvider : IDisposable, ISampleProvider
    {
        protected List<float> floatList = new List<float>();

        public OpusSampleProvider(Stream stream, uint length, int channels)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, channels);
            }
            else
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, channels);
            }

            MemoryStream temp = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(temp, Encoding.ASCII, true))
            using (OpusDecoder decoder = OpusDecoder.Create(48000, channels))
            using (BinaryReader reader = new BinaryReader(stream))
            using (LibResampler resampler = new LibResampler(48000, 44100, channels))
            {
                for (int i = 0; i < length; i++)
                {
                    int framesize = (int)reader.ReadUInt32();
                    byte[] frame = reader.ReadBytes(framesize);

                    int cchannels = decoder.GetChannels(frame);
                    int frames = decoder.GetFrames(frame);
                    int samples = decoder.GetSamples(frame);

                    float[] output = decoder.DecodeFloat(frame, framesize);

                    if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                    {
                        output = resampler.Resample(output, out int sampleBufferUsed);
                    }

                    floatList.AddRange(output);
                }
            }
        }

        public WaveFormat WaveFormat { get; protected set; }

        public void Dispose()
        {
            floatList.Clear();
        }

        int position = 0;
        public int Read(float[] buffer, int offset, int count)
        {
            int readCount = Math.Min(floatList.Count - position, count);

            for (int i = 0; i < readCount; i++)
            {
                buffer[i + offset] = floatList[position + i];
            }

            position += readCount;

            return readCount;
        }
    }
}
