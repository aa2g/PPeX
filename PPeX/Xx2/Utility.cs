using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    internal static class Utility
    {
        public static byte[] GetMd5(byte[] data)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(data);
        }

        public static bool CompareBytes(byte[] arrayA, byte[] arrayB)
        {
            if (arrayA.Length != arrayB.Length)
                return false;

            for (int i = 0; i < arrayA.Length; i++)
                if (arrayA[i] != arrayB[i])
                    return false;

            return true;
        }

        public class ByteEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                return CompareBytes(x, y);
            }

            public int GetHashCode(byte[] obj)
            {
                return obj.Sum(x => x.GetHashCode());
            }
        }
    }
}
