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
        protected TextureBank texBank;

        public Xx3Encoder(Stream source, TextureBank bank) : base(source)
        {
            texBank = bank;
        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx3Mesh;

        public override ArchiveDataType DataType => ArchiveDataType.Mesh;

        public override Stream Encode()
        {
            xxParser parser = new xxParser(BaseStream);
            Xx3File file = new Xx3File(parser, texBank);

            Xx3Writer writer = new Xx3Writer(Core.Settings.Xx2Precision);

            MemoryStream encodedXx3 = new MemoryStream();

            writer.Write(file, encodedXx3);

            encodedXx3.Position = 0;

            return encodedXx3;
        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.xx3";
        }

        public override Stream Decode()
        {
            Xx3File file = Xx3Reader.Read(BaseStream);

            MemoryStream mem = new MemoryStream();

            file.DecodeToXX(mem, texBank);

            mem.Position = 0;

            return mem;
        }
    }
}
