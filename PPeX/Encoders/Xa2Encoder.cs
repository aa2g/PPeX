using PPeX.Xx2.Xa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public class Xa2Encoder : BaseEncoder
    {
        public Xa2Encoder(Stream source) : base(source)
        {

        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xa2Animation;

        public override ArchiveDataType DataType => ArchiveDataType.Animation;

        public override Stream Encode()
        {
            MemoryStream mem = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(mem, System.Text.Encoding.ASCII, true))
            using (BinaryReader reader = new BinaryReader(BaseStream, System.Text.Encoding.ASCII, true))
            {
                var xa = XaFile.FromReader(reader);

                Xa2Writer xa2 = new Xa2Writer();
                xa2.Write(writer, xa);

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
                throw new NotImplementedException();
                //var sviex = Sviex2Reader.FromReader(reader);

                //sviex.Write(writer);

                //mem.Position = 0;
                //return mem;
            }
        }

        public override string NameTransform(string original)
        {
            return $"{original.Substring(0, original.LastIndexOf('.'))}.xa2";
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
