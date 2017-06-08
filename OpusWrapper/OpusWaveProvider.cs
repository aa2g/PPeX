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

        public OpusWaveProvider(Stream stream, uint length)
        {
           using (MemoryStream temp = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(temp))
            using (OpusDecoder decoder = OpusDecoder.Create(48000, 1))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                //long offset = stream.Position;
                for (int i = 0; i < length; i++)
                //while (stream.Position < length)
                {
                    int framesize = (int)reader.ReadUInt32();
                    byte[] frame = reader.ReadBytes(framesize);

                    int outputlen;
                    byte[] output = decoder.Decode(frame, framesize, out outputlen);
                    writer.Write(output, 0, outputlen);
                }

                internalstream = new MemoryStream(temp.ToArray());
            }
        }

        public WaveFormat WaveFormat => new WaveFormat(48000, 16, 2);

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
