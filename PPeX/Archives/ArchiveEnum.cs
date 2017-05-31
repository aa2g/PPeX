using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public enum ArchiveType : ushort
    {
        Archive = 1,
        Mod = 2,
        BGMPack = 3
    }

    public enum ArchiveFileType : byte
    {
        Raw = 0,
        Audio = 1,
        Image = 2
    }

    public enum ArchiveFileCompression : byte
    {
        Uncompressed = 0,
        LZ4 = 1,
        Zstandard = 2,

        Duplicate = 255
    }

    [Flags]
    public enum ArchiveFileFlags : byte
    {
        None = 0,
        Meta = 1,
        Duplicate = 2,
        MemoryCached = 4
    }
}
