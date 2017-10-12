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

        public byte[] UnencodedData;

        public xxParser(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Version = reader.ReadInt32();

                Unknown = reader.ReadBytes(22);

                RootObject = new xxObject(reader, Version);

                UnencodedData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }
    }




    public static class xxExtensions
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
    }
}
