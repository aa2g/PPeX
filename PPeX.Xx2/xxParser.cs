using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxParser
    {
        public int Version { get; protected set; }

        public byte[] Unknown;

        public xxObject RootObject { get; protected set; }

        public xxParser(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Version = reader.ReadInt32();

                Unknown = reader.ReadBytes(22);

                RootObject = new xxObject(reader, Version);
            }
        }
    }




    public static class xxExtensions
    {
        public static string ReadEncryptedStringShort(this BinaryReader reader)
        {
            int length = reader.ReadUInt16();
            byte[] array = reader.ReadBytes(length);

            for (int i = 0; i < length - 1; i++)
                array[i] = (byte)~array[i];

            return Encoding.ASCII.GetString(array).TrimEnd('\0');
        }

        public static string ReadEncryptedStringInt(this BinaryReader reader)
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
    }
}
