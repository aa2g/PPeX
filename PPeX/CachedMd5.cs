using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class CachedMd5
    {
        public string Filename { get; set; }

        public DateTime LastModified { get; set; }

        public long Length { get; set; }

        public Md5Hash Hash { get; set; }

        public override string ToString()
        {
            return Hash.ToString();
        }

        public string ToWritableString()
        {
            return Filename + "|" + LastModified.ToBinary().ToString() + "|" + Length.ToString() + "|" + Hash.ToString();
        }

        public static CachedMd5 FromString(string value)
        {
            string[] array = value.Split('|');

            CachedMd5 md5 = new CachedMd5();
            md5.Filename = array[0];
            md5.LastModified = DateTime.FromBinary(long.Parse(array[1]));
            md5.Length = long.Parse(array[2]);
            md5.Hash = (Md5Hash)array[3];

            return md5;
        }

        public static CachedMd5 FromFile(string path)
        {
            CachedMd5 md5 = new CachedMd5();
            md5.Filename = path;

            FileInfo info = new FileInfo(path);

            md5.LastModified = info.LastWriteTime;
            md5.Length = info.Length;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                md5.Hash = Utility.GetMd5(stream);

            return md5;
        }

        public bool WeakFileCompare(string path)
        {
            FileInfo info = new FileInfo(path);
            return path == Filename && info.Length == Length && info.LastWriteTime == LastModified;
        }
    }
}
