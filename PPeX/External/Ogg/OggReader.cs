using System;
using System.IO;
using System.Text;

namespace PPeX.External.Ogg
{
    public class OggReader : IDisposable
    {
        protected BinaryReader reader;

        public int Channels { get; protected set; }

        public int Preskip { get; protected set; }

        public bool IsStreamFinished => reader.BaseStream.Position == reader.BaseStream.Length;

        public OggReader(Stream oggStream)
        {
            reader = new BinaryReader(oggStream);

            var headerPage = ReadPage();
            var metadataPage = ReadPage();

#warning Need to do this programatically
            Channels = headerPage.Data.Span[9];

            Preskip = BitConverter.ToUInt16(headerPage.Data.Span.Slice(10, 2));
        }

        public ReadOnlyMemory<byte> ReadPacket()
        {
            return ReadPage().Data;
        }

        public OggPage ReadPage()
        {
#warning This doesn't verify anything that it reads, needs checks
            
            string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic != OggPage.Magic)
                throw new InvalidDataException("This stream is either invalid or out of sync. Magic does not match.");

            byte version = reader.ReadByte();
            if (version != OggPage.OggVersion)
                throw new InvalidDataException("This stream is either invalid or out of sync. Ogg version does not match.");

            OggPageFlags flags = (OggPageFlags)reader.ReadByte();

            ulong granulePosition = reader.ReadUInt64();

            int serialNumber = reader.ReadInt32();

            int pageIndex = reader.ReadInt32();

            reader.ReadBytes(4); //checksum

            byte segmentCount = reader.ReadByte();

            int totalDataLength = 0;

#warning This doesn't take into account multi-packet pages
            for (int i = 0; i < segmentCount; i++)
                totalDataLength += reader.ReadByte();

            byte[] data = reader.ReadBytes(totalDataLength);

            return new OggPage(data, pageIndex, granulePosition, serialNumber, flags);
        }

        public void Dispose()
        {
            ((IDisposable)reader).Dispose();
        }
    }
}
