using System;
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

        public static Encoding EncodingShiftJIS => Encoding.GetEncoding("Shift-JIS");

        public static byte[] DecryptHeaderBytes(byte[] buf)
        {
            byte[] table = new byte[]
            {
                0xFA, 0x49, 0x7B, 0x1C, // var48
				0xF9, 0x4D, 0x83, 0x0A,
                0x3A, 0xE3, 0x87, 0xC2, // var24
				0xBD, 0x1E, 0xA6, 0xFE
            };

            byte var28;
            for (int var4 = 0; var4 < buf.Length; var4++)
            {
                var28 = (byte)(var4 & 0x7);
                table[var28] += table[8 + var28];
                buf[var4] ^= table[var28];
            }

            return buf;
        }

        const byte FirstByte = 0x01;
        const int Version = 0x6C;
        readonly static byte[] ppVersionBytes = Encoding.ASCII.GetBytes("[PPVER]\0");

        public static uint HeaderSize(int numFiles)
        {
            return (uint)((288 * numFiles) + 9 + 12);
        }

        public static byte[] code = new byte[] {
				0x4D, 0x2D, 0xBF, 0x6A, 0x5B, 0x4A, 0xCE, 0x9D,
				0xF4, 0xA5, 0x16, 0x87, 0x92, 0x9B, 0x13, 0x03,
				0x8F, 0x92, 0x3C, 0xF0, 0x98, 0x81, 0xDB, 0x8E,
				0x5F, 0xB4, 0x1D, 0x2B, 0x90, 0xC9, 0x65, 0x00 };

        public static void EncryptPP(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] ^= code[i % 32];
        }

        public static byte[] CreateHeader(ExtendedArchive arc)
        {
            throw new NotImplementedException("bork");

            byte[] headerBuf = new byte[HeaderSize(arc.ArchiveFiles.Count)];
            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                writer.Write(ppVersionBytes);
                writer.Write(DecryptHeaderBytes(BitConverter.GetBytes(Version)));

                writer.Write(DecryptHeaderBytes(new byte[] { FirstByte }));
                writer.Write(DecryptHeaderBytes(BitConverter.GetBytes(arc.ArchiveFiles.Count)));

                byte[] fileHeaderBuf = new byte[288 * arc.ArchiveFiles.Count];
                uint fileOffset = (uint)headerBuf.Length;
                uint largestSize = 0;
                var list = arc.ArchiveFiles.ToArray();

                for (int i = 0; i < arc.ArchiveFiles.Count; i++)
                {
                    int idx = i * 288;
                    if (largestSize < list[i].Size)
                        largestSize = list[i].Size;

                    Utility.EncodingShiftJIS.GetBytes(list[i].Name).CopyTo(fileHeaderBuf, idx);
                    BitConverter.GetBytes(list[i].Size).CopyTo(fileHeaderBuf, idx + 260);
                    BitConverter.GetBytes((uint)headerBuf.Length).CopyTo(fileHeaderBuf, idx + 264);
                    
                    BitConverter.GetBytes(list[i].Size).CopyTo(fileHeaderBuf, idx + 284);
                }

                writer.Write(DecryptHeaderBytes(fileHeaderBuf));
                writer.Write(DecryptHeaderBytes(BitConverter.GetBytes(headerBuf.Length)));
#warning investigate this
                byte[] dummy = new byte[largestSize];
                EncryptPP(dummy);
                writer.Write(dummy);
                writer.Flush();

                return mem.ToArray();
            }
        }

        public static byte[] CreateHeader(ICollection<string> arc)
        {
            int count = arc.Count;
            byte[] headerBuf = new byte[HeaderSize(count)];
            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                writer.Write(ppVersionBytes);
                writer.Write(DecryptHeaderBytes(BitConverter.GetBytes(Version)));

                writer.Write(DecryptHeaderBytes(new byte[] { FirstByte }));
                writer.Write(DecryptHeaderBytes(BitConverter.GetBytes(arc.Count)));

                byte[] fileHeaderBuf = new byte[288 * count];
                uint fileOffset = (uint)headerBuf.Length;
                uint largestSize = 32 * 1024;
                var list = arc.ToArray();

                for (int i = 0; i < count; i++)
                {
                    int idx = i * 288;

                    Utility.EncodingShiftJIS.GetBytes(list[i]).CopyTo(fileHeaderBuf, idx);
                    BitConverter.GetBytes(12).CopyTo(fileHeaderBuf, idx + 260);
                    BitConverter.GetBytes(fileOffset).CopyTo(fileHeaderBuf, idx + 264);

                    BitConverter.GetBytes(12).CopyTo(fileHeaderBuf, idx + 284);
                }

                writer.Write(DecryptHeaderBytes(fileHeaderBuf));
                writer.Write(DecryptHeaderBytes(BitConverter.GetBytes(headerBuf.Length)));
#warning investigate this
                byte[] dummy = new byte[largestSize];
                EncryptPP(dummy);
                writer.Write(dummy);
                writer.Flush();

                return mem.ToArray();
            }
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
                    using (ZstdNet.Compressor zstd = new ZstdNet.Compressor(new ZstdNet.CompressionOptions(3)))
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
