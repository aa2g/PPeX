using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Xx2;

namespace PPeX.Encoders
{
    public class Xx3Encoder : BaseEncoder
    {
        public static TextureBank texBank = new TextureBank();

        MemoryStream encodedXx3;

        public Xx3Encoder(IDataSource source) : base(source)
        {

        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx2Mesh;

        public override uint EncodedLength { get; protected set; }

        public override Stream Encode()
        {
            xxParser parser = new xxParser(Source.GetStream());
            Xx3File file = new Xx3File(parser, texBank);

            Xx3Writer writer = new Xx3Writer(Core.Settings.Xx2Precision);

            encodedXx3 = new MemoryStream();

            writer.Write(file, encodedXx3);

            EncodedLength = (uint)encodedXx3.Length;

            encodedXx3.Position = 0;

            return encodedXx3;
        }

        public override string NameTransform(string original)
        {
            return original.Replace(".xx", ".xx3");
        }

        public override void Dispose()
        {
            if (encodedXx3 != null)
                encodedXx3.Dispose();
            base.Dispose();
        }
    }
}
