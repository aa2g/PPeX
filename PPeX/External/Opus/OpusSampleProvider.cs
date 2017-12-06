using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.IO;
using PPeX.External.libresample;
using PPeX.External.Ogg;

namespace FragLabs.Audio.Codecs
{
    public class OpusSampleProvider : IDisposable, ISampleProvider
    {
        protected List<float> floatList = new List<float>();

        public OpusSampleProvider(OggReader reader)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, reader.Channels);

            MemoryStream temp = new MemoryStream();
            
            using (BinaryWriter writer = new BinaryWriter(temp, Encoding.ASCII, true))
            using (OpusDecoder decoder = OpusDecoder.Create(48000, reader.Channels))
            using (LibResampler resampler = new LibResampler(48000, 44100, reader.Channels))
            {
                while (!reader.IsStreamFinished)
                {
                    byte[] frame = reader.ReadPacket();

                    float[] output = decoder.DecodeFloat(frame, frame.Length);

                    output = resampler.Resample(output, reader.IsStreamFinished, out int sampleBufferUsed);

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
