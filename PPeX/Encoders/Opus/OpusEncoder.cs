using System;
using System.Collections.Generic;
using System.IO;
using PPeX.External.Wave;
using FragLabs.Audio.Codecs;
using PPeX.External.libresample;
using System.Linq;
using PPeX.External.Ogg;

namespace PPeX.Encoders
{
    public class OpusEncoder : BaseEncoder
    {
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

        public OpusEncoder(IDataSource source, bool preserveStereo) : base(source)
        {
            PreserveStereo = preserveStereo;
        }

        public override ArchiveFileType Encoding => ArchiveFileType.OpusAudio;

        public override uint EncodedLength { get; protected set; }

        protected byte[] internalEncodedData;
        protected bool IsEncoded = false;

        protected MemoryStream internalEncode()
        {
            var mem = new MemoryStream();

            try
            {
                using (var data = Source.GetStream())
                using (var wav = new WaveReader(data))
                {
#warning need to add preserve stereo option
                    byte channels = (byte)wav.Channels;

                    int resampleRate = wav.SampleRate < 24000 ? 24000 : 48000;

                    var application = channels > 1 ?
                        FragLabs.Audio.Codecs.Opus.Application.Audio :
                        FragLabs.Audio.Codecs.Opus.Application.Voip;

                    using (var wrapper = new OggWrapper(mem, channels, true))
                    using (var opus = FragLabs.Audio.Codecs.OpusEncoder.Create(resampleRate, channels, application))
                    {
                        opus.Bitrate = channels > 1 ? Core.Settings.OpusMusicBitrate : Core.Settings.OpusVoiceBitrate;
                        int packetsize = (int)(resampleRate * Core.Settings.OpusFrameSize * 2 * channels);
                            
                        uint count = 0;

                        List<float> bufferedSamples = new List<float>();
                        int rawSampleCount = (int)Math.Round(resampleRate * Core.Settings.OpusFrameSize);
                        int samplesToRead = rawSampleCount * channels;

                        using (var resampler = new LibResampler(wav.SampleRate, resampleRate, channels))
                        {
                            float[] inputSampleBuffer = new float[samplesToRead];
                            int result = wav.Read(inputSampleBuffer, 0, samplesToRead);

                            while (result > 0)
                            {
                                float[] outputBuffer = resampler.Resample(inputSampleBuffer, result < samplesToRead, out int sampleBufferUsed);

                                bufferedSamples.AddRange(outputBuffer);

                                result = wav.Read(inputSampleBuffer, 0, samplesToRead);
                            }
                        }

                        while (bufferedSamples.Count > 0)
                        {
                            count++;

                            int trueSamplesToRead = Math.Min(bufferedSamples.Count, samplesToRead);

                            float[] outputBuffer = bufferedSamples.GetRange(0, trueSamplesToRead).ToArray();
                            bufferedSamples.RemoveRange(0, trueSamplesToRead);

                            Array.Resize(ref outputBuffer, samplesToRead);

                            byte[] output = opus.Encode(outputBuffer, rawSampleCount, out int outlen);

                            wrapper.WritePacket(output.Take(outlen).ToArray(), (int)(48000 * Core.Settings.OpusFrameSize), bufferedSamples.Count == 0);
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
            return original.Replace(".wav", ".opus");
        }

        public override void Dispose()
        {
            internalEncodedData = null;
            IsEncoded = false;
            base.Dispose();
        }
    }
}
