using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SB3Utility;

namespace PPeX
{
    public class PPSource : IDataSource
    {
        protected IReadFile subfile;

        public PPSource(IReadFile subfile)
        {
            this.subfile = subfile;

            using (Stream stream = GetStream())
            {
                _md5 = Utility.GetMd5(stream);

                _size = (uint)stream.Position;
            }
        }

        protected byte[] _md5;
        public byte[] Md5 => _md5;

        protected uint _size;
        public uint Size => _size;

        public Stream GetStream()
        {
            return subfile.CreateReadStream();
        }
    }
}
