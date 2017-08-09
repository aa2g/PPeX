using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using FragLabs.Audio.Codecs;

namespace PPeX.Encoders
{
    public class XggEncoder : BaseEncoder
    {
        public static readonly string Magic = "XGG";
        public static readonly byte Version = 4;

        public bool PreserveStereo { get; set; }

        public override IDataSource Source
        {
            get
            {
                return base.Source;
            }

            protected set
            {
                IsEncoded = false;
                base.Source = value;
            }
        }

        public XggEncoder(IDataSource source, bool preserveStereo) : base(source)
        {
            PreserveStereo = preserveStereo;
        }

        public override ArchiveFileType Encoding => ArchiveFileType.XggAudio;

        public override uint EncodedLength { get; protected set; }

        protected byte[] internalEncodedData;
        protected bool IsEncoded = false;

        protected MemoryStream internalEncode()
        {
            var mem = new MemoryStream();

            try
            {
                using (var writer = new BinaryWriter(mem, System.Text.Encoding.ASCII, true))
                using (var data = Source.GetStream())
                using (var wav = new WaveFileReader(data))
                {
#warning need to add preserve stereo option
                    byte channels = (byte)wav.WaveFormat.Channels;

                    using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                        wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                        , channels)))
                    using (var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, channels, FragLabs.Audio.Codecs.Opus.Application.Audio))
                    {

                        opus.Bitrate = Core.Settings.XggBitrate;
                        int packetsize = (int)(res.WaveFormat.SampleRate * Core.Settings.XggFrameSize * 2 * channels);

                        writer.Write(System.Text.Encoding.ASCII.GetBytes(Magic));
                        writer.Write(Version);
                        writer.Write(packetsize);
                        writer.Write(opus.Bitrate);
                        writer.Write(channels);

                        long oldpos = mem.Position;
                        uint count = 0;
                        writer.Write(count);

                        byte[] buffer = new byte[packetsize];
                        int result = res.Read(buffer, 0, packetsize);
                        while (result > 0)
                        {
                            count++;
                            int outlen = 0;
                            byte[] output = opus.Encode(buffer, packetsize / (2 * channels), out outlen);
                            writer.Write((uint)outlen);
                            writer.Write(output, 0, outlen);

                            result = res.Read(buffer, 0, packetsize);
                        }

                        mem.Position = oldpos;
                        writer.Write(count);
                    }
                }

            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is ArgumentException || ex is FormatException)
            {
                mem.SetLength(0);
            }
            finally
            {
                EncodedLength = (uint)mem.Length;
                mem.Position = 0;
            }

            return mem;
        }

        public override Stream Encode()
        {
            if (!IsEncoded)
            {
                using (MemoryStream buffer = internalEncode())
                    internalEncodedData = buffer.ToArray();
                IsEncoded = true;
            }
            
            return new MemoryStream(internalEncodedData, false);
        }

        public override string NameTransform(string original)
        {
            return original.Replace(".wav", ".xgg");
        }

        public override void Dispose()
        {
            internalEncodedData = null;
            IsEncoded = false;
            base.Dispose();
        }
    }

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

                using (OpusWaveProvider wav = new OpusWaveProvider(BaseStream, count, Channels))
                    WaveFileWriter.WriteWavFileToStream(output, wav);
                //wav.ExportWAVToStream(stream);
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
