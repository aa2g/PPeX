using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using FragLabs.Audio.Codecs;
using PPeX.External.libresample;

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
                {
                    using (var wav = new WaveFileReader(data))
                    {
#warning need to add preserve stereo option
                        byte channels = (byte)wav.WaveFormat.Channels;

                        using (var resampler = new LibResampler(wav.WaveFormat.SampleRate,
                            wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                            , channels))
                        using (var opus = OpusEncoder.Create(resampler.SampleRate, channels, FragLabs.Audio.Codecs.Opus.Application.Audio))
                        {
                            opus.Bitrate = channels > 1 ? Core.Settings.XggMusicBitrate : Core.Settings.XggVoiceBitrate;
                            int packetsize = (int)(resampler.SampleRate * Core.Settings.XggFrameSize * 2 * channels);

                            writer.Write(System.Text.Encoding.ASCII.GetBytes(Magic));
                            writer.Write(Version);
                            writer.Write(packetsize);
                            writer.Write(opus.Bitrate);
                            writer.Write(channels);

                            long oldpos = mem.Position;
                            uint count = 0;
                            writer.Write(count);

                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                //mediafoundationresampler
                                using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                                    wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                                    , channels)))
                                {
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
                                }
                            }
                            else
                            {
                                //libresample
                                var sampleProvider = wav.ToSampleProvider();
                                int rawSampleCount = (int)Math.Round(resampler.SampleRate * Core.Settings.XggFrameSize);
                                int samplesToRead = rawSampleCount * channels;

                                float[] inputSampleBuffer = new float[samplesToRead];
                                int result = sampleProvider.Read(inputSampleBuffer, 0, samplesToRead);

                                List<float> bufferedSamples = new List<float>();

                                while (result > 0)
                                {
                                    float[] outputBuffer = resampler.Resample(inputSampleBuffer, out int sampleBufferUsed);

                                    bufferedSamples.AddRange(outputBuffer);

                                    result = sampleProvider.Read(inputSampleBuffer, 0, samplesToRead);
                                }


                                while (bufferedSamples.Count > 0)
                                {
                                    count++;

                                    int trueSamplesToRead = Math.Min(bufferedSamples.Count, samplesToRead);

                                    float[] outputBuffer = bufferedSamples.GetRange(0, trueSamplesToRead).ToArray();
                                    bufferedSamples.RemoveRange(0, trueSamplesToRead);

                                    Array.Resize(ref outputBuffer, samplesToRead);

                                    byte[] output = opus.Encode(outputBuffer, rawSampleCount, out int outlen);

                                    writer.Write((uint)outlen);
                                    writer.Write(output, 0, outlen);
                                }
                            }

                            mem.Position = oldpos;
                            writer.Write(count);
                        }
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
}
