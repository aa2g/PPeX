using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FragLabs.Audio.Codecs
{
    public static class Extensions
    {
        public static void WriteString(this BinaryWriter writer, string str)
        {
            writer.Write(Encoding.ASCII.GetBytes(str));
        }
    }
}
