﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PPeX
{
    public static class Utility
    {
        public static byte[] GetMd5(Stream stream)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(stream);
        }

        public static byte[] GetMd5(Stream stream, long offset, long length)
        {
            MD5 md5 = MD5.Create();
            Substream sub = new Substream(stream, offset, length);

            return md5.ComputeHash(sub);
        }

        public static long TestCompression(Stream data, ArchiveFileCompression method)
        {
            long output = -1;

            switch (method)
            {
                case ArchiveFileCompression.Uncompressed:
                    return data.Length;
                case ArchiveFileCompression.LZ4:
                    using (MemoryStream buffer = new MemoryStream())
                    using (LZ4.LZ4Stream lz4 = new LZ4.LZ4Stream(buffer, LZ4.LZ4StreamMode.Compress, LZ4.LZ4StreamFlags.HighCompression | LZ4.LZ4StreamFlags.IsolateInnerStream, 4 * 1024 * 1024))
                    {
                        data.CopyTo(lz4);
                        lz4.Close();
                        return buffer.Length;
                    }
                case ArchiveFileCompression.Zstandard:
                    using (ZstdNet.Compressor zstd = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(6)))
                    using (MemoryStream temp = new MemoryStream())
                    {
                        data.CopyTo(temp);
                        return zstd.Wrap(temp.ToArray()).LongLength;
                    }
            }

            return output;
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
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
    }
}
