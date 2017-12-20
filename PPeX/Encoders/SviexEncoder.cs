using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class SviexEncoder : PassthroughEncoder
    {
        public override ArchiveDataType DataType => ArchiveDataType.Sviex;

        public override ArchiveFileType Encoding => ArchiveFileType.SviexMesh;

        public SviexEncoder(Stream baseStream) : base(baseStream)
        {

        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.sviex";
        }
    }
}
