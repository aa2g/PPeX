using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public static class EncoderCommon
    {
        public static string ReadEncryptedString(this BinaryReader reader)
        {
            int length = reader.ReadInt32();
            byte[] array = reader.ReadBytes(length);

            for (int i = 0; i < length - 1; i++)
                array[i] = (byte)~array[i];

            string decrypted = Encoding.ASCII.GetString(array);
            if (decrypted.Length > 0)
                decrypted = decrypted.Remove(decrypted.Length - 1);

            return decrypted;
        }
        public static void WriteEncryptedString(this BinaryWriter writer, string String)
        {
            if (String.Length == 0)
            {
                writer.Write((int)0);
                return;
            }

            writer.Write((int)(String.Length + 1));


            byte[] array = Encoding.ASCII.GetBytes(String);

            Array.Resize(ref array, array.Length + 1);

            array[array.Length - 1] = 0;

            for (int i = 0; i < array.Length; i++)
                array[i] = (byte)~array[i];

            writer.Write(array);
        }

        public static uint[] DecodeAll(BinaryReader reader, int count, bool half, bool zigzag)
        {
            int[] deltas = new int[count];

            for (int i = 0; i < count; i++)
            {
                uint predelta;

                if (half)
                    predelta = DecodeHalf(reader);
                else
                    predelta = DecodeFull(reader);

                if (zigzag)
                    deltas[i] = DecodeZigzag(predelta);
                else
                    deltas[i] = (int)predelta;
            }

            uint[] output = new uint[count];
            uint lastValue = 0;

            for (int i = 0; i < count; i++)
            {
                output[i] = lastValue = (uint)(lastValue + deltas[i]);
            }

            return output;
        }

        public static uint ZigzagBase(int delta)
        {
            if (delta < 1)
            {
                return (uint)((-1 * delta) << 1);
            }
            else
            {
                return (uint)((delta << 1) - 1);
            }
        }

        public static byte[] ZigzagFull(int delta)
        {
            return EncodeFull(ZigzagBase(delta));
        }

        public static byte[] ZigzagHalf(int delta)
        {
            return EncodeHalf(ZigzagBase(delta));
        }

        public static int DecodeZigzag(uint zcoded)
        {
            if ((zcoded & 0x1) != 1)
                return (int)(((zcoded >> 1) * -1));
            else
                return (int)((zcoded + 1) >> 1);
        }

        public static byte[] EncodeFull(uint code)
        {
            List<byte> encoded = new List<byte>();

            while ((code & 0xFFFFFF80) > 0U)
            {
                byte section = (byte)(code | 0x80);
                code >>= 7;

                encoded.Add(section);
            }

            byte final = (byte)code;
            encoded.Add(final);

            return encoded.ToArray();
        }

        public static byte[] EncodeHalf(uint code)
        {
            List<byte> encoded = new List<byte>();
            uint newCode = code;

            while ((newCode & 0xFFFFFFF8) > 0U)
            {
                byte section = (byte)((newCode | 0x8) & 0xF);
                newCode >>= 3;
                encoded.Add(section);
            }

            byte final = (byte)(newCode & 0x7);
            encoded.Add(final);

            List<byte> encoded2 = new List<byte>();
            //if (encoded.Count > 1)
            for (int i = 0; i < encoded.Count - 1; i += 2)
            {
                encoded2.Add((byte)((encoded[i + 1] << 4) | encoded[i]));
            }

            if (encoded.Count % 2 == 1)
                encoded2.Add(encoded.Last());

            return encoded2.ToArray();
        }

        public static uint DecodeFull(BinaryReader reader)
        {
            byte current = 0xFF;

            uint full = 0;

            int shift = 0;

            while ((current & 0x80) != 0)
            {
                current = reader.ReadByte();

                full |= (uint)(current & 0x7F) << shift;

                shift += 7;
            }

            return full;
        }

        public static uint DecodeHalf(BinaryReader reader)
        {
            byte current = reader.ReadByte();
            int shift = 0;

            uint full = 0;

            while (true)
            {
                full |= (uint)((current & 0x7) << shift);

                if ((current & 0x8) == 0)
                    break;

                shift += 3;

                current >>= 4;

                full |= (uint)((current & 0x7) << shift);

                if ((current & 0x8) == 0)
                    break;

                shift += 3;

                current = reader.ReadByte();
            }

            return full;
        }
    }
}
