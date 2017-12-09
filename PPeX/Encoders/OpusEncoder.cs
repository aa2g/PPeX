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
        public bool PreserveStereo { get; set; } = true;

        public OpusEncoder(Stream source) : base(source)
        {

        }

        public override ArchiveFileType Encoding => ArchiveFileType.OpusAudio;

        public override ArchiveDataType DataType => ArchiveDataType.Audio;

        public override Stream Encode()
        {
            var mem = new MemoryStream();

            try
            {
                using (var wav = new WaveReader(BaseStream))
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
                mem.Position = 0;
            }

            return mem;
        }

        public override Stream Decode()
        {
            MemoryStream output = new MemoryStream();

            int resampleRate = 44100;
            
            using (OggReader reader = new OggReader(BaseStream))
            using (MemoryStream temp = new MemoryStream())
            using (BinaryWriter tempWriter = new BinaryWriter(temp))
            using (var decoder = OpusDecoder.Create(48000, reader.Channels))
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
                        Buffer.BlockCopy(outputSamples, preskip, newSamples, 0, outputSamples.Length - preskip);

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

            output.Position = 0;
            return output;
        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.opus";
        }
    }
}
