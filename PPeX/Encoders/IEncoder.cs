using System;
using System.IO;

namespace PPeX
{
    public interface IEncoder : IDisposable
    {
        ArchiveFileType Encoding { get; }

        ArchiveDataType DataType { get; }

        void Encode(Stream input, Stream output);
        void Decode(Stream input, Stream output);

        string RealNameTransform(string original);
    }
}