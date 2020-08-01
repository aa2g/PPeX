﻿using System;

namespace PPeX
{
    public struct Md5Hash
    {
        public byte[] Hash { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Md5Hash hash)
                return Utility.CompareBytes(Hash, hash.Hash);

            if (obj is byte[] bytes)
	            return Utility.CompareBytes(Hash, bytes);

            if (obj is string s)
	            return Equals((Md5Hash)s);

            return false;
        }

        public static bool operator ==(Md5Hash hash1, byte[] hash2)
        {
            return hash1.Equals(hash2);
        }

        public static bool operator !=(Md5Hash hash1, byte[] hash2)
        {
            return !hash1.Equals(hash2);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return BitConverter.ToString(Hash).Replace("-", "");
        }

        public static implicit operator byte[](Md5Hash hash)
        {
            return hash.Hash;
        }

        public static implicit operator Md5Hash(byte[] hash)
        {
            return new Md5Hash
            {
                Hash = hash
            };
        }

        public static explicit operator Md5Hash(string hash)
        {
            return new Md5Hash
            {
                Hash = StringToByteArrayFastest(hash.Replace("-", ""))
            };
        }

        //from https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
        private static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < (hex.Length >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
