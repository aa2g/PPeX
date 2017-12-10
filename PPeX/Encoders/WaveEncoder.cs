using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class WaveEncoder : PassthroughEncoder
    {
        public override ArchiveDataType DataType => ArchiveDataType.Audio;
        public override ArchiveFileType Encoding => ArchiveFileType.WaveAudio;

        public WaveEncoder(Stream baseStream) : base(baseStream)
        {

        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.wav";
        }
    }
}
