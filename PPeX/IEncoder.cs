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

        ArchiveDataType DataType { get; }

        Stream Encode();
        Stream Decode();
        
        string NameTransform(string original);
    }
}
