using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class XaEncoder : PassthroughEncoder
    {
        public override ArchiveDataType DataType => ArchiveDataType.Animation;

        public override ArchiveFileType Encoding => ArchiveFileType.XaAnimation;

        public XaEncoder(Stream baseStream) : base(baseStream)
        {

        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.xa";
        }
    }
}
