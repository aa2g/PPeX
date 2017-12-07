using PPeX.Common;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PPeX.External.Wave
{
    public class WaveReader : IDisposable
    {
        protected BinaryReader reader;
        protected long remainingData;

        public int Channels { get; protected set; }
        public int SampleRate { get; protected set; }

        protected int bitsPerSample;
        public int BitsPerSample => bitsPerSample;

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

            reader.ReadUInt32(); //chunk length

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

            bitsPerSample = reader.ReadInt16();

            string format = "";
            while ((format = reader.ReadString(4)) != "data")
                stream.Seek(reader.ReadUInt32(), SeekOrigin.Current);

            remainingData = reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            if (remainingData <= 0)
                throw new EndOfStreamException();

            remainingData--;

            return reader.ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            if (remainingData <= 1)
                throw new EndOfStreamException();

            remainingData -= 2;

            return reader.ReadInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadSampleAsInt()
        {
            if (bitsPerSample == 8)
            {
                //unsigned 8 bit LE

                return ReadByte();
            }
            else if (bitsPerSample == 16)
            {
                //signed 16 bit LE

                return ReadInt16();
            }
            else if (bitsPerSample == 24)
            {
                //signed 24 bit LE

                byte sample1 = ReadByte();
                byte sample2 = ReadByte();
                byte sample3 = ReadByte();

                //BitArray bitArray = new BitArray(new[] { sample1, sample2, sample3 });
                //needs reversed???

                BitArray bitArray = new BitArray(new[] { sample3, sample2, sample1 });

                bitArray[0] = bitArray[8];
                bitArray[8] = false;

                int[] value = new int[1];
                bitArray.CopyTo(value, 0);

                return value[0];
            }

            throw new NotSupportedException("Bits per sample value not supported.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSampleAsFloat()
        {
            if (bitsPerSample == 8)
            {
                //unsigned 8 bit LE
                return (float)((ReadByte() - 128) / 128.0);
            }
            else if (bitsPerSample == 16)
            {
                //signed 16 bit LE

                return ReadInt16() / (float)short.MaxValue;
            }
            else if (bitsPerSample == 24)
            {
                //signed 24 bit LE
                
                return ReadSampleAsInt() / (float)0x7FFFFF;
            }

            throw new NotSupportedException("Bits per sample value not supported.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte[] buffer, int offset, int count)
        {
            return reader.BaseStream.Read(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(float[] buffer, int offset, int count)
        {
            int read = 0;
            for (int i = 0; i < count; i++)
            {
                if (remainingData == 0)
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
