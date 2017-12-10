using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class XxEncoder : PassthroughEncoder
    {
        public override ArchiveDataType DataType => ArchiveDataType.Mesh;

        public override ArchiveFileType Encoding => ArchiveFileType.XxMesh;

        public XxEncoder(Stream baseStream) : base(baseStream)
        {

        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.xx";
        }
    }
}
