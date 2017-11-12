using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PPeX.External.PP
{
    public interface IWriteFile
    {
        string Name { get; set; }
        void WriteTo(Stream stream);
    }
}
