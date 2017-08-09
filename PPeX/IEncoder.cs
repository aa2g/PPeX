using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public interface IEncoder : IDisposable
    {
        ArchiveFileType Encoding { get; }

        Stream Encode();
        uint EncodedLength { get; }

        string NameTransform(string original);
    }

    public interface IDecoder : IDisposable
    {
        ArchiveFileType Encoding { get; }

        Stream Decode();

        string NameTransform(string modified);
    }
}
