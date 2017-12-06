using System;
using System.IO;
using System.Text;

namespace PPeX.External.Ogg
{
    public class OggWrapper : IDisposable
    {
        protected static int SerialNumber = 666;
        protected static string VendorString => $"PPeX {Core.GetVersion().ToString()}";

        protected BinaryWriter dataWriter;
        protected int pageIndex = 2;
        protected ulong granulePosition = 0;

        public OggWrapper(Stream outputStream, int channels, bool keepOpen = false)
        {
            dataWriter = new BinaryWriter(outputStream, Encoding.ASCII, keepOpen);

            WritePage(GetOpusHeaderPage(channels));

            WritePage(GetOpusCommentPage());
        }

        public void WritePage(OggPage page)
        {
            dataWriter.Write(page.Header);
            dataWriter.Write(page.Data);
        }

        public void WritePacket(byte[] packet, int samples, bool isLast)
        {
            WritePage(CreatePage(packet, samples, isLast));
        }

        protected OggPage CreatePage(byte[] packet, int samples, bool isLast)
        {
            granulePosition += (ulong)samples;

            OggPageFlags flags = OggPageFlags.None;

            if (isLast)
                flags |= OggPageFlags.EndOfStream;

            return new OggPage(packet, pageIndex++, granulePosition, SerialNumber, flags);
        }

        protected static OggPage GetOpusCommentPage()
        {
            const string CommentMagic = "OpusTags";

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                writer.Write(Encoding.ASCII.GetBytes(CommentMagic));

                //vendor string
                byte[] vendorString = Encoding.UTF8.GetBytes(VendorString);

                writer.Write((int)vendorString.Length);
                writer.Write(vendorString);

                //no actual metadata
                writer.Write((int)0);

                return new OggPage(mem.ToArray(), 1, 0, SerialNumber);
            }
        }

        protected static OggPage GetOpusHeaderPage(int channels)
        {
            const string OpusMagic = "OpusHead";
            const byte OpusVersion = 1;

            using (MemoryStream mem = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                writer.Write(Encoding.ASCII.GetBytes(OpusMagic));
                writer.Write(OpusVersion);

                writer.Write((byte)channels);

                //pre-skip
                writer.Write((ushort)0);

                //input sample rate
                writer.Write((uint)48000);

                //output gain
                writer.Write((short)0);

                //channel map
                writer.Write((byte)0);

                return new OggPage(mem.ToArray(), 0, 0, SerialNumber, OggPageFlags.BeginningOfStream);
            }
        }

        public void Dispose()
        {
            ((IDisposable)dataWriter).Dispose();
        }
    }
}
