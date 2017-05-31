using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Lame;
using FragLabs.Audio.Codecs;

namespace PPeX
{
#warning remove
    public class AudioSubfile : BaseSubfile
    {
        protected uint _size;
        public override uint Size => _size;

        public AudioSubfile(IDataSource Source, string Name, string Archive) : base(Source, Name, Archive)
        {
            _name = Name.Replace(".xgg", ".wav");
            _size = Source.Size * 15;
        }

        public override void WriteToStream(Stream stream)
        {
            using (Stream source = Source.GetStream())
            {
                //byte[] buffer = new byte[Source.Size];
                //source.Read(buffer, 0, (int)Source.Size);
                //using (MemoryStream mem = new MemoryStream(buffer))
                //using (Mp3FileReader mp3 = new Mp3FileReader(mem))
                using (OpusWaveProvider wav = new OpusWaveProvider(source, Source.Size))
                {
                    WaveFileWriter.WriteWavFileToStream(stream, wav);
                }
            }
            
        }
    }
}
