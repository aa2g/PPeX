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
    /// <summary>
    /// A subfile that is specialized for audio (uses specialized opus wrapper .xgg).
    /// </summary>
    public class XggSubfile : BaseSubfile
    {
        public readonly string Magic = "XGG";
        public readonly byte Version = 3;

        protected uint _size;
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public override uint Size => _size;

        /// <summary>
        /// Creates a new .xgg subfile from audio data.
        /// </summary>
        /// <param name="Source">The source of the data.</param>
        /// <param name="Name">The name of the subfile.</param>
        /// <param name="Archive">The name of the .pp archive to associate with.</param>
        public XggSubfile(IDataSource Source, string Name, string Archive) : base(Source, Name, Archive)
        {
            //We want to make it look like a .wav file to the game
            _name = Name.Replace(".xgg", ".wav");

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
                Channels = reader.ReadByte();
                uint count = reader.ReadUInt16();

                _size = (uint)(FrameSize * (count + 4) * 2); //add 4 to keep a good buffer in case
            }
        }

        /// <summary>
        /// The maximum size of each individual opus frames.
        /// </summary>
        public int FrameSize;
        /// <summary>
        /// The (approximate) bitrate of the opus stream.
        /// </summary>
        public int Bitrate;
        /// <summary>
        /// The amount of channels contained in the opus stream.
        /// </summary>
        public int Channels;

        /// <summary>
        /// Writes a compressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the compressed data to.</param>
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
                Channels = reader.ReadByte();
                uint count = reader.ReadUInt16();

                using (OpusWaveProvider wav = new OpusWaveProvider(source, count, Channels))
                    WaveFileWriter.WriteWavFileToStream(stream, wav);
            }
        }
    }
}
