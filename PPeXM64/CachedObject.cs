using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXM64
{
    /// <summary>
    /// A cached version of a file to be kept in memory.
    /// </summary>
    public class CachedObject
    {
        public byte[] Data;
        public byte Priority;
        public byte[] MD5;
        public string Name;
    }
}
