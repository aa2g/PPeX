using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crc32C;

namespace PPeX
{
    public class FileSource : IDataSource
    {
        public string Filename { get; protected set; }

        uint _size;
        public uint Size => _size;

        byte[] _md5;
        public byte[] Md5 => _md5;

        public FileSource(string Filename)
        {
            this.Filename = Filename;
            using (FileStream fs = new FileStream(Filename, FileMode.Open))
            {
                _size = (uint)fs.Length;
                _md5 = Utility.GetMd5(fs);
            }
        }

        public Stream GetStream()
        {
            return new FileStream(Filename, FileMode.Open, FileAccess.Read);
        }
    }
}
