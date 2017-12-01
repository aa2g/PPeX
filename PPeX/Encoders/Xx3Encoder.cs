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

        MemoryStream encodedXx3;

        public Xx3Encoder(IDataSource source, TextureBank bank) : base(source)
        {
            texBank = bank;
        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx3Mesh;

        public override uint EncodedLength { get; protected set; }

        public override Stream Encode()
        {
            Stream readStream = Source.GetStream();

#if WINE
            MemoryStream mem = new MemoryStream();
            readStream.CopyTo(mem);
            readStream.Dispose();
            mem.Position = 0;
            readStream = mem;
#endif

            xxParser parser = new xxParser(readStream);
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

    public class Xx3Decoder : BaseDecoder
    {
        protected TextureBank texBank;
        protected Xx3Provider provider;

        public Xx3Decoder(Stream baseStream, Xx3Provider provider) : base(baseStream)
        {
            this.provider = provider;
        }

        public Xx3Decoder(Stream baseStream, TextureBank bank) : base(baseStream)
        {
            texBank = bank;
        }

        public override ArchiveFileType Encoding => ArchiveFileType.Xx3Mesh;

        public override Stream Decode()
        {
            Xx3File file = Xx3Reader.Read(BaseStream);

            MemoryStream mem = new MemoryStream();

            if (texBank == null)
            {
                texBank = new TextureBank();

                foreach (var texRef in file.TextureRefs)
                {
                    string name = texRef.Name;
                    using (Stream stream = provider.TextureFiles[name].GetRawStream())
                    using (BinaryReader reader = new BinaryReader(stream))
                        texBank[name] = reader.ReadBytes((int)stream.Length);
                }
            }

            file.DecodeToXX(mem, texBank);

            mem.Position = 0;

            return mem;
        }

        public Stream DecodeBlankTextures()
        {
            Xx3File file = Xx3Reader.Read(BaseStream);

            MemoryStream mem = new MemoryStream();

            file.DecodeToBlankXX(mem);

            mem.Position = 0;

            return mem;
        }

        public override string NameTransform(string original)
        {
            return original.Replace(".xx3", ".xx");
        }
    }
}
