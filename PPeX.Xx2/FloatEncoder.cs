using System;
using System.Collections.Generic;
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

            //encode signs
            
            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].Sign)
                    encoded.Add(0);
                else
                    encoded.Add(1);
            }

            //encode exponents
            byte lastExponent = 0;
            int[] expDeltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                expDeltas[i] = values[i].Exponent - lastExponent;

                lastExponent = values[i].Exponent;
            }
            
            for (int i = 0; i < expDeltas.Length; i++)
            {
                encoded.AddRange(EncoderCommon.ZigzagHalf(expDeltas[i]));
            }

            //encode mantissas
            int lastMantissa = 0;
            int[] mantissaDeltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                mantissaDeltas[i] = values[i].Mantissa - lastMantissa;

                lastMantissa = values[i].Mantissa;
            }

            for (int i = 0; i < expDeltas.Length; i++)
            {
                encoded.AddRange(EncoderCommon.EncodeHalf((uint)mantissaDeltas[i]));
            }

            return encoded.ToArray();
        }

        public static uint[] Quantize(float[] floats, int precision)
        {
            uint[] values = new uint[floats.Length];

            float minimum = floats.Min();
            float maximum = floats.Max();
            float total = maximum - minimum;

            uint largest = (uint)((2 << precision) - 1);

            for (int i = 0; i < floats.Length; i++)
            {
                float value = floats[i] - minimum;

                float result = value * largest / total;

                values[i] = (uint)(Math.Round(result, MidpointRounding.AwayFromZero));
            }

            return values;
        }

        public static byte[] Encode(float[] floats, int precision)
        {
            uint[] values = Quantize(floats, precision);

            List<byte> output = new List<byte>();

            //write offset
            output.AddRange(BitConverter.GetBytes(floats.Min()));

            //write multiplier
            output.AddRange(BitConverter.GetBytes(floats.Max() - floats.Min()));

            output.AddRange(IntegerEncoder.Encode(values, false));

            return output.ToArray();
        }
    }
}