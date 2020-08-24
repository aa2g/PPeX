using PPeX.Common;
using System;
using System.IO;
using System.Text;

namespace PPeX.External.Wave
{
    public class WaveWriter
    {
        public static short ConvertSample(float sample)
        {
            sample = Math.Max(sample, -1.0f);

            sample = Math.Min(sample, 1.0f);

            return (short)Math.Round(sample * short.MaxValue);
        }

        public static void WriteWAVHeader(Stream stream, int channels, int totalLength, int sampleRate, int bitRate = 16)
        {
	        using BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true);

	        int byteRate = channels * sampleRate * (bitRate / 8);

	        //descriptor
	        writer.WriteString("RIFF");
	        writer.Write((uint)(totalLength - 8));
	        writer.WriteString("WAVE");

	        //fmt subchunk
	        writer.WriteString("fmt ");
	        writer.Write((uint)16);
	        writer.Write((ushort)1);
	        writer.Write((ushort)channels);
	        writer.Write((uint)sampleRate);
	        writer.Write((uint)byteRate);
	        writer.Write((ushort)(channels * (bitRate / 8)));
	        writer.Write((ushort)bitRate);

	        //data subchunk
	        writer.WriteString("data");
	        writer.Write((uint)totalLength - 44);

	        writer.Flush();
        }
    }
}