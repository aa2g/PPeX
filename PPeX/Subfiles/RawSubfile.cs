using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class RawSubfile : BaseSubfile
    {
        public override uint Size => Source.Size;

        public RawSubfile(IDataSource Source, string Name, string Archive) : base(Source, Name, Archive)
        {
            
        }

        public override void WriteToStream(Stream stream)
        {
            using (Stream source = Source.GetStream())
            {
                source.CopyTo(stream);
            }
        }
    }
}
