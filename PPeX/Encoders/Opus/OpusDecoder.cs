using System;
using System.IO;
using FragLabs.Audio.Codecs;
using NAudio.Wave;
using PPeX.External.Ogg;

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

            using (OggReader reader = new OggReader(BaseStream))
            using (OpusSampleProvider samp = new OpusSampleProvider(reader))
                WaveFileWriter.WriteWavFileToStream(output, samp.ToWaveProvider16());
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
