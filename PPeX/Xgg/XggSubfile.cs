using FragLabs.Audio.Codecs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xgg
{
    public class XggSubfile : BaseSubfile
    {
        public readonly string Magic = "XGG";
        public readonly byte Version = 2;

        protected uint _size;
        public override uint Size => _size;

        public XggSubfile(IDataSource Source, string Name, string Archive) : base(Source, Name, Archive)
        {
            _name = Name.Replace(".xgg", ".wav");
#warning change to a bitrate scaled estimate
            _size = Source.Size * 30; //fast estimate
        }

        public int Bitrate;
        public int FrameSize;

        public override void WriteToStream(Stream stream)
        {
            using (Stream source = Source.GetStream())
            using (BinaryReader reader = new BinaryReader(source, Encoding.ASCII, true))
            {
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(3));

                if (Magic != magic)
                    throw new InvalidDataException("Supplied file is not an XGG wrapped file.");

                byte version = reader.ReadByte();

                if (version != Version)
                    throw new InvalidDataException("Supplied XGG wrapped file is of an incompatible version.");

                FrameSize = reader.ReadInt32();
                Bitrate = reader.ReadInt32();
                uint count = reader.ReadUInt16();

                using (OpusWaveProvider wav = new OpusWaveProvider(source, count))
                    WaveFileWriter.WriteWavFileToStream(stream, wav);
            }
        }
    }
}
