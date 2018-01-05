using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Xx2.Sviex;

namespace PPeX.Encoders
{
    public class Sviex2Encoder : BaseEncoder
    {
        public Sviex2Encoder(Stream source) : base(source)
        {

        }

        public override ArchiveFileType Encoding => ArchiveFileType.Sviex2Mesh;

        public override ArchiveDataType DataType => ArchiveDataType.Sviex;

        public override Stream Encode()
        {
            MemoryStream mem = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(mem, System.Text.Encoding.ASCII, true))
            using (BinaryReader reader = new BinaryReader(BaseStream, System.Text.Encoding.ASCII, true))
            {
                var sviex = SviexFile.FromReader(reader);

                Sviex2Writer sviex2 = new Sviex2Writer();
                sviex2.Write(writer, sviex);

                mem.Position = 0;
                return mem;
            }
        }

        public override Stream Decode()
        {
            MemoryStream mem = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(mem, System.Text.Encoding.ASCII, true))
            using (BinaryReader reader = new BinaryReader(BaseStream, System.Text.Encoding.ASCII, true))
            {
                var sviex = Sviex2Reader.FromReader(reader);

                sviex.Write(writer);

                mem.Position = 0;
                return mem;
            }
        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.sviex2";
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
