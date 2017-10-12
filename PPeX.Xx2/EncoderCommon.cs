using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    internal static class EncoderCommon
    {


        public static byte[] ZigzagFull(int delta)
        {
            if (delta < 1)
            {
                return EncodeFull((uint)((-1 * delta) * 2));
            }
            else
            {
                return EncodeFull((uint)(((-1 * delta) * 2) + 1));
            }
        }

        public static byte[] ZigzagHalf(int delta)
        {
            if (delta < 1)
            {
                return EncodeHalf((uint)((-1 * delta) * 2));
            }
            else
            {
                return EncodeHalf((uint)(((-1 * delta) * 2) + 1));
            }
        }

        public static byte[] EncodeFull(uint code)
        {
            List<byte> encoded = new List<byte>();

            while ((code & 0xFFFFFF00) > 0U)
            {
                byte section = (byte)(code | 0x80);
                code >>= 7;

                encoded.Add(section);
            }

            byte final = (byte)(code & 0x7F);
            encoded.Add(final);

            return encoded.ToArray();
        }

        public static byte[] EncodeHalf(uint code)
        {
            List<byte> encoded = new List<byte>();

            while ((code & 0xFFFFFFF0) > 0U)
            {
                byte section = (byte)((code | 0xF) & 0xF);
                code >>= 3;
                encoded.Add(section);
            }

            byte final = (byte)(code & 0xF);
            encoded.Add(final);

            List<byte> encoded2 = new List<byte>();
            //if (encoded.Count > 1)
            for (int i = 0; i < encoded.Count - 1; i += 2)
            {
                encoded2.Add((byte)((encoded[i] << 4) | encoded[i + 1]));
            }

            if (encoded.Count % 2 == 1)
                encoded2.Add((byte)(encoded.Last() << 4));

            return encoded.ToArray();
        }
    }
}
