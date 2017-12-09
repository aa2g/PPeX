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

        public Xx2Encoder(Stream source) : base(source)
        {
            
        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx2Mesh;

        public override ArchiveDataType DataType => ArchiveDataType.Mesh;

        public override Stream Encode()
        {
            xxParser parser = new xxParser(BaseStream);
            Xx2File file = new Xx2File(parser);
            Xx2Writer writer;

            if (Core.Settings.Xx2IsUsingQuality)
                writer = new Xx2Writer(Core.Settings.Xx2Quality);
            else
                writer = new Xx2Writer(Core.Settings.Xx2Precision);


            encodedXx2 = new MemoryStream();

            writer.Write(file, encodedXx2);

            encodedXx2.Position = 0;

            return encodedXx2;
        }

        public override Stream Decode()
        {
            Xx2File file = Xx2Reader.Read(BaseStream);

            MemoryStream mem = new MemoryStream();
            file.DecodeToXX(mem);

            mem.Position = 0;

            return mem;
        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.xx2";
        }

        public override void Dispose()
        {
            if (encodedXx2 != null)
                encodedXx2.Dispose();
            base.Dispose();
        }
    }
}
