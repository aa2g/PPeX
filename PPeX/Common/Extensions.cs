using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Common
{
    public static class Extensions
    {
        public static void WriteString(this BinaryWriter writer, string str)
        {
            writer.Write(Encoding.ASCII.GetBytes(str));
        }

        public static string ReadString(this BinaryReader reader, int length)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(length));
        }

        public static byte[] ToByteArray(this Stream str)
        {
            str.Position = 0;
            byte[] buffer = new byte[str.Length];
            str.Read(buffer, 0, (int)str.Length);
            return buffer;
        }
    }
}
