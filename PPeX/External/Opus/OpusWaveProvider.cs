using System;
using System.Text;
using NAudio.Wave;
using System.IO;

namespace FragLabs.Audio.Codecs
{
    public class OpusWaveProvider : IWaveProvider, IDisposable
    {
        public MemoryStream InternalStream { get; protected set; }

        public long WAVLength => InternalStream.Length + 44;

        public OpusWaveProvider(Stream stream, uint length, int channels)
        {
            WaveFormat = new WaveFormat(48000, 16, channels);

            using (MemoryStream temp = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(temp))
            using (OpusDecoder decoder = OpusDecoder.Create(48000, channels))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                for (int i = 0; i < length; i++)
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

                InternalStream = new MemoryStream(temp.ToArray());
            }
        }

        public WaveFormat WaveFormat { get; protected set; }

        public void Dispose()
        {
            ((IDisposable)InternalStream).Dispose();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return InternalStream.Read(buffer, offset, count);
        }

        public void ExportWAVToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);
            //descriptor
            writer.WriteString("RIFF");
            writer.Write((uint)(36 + InternalStream.Length));
            writer.WriteString("WAVE");

            //fmt subchunk
            writer.WriteString("fmt ");
            writer.Write((uint)16);
            writer.Write((ushort)1);
            writer.Write((ushort)WaveFormat.Channels);
            writer.Write((uint)(WaveFormat.SampleRate));
            writer.Write((uint)(WaveFormat.SampleRate * WaveFormat.Channels * (WaveFormat.BitsPerSample / 2)));
            writer.Write((ushort)(WaveFormat.Channels * (WaveFormat.BitsPerSample / 2)));
            writer.Write((ushort)(WaveFormat.BitsPerSample));

            //data subchunk
            writer.WriteString("data");
            writer.Write((uint)InternalStream.Length);

            writer.Flush();

            InternalStream.Position = 0;
            InternalStream.CopyTo(stream);
        }
    }
}