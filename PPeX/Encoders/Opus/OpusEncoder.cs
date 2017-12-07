using System;
using System.Collections.Generic;
using System.IO;
using FragLabs.Audio.Codecs;
using PPeX.External.libresample;
using System.Linq;
using PPeX.External.Ogg;
using PPeX.External.Wave;

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
                    
                    using (var opus = FragLabs.Audio.Codecs.OpusEncoder.Create(resampleRate, channels, application))
                    using (var wrapper = new OggWrapper(mem, channels, (ushort)opus.LookaheadSamples, true))
                    using (var resampler = new LibResampler(wav.SampleRate, resampleRate, channels))
                    {
                        opus.Bitrate = channels > 1 ? Core.Settings.OpusMusicBitrate : Core.Settings.OpusVoiceBitrate;
                        int packetsize = (int)(resampleRate * Core.Settings.OpusFrameSize * 2 * channels);
                        
                        int rawSampleCount = (int)Math.Round(resampleRate * Core.Settings.OpusFrameSize);
                        int inputSampleCount = (int)Math.Round(wav.SampleRate * Core.Settings.OpusFrameSize);
                        int samplesToRead = rawSampleCount * channels;
                        int inputSamplesToRead = inputSampleCount * channels;
                        
                        float[] inputSampleBuffer = new float[inputSamplesToRead];
                        int result = wav.Read(inputSampleBuffer, 0, inputSamplesToRead);

                        while (result > 0)
                        {
                            if (result < inputSamplesToRead)
                            {
                                int newSize = result;

                                if (channels == 2 && result % 2 == 1)
                                    newSize++;

                                Array.Resize(ref inputSampleBuffer, newSize);
                            }

                            float[] outputBuffer = resampler.Resample(inputSampleBuffer, result < inputSamplesToRead, out int sampleBufferUsed);

                            Array.Resize(ref outputBuffer, samplesToRead);

                            byte[] output = opus.Encode(outputBuffer, rawSampleCount, out int outlen);

                            wrapper.WritePacket(output.Take(outlen).ToArray(), (int)(48000 * Core.Settings.OpusFrameSize), result < inputSamplesToRead);
                                
                            result = wav.Read(inputSampleBuffer, 0, inputSamplesToRead);

                            Array.Clear(inputSampleBuffer, result, inputSampleBuffer.Length - result);
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
