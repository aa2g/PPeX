using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using System.IO;

namespace FragLabs.Audio.Codecs
{
    public class OpusWaveProvider : IWaveProvider, IDisposable
    {
        protected MemoryStream internalstream;

        public OpusWaveProvider(Stream stream, uint length, int channels)
        {
            WaveFormat = new WaveFormat(48000, 16, channels);

            using (MemoryStream temp = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(temp))
            using (OpusDecoder decoder = OpusDecoder.Create(48000, channels))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                //long offset = stream.Position;
                for (int i = 0; i < length; i++)
                //while (stream.Position < length)
                {
                    int framesize = (int)reader.ReadUInt32();
                    byte[] frame = reader.ReadBytes(framesize);

                    int cchannels = decoder.GetChannels(frame);
                    int frames = decoder.GetFrames(frame);
                    int samples = decoder.GetSamples(frame);

                    int outputlen;
                    byte[] output = decoder.Decode(frame, framesize, out outputlen);
                    writer.Write(output, 0, outputlen);
                }

                internalstream = new MemoryStream(temp.ToArray());
            }
        }

        public WaveFormat WaveFormat { get; protected set; }

        public void Dispose()
        {
            ((IDisposable)internalstream).Dispose();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return internalstream.Read(buffer, offset, count);
        }
    }
}
