using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX;
using NAudio.Wave;
using FragLabs.Audio.Codecs;

namespace PPeX.Xgg
{
    /// <summary>
    /// A data source that converts an existing data source into an .xgg stream.
    /// </summary>
    public class XggWrappedSource : IDataSource
    {
        public readonly byte Version = 3;
        protected IDataSource basesource;

        /// <summary>
        /// Creates a new .xgg stream.
        /// </summary>
        /// <param name="source">The data source to convert.</param>
        public XggWrappedSource(IDataSource source)
        {
            _md5 = source.Md5;
            basesource = source;
        }

        protected byte[] _md5;
        /// <summary>
        /// The MD5 hash of the uncompressed data.
        /// </summary>
        public byte[] Md5 => _md5;

        protected uint _size = 0;
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public uint Size => _size;

        private Stream Compile(Stream basestream)
        {
            var mem = new MemoryStream();

            try
            {
                using (var writer = new BinaryWriter(mem, Encoding.ASCII, true))
                using (var wav = new WaveFileReader(basestream))
                {
#warning need to add preserve stereo option
                    byte channels = (byte)wav.WaveFormat.Channels;
                    
                    using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                        wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                        , channels)))
                    {
                        var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, channels, FragLabs.Audio.Codecs.Opus.Application.Audio);
                        opus.Bitrate = Core.Settings.XggBitrate;
                        int packetsize = (int)(res.WaveFormat.SampleRate * Core.Settings.XggFrameSize * 2 * channels);

                        writer.Write(Encoding.ASCII.GetBytes("XGG"));
                        writer.Write(Version);
                        writer.Write(packetsize);
                        writer.Write(opus.Bitrate);
                        writer.Write(channels);

                        long oldpos = mem.Position;
                        ushort count = 0;
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
                mem.Position = 0;
                _size = (uint)mem.Length;
            }

            return mem;
        }

        /// <summary>
        /// Returns an .xgg encoded stream.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            using (Stream basestream = basesource.GetStream())
                return Compile(basestream);
        }
    }
}
