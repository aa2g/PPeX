using System;
using System.IO;
using System.Linq;

namespace PPeX.External.CRC32
{
    public static class CRC32
    {
        private static readonly uint[] Table;

        static CRC32()
        {
            const uint poly = 0xedb88320;
            Table = new uint[256];
            for (uint i = 0; i < Table.Length; ++i)
            {
                var temp = i;
                for (var j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                        temp = (temp >> 1) ^ poly;
                    else
                        temp >>= 1;
                }
                Table[i] = temp;
            }
        }

        public static uint Compute(Stream stream)
        {
            var crc = 0xffffffff;

            int result = 0;

            while ((result = stream.ReadByte()) != -1)
            {
                byte t = (byte)result;
                var index = (byte)((crc & 0xff) ^ t);
                crc = (crc >> 8) ^ Table[index];
            }
            return ~crc;
        }

        public static uint Compute(Span<byte> bytes)
        {
            var crc = 0xffffffff;
            foreach (var t in bytes)
            {
                var index = (byte)((crc & 0xff) ^ t);
                crc = (crc >> 8) ^ Table[index];
            }
            return ~crc;
        }

        public static byte[] ComputeToBytes(byte[] bytes) => BitConverter.GetBytes(Compute(bytes)).Reverse().ToArray();

        public static string ComputeToString(byte[] bytes) => BitConverter.ToString(ComputeToBytes(bytes)).Replace("-", string.Empty);
    }
}
