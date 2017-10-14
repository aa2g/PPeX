using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Xx2;

namespace PPeX.Encoders
{
    public class Xx2Encoder : BaseEncoder
    {
        MemoryStream encodedXx2;

        public Xx2Encoder(IDataSource source) : base(source)
        {
            
        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx2Mesh;

        public override uint EncodedLength { get; protected set; }

        public override Stream Encode()
        {
            xxParser parser = new xxParser(Source.GetStream());
            Xx2File file = new Xx2File(parser);

            Xx2Writer writer = new Xx2Writer(Core.Settings.Xx2Precision);

            encodedXx2 = new MemoryStream();

            writer.Write(file, encodedXx2);

            EncodedLength = (uint)encodedXx2.Length;

            return encodedXx2;
        }

        public override string NameTransform(string original)
        {
            return original.Replace(".xx", ".xx2");
        }

        public override void Dispose()
        {
            if (encodedXx2 != null)
                encodedXx2.Dispose();
            base.Dispose();
        }
    }
}
