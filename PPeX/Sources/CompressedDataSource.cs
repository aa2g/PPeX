using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.MediaFoundation;
using FragLabs.Audio.Codecs;

namespace PPeX
{
#warning remove
    public class CompressedDataSource : IDataSource
    {
        protected IDataSource basesource;
        protected ArchiveFileType type;

        public CompressedDataSource(IDataSource basesource, ArchiveFileType type)
        {
            this.basesource = basesource;
            this.type = type;

            _md5 = basesource.Md5;
            _size = basesource.Size;

            /*
            using (Stream stream = GetStream())
            {
                _md5 = Utility.GetMd5(stream);
                
                _size = (uint)stream.Length;
            }*/
        }

        protected byte[] _md5;
        public byte[] Md5 => _md5;

        protected uint _size;
        public uint Size => _size;

        protected Stream SerialAudioEncode()
        {
            var mem = new MemoryStream();
            try
            {
                using (var writer = new BinaryWriter(mem, Encoding.Unicode, true))
                using (var wav = new WaveFileReader(basesource.GetStream()))
                using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                    wav.WaveFormat.SampleRate < 32000 ? 24000 : 48000
                    , 1)))
                {
                    var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
                    opus.Bitrate = 96000;
                    int packetsize = (int)(res.WaveFormat.SampleRate * 0.04);
                    byte[] buffer = new byte[packetsize];
                    int result = res.Read(buffer, 0, packetsize);
                    while (result > 0)
                    {
                        int outlen = 0;
                        byte[] output = opus.Encode(buffer, result / 2, out outlen);
                        writer.Write((uint)outlen);
                        writer.Write(output, 0, outlen);

                        result = res.Read(buffer, 0, packetsize);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                mem.Position = 0;
                _size = (uint)mem.Length;
            }
            return mem;
        }

        protected Stream ParallelAudioEncode()
        {
            var mem = new MemoryStream();
            try
            {
                using (var writer = new BinaryWriter(mem, Encoding.Unicode, true))
                using (var wav = new WaveFileReader(basesource.GetStream()))
                using (var res = new MediaFoundationResampler(wav, new WaveFormat(
                    wav.WaveFormat.SampleRate < 32000 ? 24000 : 48000
                    , 1)))
                {
                    List<byte[]> holder = new List<byte[]>();
                    List<Task> work = new List<Task>();
                    //var mp3 = new LameMP3FileWriter(mem, res.WaveFormat, LAMEPreset.VBR_10);
                    var opus = OpusEncoder.Create(res.WaveFormat.SampleRate, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
                    opus.Bitrate = 128000;
                    int packetsize = (int)(res.WaveFormat.SampleRate * 0.04);

                    byte[] buffer = new byte[4096];
                    int result = res.Read(buffer, 0, 4096);
                    int counter = 0;
                    while (result > 0)
                    {
                        byte[] workset = buffer.ToArray();
                        int tempc = counter++;
                        work.Add(Task.Factory.StartNew(() =>
                        {
                            int outlen;
                            byte[] output = opus.Encode(workset, result / 2, out outlen);

                            byte[] trimmed = new byte[outlen = 4];

                            Array.Copy(BitConverter.GetBytes((uint)outlen), trimmed, 4);

                            Array.Copy(output, 0, trimmed, 4, outlen);

                            holder[tempc] = output;
                        }));

                        result = res.Read(buffer, 0, 4096);
                    }

                    Task.WaitAll(work.ToArray());

                    foreach (byte[] b in holder)
                    {
                        mem.Write(b, 0, b.Length);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                mem.Position = 0;
                _size = (uint)mem.Length;
            }
            return mem;
        }

        public Stream GetStream()
        {
            switch (type)
            {
                case ArchiveFileType.Audio:
                    return SerialAudioEncode();
            }
            return basesource.GetStream();
        }
    }
}
