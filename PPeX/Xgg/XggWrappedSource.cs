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
    public class XggWrappedSource : IDataSource
    {
        public readonly byte Version = 2;
        protected IDataSource basesource;

        public XggWrappedSource(IDataSource source)
        {
            _md5 = source.Md5;
            basesource = source;
        }

        /*
        public XggWrappedSource(Stream stream)
        {
            _md5 = Utility.GetMd5(stream);
            stream.Position = 0;
            Compile(stream);
        }
        */

        protected byte[] _md5;
        public byte[] Md5 => _md5;

        protected uint _size = 0;
        public uint Size => _size;

        private Stream Compile(Stream basestream)
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem, Encoding.ASCII, true))
            using (var wav = new WaveFileReader(basestream))
            using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                , 1)))
            {
                var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
                opus.Bitrate = Settings.XggBitrate;
                int packetsize = (int)(res.WaveFormat.SampleRate * 2 * Settings.XggFrameSize);

                writer.Write(Encoding.ASCII.GetBytes("XGG"));
                writer.Write(Version);
                writer.Write(packetsize);
                writer.Write(opus.Bitrate);

                long oldpos = mem.Position;
                ushort count = 0;
                writer.Write(count);

                byte[] buffer = new byte[packetsize];
                int result = res.Read(buffer, 0, packetsize);
                while (result > 0)
                {
                    count++;
                    int outlen = 0;
                    byte[] output = opus.Encode(buffer, result / 2, out outlen);
                    writer.Write((uint)outlen);
                    writer.Write(output, 0, outlen);

                    result = res.Read(buffer, 0, packetsize);
                }

                mem.Position = oldpos;
                writer.Write(count);
            }

            mem.Position = 0;
            _size = (uint)mem.Length;
            return mem;
        }

        //experimental multithreading attempts

        private Stream CompileMMX(Stream basestream)
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem, Encoding.ASCII, true))
            using (var wav = new WaveFileReader(basestream))
            using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                , 1)))
            using (MemoryStream decomp = new MemoryStream())
            {
                var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
                opus.Bitrate = Settings.XggBitrate;
                int packetsize = (int)(res.WaveFormat.SampleRate * 2 * Settings.XggFrameSize);

                writer.Write(Encoding.ASCII.GetBytes("XGG"));
                writer.Write(Version);
                writer.Write(packetsize);
                writer.Write(opus.Bitrate);

                long oldpos = mem.Position;
                ushort count = 0;
                writer.Write(count);

                List<int> lengths = new List<int>();

                byte[] buffer = new byte[packetsize];
                int result = res.Read(buffer, 0, packetsize);
                while (result > 0)
                {
                    decomp.Write(buffer, 0, packetsize);

                    result = res.Read(buffer, 0, packetsize);
                }

                byte[] dec = decomp.ToArray();
                decomp.Close();

                Parallel.For(0, (int)Math.Floor((double)dec.Length / packetsize), (i) =>
                {
                    ArraySegment<byte> segment = new ArraySegment<byte>(dec,
                        i * packetsize,
                        ((i + 1) * packetsize < dec.Length) ? packetsize : dec.Length % packetsize);

                    if (segment.Count < 2)
                        return;

                    int outlen = 0;
                    byte[] enc = opus.Encode(segment.ToArray(), segment.Count / 2, out outlen);
                    
                    Array.Copy(BitConverter.GetBytes((uint)outlen), 0, dec, segment.Offset, 4);

                    for (int ii = 0; ii < segment.Count - 4; i++)
                    {
                        if (ii < outlen)
                            dec[4 + segment.Offset + ii] = enc[ii];
                        else
                            dec[4 + segment.Offset + ii] = 0;

                    }

                    lengths[i] = outlen;
                });

                int total = lengths.Sum();
                byte[] output = new byte[total];

                for (int i = 0; i < lengths.Count; i++)
                {
                    mem.Write(dec, i * packetsize, lengths[i]);
                }

                mem.Position = oldpos;
                writer.Write(count);
            }

            mem.Position = 0;
            _size = (uint)mem.Length;
            return mem;
        }

        private Stream CompileMMX2(Stream basestream)
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem, Encoding.ASCII, true))
            using (var wav = new WaveFileReader(basestream))
            using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                wav.WaveFormat.SampleRate < 24000 ? 24000 : 48000
                , 1)))
            {
                var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
                opus.Bitrate = Settings.XggBitrate;
                int packetsize = (int)(res.WaveFormat.SampleRate * 2 * Settings.XggFrameSize);

                writer.Write(Encoding.ASCII.GetBytes("XGG"));
                writer.Write(Version);
                writer.Write(packetsize);
                writer.Write(opus.Bitrate);

                long oldpos = mem.Position;
                ushort count = 0;
                writer.Write(count);

                byte[] uncompressed = new byte[10000 + (res.WaveFormat.AverageBytesPerSecond / wav.WaveFormat.AverageBytesPerSecond) * wav.Length];

                byte[] buffer = new byte[packetsize];
                int result = res.Read(buffer, 0, packetsize);
                int offset = 0;
                while (result > 0)
                {
                    /*
                    count++;
                    int outlen = 0;
                    byte[] output = opus.Encode(buffer, result / 2, out outlen);
                    writer.Write((uint)outlen);
                    writer.Write(output, 0, outlen);*/

                    Array.Copy(buffer, 0, uncompressed, offset, result);
                    offset += result;

                    result = res.Read(buffer, 0, packetsize);
                }
                

                mem.Position = oldpos;
                writer.Write(count);
            }

            mem.Position = 0;
            _size = (uint)mem.Length;
            return mem;
        }

        public Stream GetStream()
        {
            using (Stream basestream = basesource.GetStream())
                return Compile(basestream);
        }
    }
}
