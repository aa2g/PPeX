using PPeX.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.External.Wave
{
    public class WaveReader : IDisposable
    {
        protected BinaryReader reader;
        protected long remainingData;

        public int Channels { get; protected set; }
        public int SampleRate { get; protected set; }
        public int BitsPerSample { get; protected set; }

        public WaveReader(Stream stream)
        {
            reader = new BinaryReader(stream, Encoding.ASCII, true);

            if (reader.ReadString(4) != "RIFF")
                throw new InvalidDataException("Not a valid .wav file.");

            reader.ReadUInt32(); //file length

            if (reader.ReadString(4) != "WAVE")
                throw new InvalidDataException("Not a valid .wav file.");

            if (reader.ReadString(4) != "fmt ")
                throw new InvalidDataException("Not a valid .wav file.");

            reader.ReadUInt32(); //file length

            ushort audioFormat = reader.ReadUInt16();

#warning Support IEEE float wav files
            if (audioFormat == (ushort)AudioFormat.IEEE_FLOAT)
                throw new NotImplementedException("32-bit IEEE float wave files currently not supported.");
            else if (audioFormat != (ushort)AudioFormat.PCM)
                throw new NotSupportedException("Wave format not supported.");

            Channels = reader.ReadUInt16();
            SampleRate = reader.ReadInt32();

            reader.ReadInt32(); //byte rate
            reader.ReadInt16(); //block align

            BitsPerSample = reader.ReadInt16();
            
            if (reader.ReadString(4) != "data")
                throw new InvalidDataException("Not a valid .wav file.");

            remainingData = reader.ReadUInt32();
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public int ReadSampleAsInt()
        {
            if (BitsPerSample == 8)
            {
                //unsigned 8 bit LE
                return ReadByte();
            }
            else if (BitsPerSample == 16)
            {
                //signed 16 bit LE

                return reader.ReadInt16();
            }
            else if (BitsPerSample == 24)
            {
                //signed 24 bit LE

                int sample = ReadByte();
                sample |= ReadByte() << 8;

                byte MSB = ReadByte();

                sample |= (MSB & 0x7F) << 16;

                if ((MSB & 0x80) != 0)
                    sample *= -1;

                return sample;
            }

            throw new NotSupportedException("Bits per sample value not supported.");
        }

        public float ReadSampleAsFloat()
        {
            int sample = ReadSampleAsInt();

            if (BitsPerSample == 8)
            {
                //unsigned 8 bit LE
                return (float)((sample - 128) / 128.0);
            }
            else if (BitsPerSample == 16)
            {
                //signed 16 bit LE

                return sample / (float)short.MaxValue;
            }
            else if (BitsPerSample == 24)
            {
                //signed 24 bit LE
                
                return sample / (float)0x7FFFFF;
            }

            throw new NotSupportedException("Bits per sample value not supported.");
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return reader.BaseStream.Read(buffer, offset, count);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = 0;
            for (int i = 0; i < count; i++)
            {
                if (reader.BaseStream.Position == reader.BaseStream.Length)
                    return read;

                read++;
                buffer[i + offset] = ReadSampleAsFloat();
            }

            return read;
        }

        public void Dispose()
        {
            ((IDisposable)reader).Dispose();
        }

        protected enum AudioFormat : ushort
        {
            PCM = 0x0001,
            IEEE_FLOAT = 0x0003,
            ALAW = 0x006,
            ULAW = 0x007,
            EXTENSIBLE = 0xFFFE,
        }
    }
}
