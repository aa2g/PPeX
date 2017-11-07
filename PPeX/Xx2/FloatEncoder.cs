using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public struct FloatComponent
    {
        public bool Sign;
        public byte Exponent;
        public int Mantissa;

        public static explicit operator float(FloatComponent component)
        {
            int reconstructed = 0;

            if (component.Sign)
                reconstructed |= 0x1;

            reconstructed <<= 8;

            reconstructed |= component.Exponent;

            reconstructed <<= 23;

            reconstructed |= component.Mantissa;

            return BitConverter.ToSingle(BitConverter.GetBytes(reconstructed).Reverse().ToArray(), 0);
        }
    }

    public static class FloatEncoder
    {
        public static FloatComponent SplitFloat(float value)
        {
            byte[] bFloat = BitConverter.GetBytes(value);

            bool sign = (bFloat[0] & 0x80) != 0;

            byte exp = (byte)((bFloat[0] << 1) | ((bFloat[1] >> 7) & 0x01));

            int mantissa = ((bFloat[1] & 0x7F) << 16) | (bFloat[2] << 8) | bFloat[3];

            return new FloatComponent
            {
                Sign = sign,
                Exponent = exp,
                Mantissa = mantissa
            };
        }

        public static byte[] Encode(float[] values)
        {
            return Encode(values.Select(x => SplitFloat(x)).ToArray());
        }

        public static byte[] Encode(FloatComponent[] values)
        {
            List<byte> encoded = new List<byte>();

            //write dummy precision
            encoded.Add(0);

            //encode signs
            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].Sign)
                    encoded.Add(0);
                else
                    encoded.Add(1);
            }

            //encode exponents
            encoded.AddRange(IntegerEncoder.Encode(values.Select(x => x.Exponent).ToArray(), true, true));

            //encode mantissas
            encoded.AddRange(IntegerEncoder.Encode(values.Select(x => (uint)x.Mantissa).ToArray(), false, true));

            return encoded.ToArray();
        }

        static float[] DecodeLossless(BinaryReader reader, int count)
        {
            FloatComponent[] decoded = new FloatComponent[count];

            //decode signs
            for (int i = 0; i < count; i++)
            {
                decoded[i].Sign = reader.ReadByte() == 1;
            }

            //decode exponents
            uint[] exponents = IntegerEncoder.DecodeHalf(reader, count, true);

            for (int i = 0; i < count; i++)
            {
                decoded[i].Exponent = (byte)exponents[i];
            }

            //decode mantissas

            uint[] mantissas = IntegerEncoder.DecodeHalf(reader, count, false);

            for (int i = 0; i < count; i++)
            {
                decoded[i].Mantissa = (int)mantissas[i];
            }

            return decoded.Select(x => (float)x).ToArray();
        }

        public static uint[] Quantize(float[] floats, int precision)
        {
            uint[] values = new uint[floats.Length];

            float minimum = floats.Min();
            float maximum = floats.Max();
            float total = maximum - minimum;

            uint largest = (uint)((1 << precision) - 1);

            for (int i = 0; i < floats.Length; i++)
            {
                float value = floats[i] - minimum;

                float result = (value * largest) / total;

                values[i] = (uint)(Math.Round(result, MidpointRounding.AwayFromZero));
            }

            return values;
        }

        public static byte[] Encode(float[] floats, double quality)
        {
            if (quality == 0)
                return Encode(floats); //fallback to lossless method

            //convert quality to a precision level
            double range = (double)floats.Max() - (double)floats.Min();

            double granularity = 1 / (Math.Pow(2, quality));

            //range / ((2 ^ precision) - 1) = granularity

            //rearrange to:
            //precision = log2((range / granularity) + 1)
            double precision = Math.Log((range / granularity) + 1, 2);

            //round up
            precision = Math.Ceiling(precision);

            //use the normal encoding method
            return Encode(floats, (int)precision);
        }

        public static byte[] Encode(float[] floats, int precision)
        {
            if (precision == 0)
                return Encode(floats); //fallback to lossless method

            uint[] values = Quantize(floats, precision);

            List<byte> output = new List<byte>();
            
            //write precision
            output.Add((byte)precision);

            //write offset
            output.AddRange(BitConverter.GetBytes(floats.Min()));

            //write multiplier
            output.AddRange(BitConverter.GetBytes(floats.Max() - floats.Min()));

            //System.Diagnostics.Trace.WriteLine("Range: " + (floats.Max() - floats.Min()).ToString());

            //write values
            if (precision > 16)
                output.AddRange(IntegerEncoder.EncodeRaw(values));
            else
                output.AddRange(IntegerEncoder.EncodeRaw(values.Select(x => (ushort)x).ToArray()));

            return output.ToArray();
        }

        static float[] DecodeLossy(BinaryReader reader, int count, byte precision)
        {
            //read offset
            float offset = reader.ReadSingle();

            //read multiplier
            float multiplier = reader.ReadSingle();

            //uint[] values = IntegerEncoder.DecodeFull(reader, count, false);
            uint[] values = IntegerEncoder.DecodeRaw(reader, count, precision > 16);

            //calculate largest within precision
            uint largest = (uint)((1 << precision) - 1);

            //calculate original float
            float[] result = values.Select(x =>
                (((float)x / largest) * multiplier) + offset).ToArray();

            return result;
        }

        public static float[] Decode(BinaryReader reader, int count)
        {
            byte precision = reader.ReadByte();

            if (precision == 0)
                return DecodeLossless(reader, count);
            else
                return DecodeLossy(reader, count, precision);
        }
    }
}