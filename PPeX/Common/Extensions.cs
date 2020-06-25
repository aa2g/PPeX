using System.IO;
using System.Text;

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
    }
}
