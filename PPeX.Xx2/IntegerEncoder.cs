using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class IntegerEncoder
    {
        public static byte[] Encode(int[] values)
        {
            int lastValue = 0;
            int[] deltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                deltas[i] = values[i] - lastValue;

                lastValue = values[i];
            }

            List<byte> encoded = new List<byte>();

            for (int i = 0; i < deltas.Length; i++)
            {
                encoded.AddRange(EncoderCommon.ZigzagFull(deltas[i]));
            }

            return encoded.ToArray();
        }


        public static byte[] Encode(short[] values)
        {
            short lastValue = 0;
            int[] deltas = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                deltas[i] = values[i] - lastValue;

                lastValue = values[i];
            }

            List<byte> encoded = new List<byte>();

            for (int i = 0; i < deltas.Length; i++)
            {
                encoded.AddRange(EncoderCommon.ZigzagFull(deltas[i]));
            }

            return encoded.ToArray();
        }


        public static byte[] Encode(byte[] values)
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
                encoded.AddRange(EncoderCommon.ZigzagFull(deltas[i]));
            }

            return encoded.ToArray();
        }
    }
}
