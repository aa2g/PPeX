using System;
using System.IO;
using FragLabs.Audio.Codecs;
using NAudio.Wave;

namespace PPeX.Encoders
{
    public class XggDecoder : BaseDecoder
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

        public XggDecoder(Stream encodedData) : base(encodedData)
        {

        }

        public override ArchiveFileType Encoding => ArchiveFileType.XggAudio;

        protected byte[] internalDecodedData;

        protected void internalDecode(Stream output)
        {
            //We want to make it look like a .wav file to the game

            using (BinaryReader reader = new BinaryReader(BaseStream, System.Text.Encoding.ASCII, true))
            {
                string magic = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(3));

                if (XggEncoder.Magic != magic)
                    throw new InvalidDataException("Supplied file is not an XGG wrapped file.");

                byte version = reader.ReadByte();

                if (version != XggEncoder.Version)
                    throw new InvalidDataException("Supplied XGG wrapped file is of an incompatible version.");

                uint FrameSize = reader.ReadUInt32();
                uint Bitrate = reader.ReadUInt32();
                byte Channels = reader.ReadByte();
                uint count = reader.ReadUInt32();

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    using (OpusWaveProvider wav = new OpusWaveProvider(BaseStream, count, Channels))
                    using (MediaFoundationResampler resampler = new MediaFoundationResampler(wav, new WaveFormat(44100, 16, Channels)))
                    {
                        WaveFileWriter.WriteWavFileToStream(output, resampler);
                    }
                }
                else
                {
                    using (OpusSampleProvider samp = new OpusSampleProvider(BaseStream, count, Channels))
                        WaveFileWriter.WriteWavFileToStream(output, samp.ToWaveProvider16());
                }
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
            return modified.Replace(".xgg", ".wav");
        }

        public override void Dispose()
        {
            internalDecodedData = null;
            IsEncoded = false;
            base.Dispose();
        }
    }
}
