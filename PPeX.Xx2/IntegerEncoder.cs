using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class IntegerEncoder
    {
        public static byte[] Encode(uint[] values, bool zigzag = true, bool half = false)
        {
            uint lastValue = 0;
            int[] deltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                deltas[i] = (int)(values[i] - lastValue);

                lastValue = values[i];
            }

            List<byte> encoded = new List<byte>();

            for (int i = 0; i < deltas.Length; i++)
            {
                if (zigzag)
                    if (half)
                        encoded.AddRange(EncoderCommon.ZigzagHalf(deltas[i]));
                    else
                        encoded.AddRange(EncoderCommon.ZigzagFull(deltas[i]));
                else
                    if (half)
                        encoded.AddRange(EncoderCommon.EncodeHalf((uint)deltas[i]));
                    else
                        encoded.AddRange(EncoderCommon.EncodeFull((uint)deltas[i]));
            }

            return encoded.ToArray();
        }


        public static byte[] Encode(ushort[] values, bool zigzag = true, bool half = false)
        {
            ushort lastValue = 0;
            int[] deltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                deltas[i] = values[i] - lastValue;

                lastValue = values[i];
            }

            List<byte> encoded = new List<byte>();

            for (int i = 0; i < deltas.Length; i++)
            {
                if (zigzag)
                    if (half)
                        encoded.AddRange(EncoderCommon.ZigzagHalf(deltas[i]));
                    else
                        encoded.AddRange(EncoderCommon.ZigzagFull(deltas[i]));
                else
                    if (half)
                    encoded.AddRange(EncoderCommon.EncodeHalf((uint)deltas[i]));
                else
                    encoded.AddRange(EncoderCommon.EncodeFull((uint)deltas[i]));
            }

            return encoded.ToArray();
        }


        public static byte[] Encode(byte[] values, bool zigzag = true, bool half = true)
        {
            byte lastValue = 0;
            int[] deltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                deltas[i] = values[i] - lastValue;

                lastValue = values[i];
            }

            List<byte> encoded = new List<byte>();

            for (int i = 0; i < deltas.Length; i++)
            {
                if (zigzag)
                    if (half)
                        encoded.AddRange(EncoderCommon.ZigzagHalf(deltas[i]));
                    else
                        encoded.AddRange(EncoderCommon.ZigzagFull(deltas[i]));
                else
                    if (half)
                    encoded.AddRange(EncoderCommon.EncodeHalf((uint)deltas[i]));
                else
                    encoded.AddRange(EncoderCommon.EncodeFull((uint)deltas[i]));
            }

            return encoded.ToArray();
        }


        public static uint[] DecodeFull(System.IO.BinaryReader reader, int count, bool zigzag = true)
        {
            return EncoderCommon.DecodeAll(reader, count, false, zigzag);
        }

        public static uint[] DecodeHalf(System.IO.BinaryReader reader, int count, bool zigzag = true)
        {
            return EncoderCommon.DecodeAll(reader, count, true, zigzag);
        }
    }
}
