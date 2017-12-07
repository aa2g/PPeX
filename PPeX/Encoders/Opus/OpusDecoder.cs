using System;
using System.IO;
using FragLabs.Audio.Codecs;
using PPeX.External.Ogg;
using PPeX.External.libresample;
using System.Text;
using PPeX.External.Wave;

namespace PPeX.Encoders
{
    public class OpusDecoder : BaseDecoder
    {
        protected Stream _baseStream;
        protected bool IsEncoded = false;
        public override Stream BaseStream
        {
            get
            {
                return base.BaseStream;
            }
            protected set
            {
                IsEncoded = false;
                base.BaseStream = value;
            }
        }

        public OpusDecoder(Stream encodedData) : base(encodedData)
        {

        }

        public override ArchiveFileType Encoding => ArchiveFileType.OpusAudio;

        protected byte[] internalDecodedData;

        protected void internalDecode(Stream output)
        {
            //We want to make it look like a .wav file to the game

            int resampleRate = 44100;
            
            using (OggReader reader = new OggReader(BaseStream))
            using (MemoryStream temp = new MemoryStream())
            using (BinaryWriter tempWriter = new BinaryWriter(temp))
            using (var decoder = FragLabs.Audio.Codecs.OpusDecoder.Create(48000, reader.Channels))
            using (LibResampler resampler = new LibResampler(48000, resampleRate, reader.Channels))
            {
                bool isFirst = true;

                while (!reader.IsStreamFinished)
                {
                    byte[] frame = reader.ReadPacket();

                    float[] outputSamples = decoder.DecodeFloat(frame, frame.Length);

                    if (isFirst)
                    {
                        //remove preskip
                        int preskip = reader.Preskip;

                        float[] newSamples = new float[outputSamples.Length - preskip];
                        Buffer.BlockCopy(frame, preskip, newSamples, 0, outputSamples.Length - preskip);

                        outputSamples = newSamples;

                        isFirst = false;
                    }

                    outputSamples = resampler.Resample(outputSamples, reader.IsStreamFinished, out int sampleBufferUsed);

                    foreach (float sample in outputSamples)
                    {
                        tempWriter.Write(WaveWriter.ConvertSample(sample));
                    }
                }

                WaveWriter.WriteWAVHeader(output, reader.Channels, (int)temp.Length, resampleRate, 16);

                temp.Position = 0;
                temp.CopyTo(output);
            }
        }

        public override Stream Decode()
        {
            if (!IsEncoded)
            {
                using (MemoryStream buffer = new MemoryStream())
                {
                    internalDecode(buffer);
                    internalDecodedData = buffer.ToArray();
                }

                IsEncoded = true;
            }

            return new MemoryStream(internalDecodedData, false);
        }

        public override string NameTransform(string modified)
        {
            return modified.Replace(".opus", ".wav");
        }

        public override void Dispose()
        {
            internalDecodedData = null;
            IsEncoded = false;
            base.Dispose();
        }
    }
}
