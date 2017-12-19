using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Xx2;

namespace PPeX.Encoders
{
    public class Xx4Encoder : BaseEncoder
    {
        protected TextureBank texBank;

        public Xx4Encoder(Stream source, TextureBank bank) : base(source)
        {
            texBank = bank;
        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx4Mesh;

        public override ArchiveDataType DataType => ArchiveDataType.Mesh;

        public override Stream Encode()
        {
            xxParser parser = new xxParser(BaseStream);
            Xx4File file = new Xx4File(parser, texBank);

            Xx4Writer writer = new Xx4Writer(Core.Settings.Xx2Precision);

            MemoryStream encodedXx4 = new MemoryStream();

            writer.Write(file, encodedXx4);

            encodedXx4.Position = 0;

            return encodedXx4;
        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.xx4";
        }

        public override Stream Decode()
        {
            Xx4File file = Xx4Reader.Read(BaseStream);

            MemoryStream mem = new MemoryStream();

            file.DecodeToXX(mem, texBank);

            mem.Position = 0;

            return mem;
        }
    }
}
