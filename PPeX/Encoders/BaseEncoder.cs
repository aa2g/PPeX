using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Encoders
{
    public abstract class BaseEncoder : IEncoder
    {
        public virtual IDataSource Source { get; protected set; }

        public BaseEncoder(IDataSource source)
        {
            Source = source;
        }

        public abstract ArchiveFileType Encoding { get; }

        public abstract uint EncodedLength { get; protected set; }

        public abstract Stream Encode();
        public abstract string NameTransform(string original);

        public virtual void Dispose()
        {
            Source.Dispose();
        }
    }


    public abstract class BaseDecoder : IDecoder
    {
        public virtual Stream BaseStream { get; protected set; }

        public BaseDecoder(Stream baseStream)
        {
            BaseStream = baseStream;
        }

        public byte[] Metadata { get; protected set; }

        public abstract ArchiveFileType Encoding { get; }

        public abstract Stream Decode();
        public abstract string NameTransform(string original);

        public virtual void Dispose()
        {
            BaseStream.Dispose();
        }
    }

    public static class EncoderFactory
    {
        public static IEncoder GetEncoder(this ISubfile subfile)
        {
            return GetEncoder(subfile.Source, subfile.Type);
        }

        public static IEncoder GetEncoder(IDataSource source, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.XggAudio:
                    return new XggEncoder(source, true);
                case ArchiveFileType.Xx2Mesh:
                    return new Xx2Encoder(source);
                case ArchiveFileType.Raw:
                    return new PassthroughEncoder(source.GetStream());
                default:
                    throw new InvalidOperationException("Encoding type is invalid.");
            }
        }

        public static IDecoder GetDecoder(Stream stream, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.XggAudio:
                    return new XggDecoder(stream);
                case ArchiveFileType.Xx2Mesh:
                    return new Xx2Decoder(stream);
                case ArchiveFileType.Raw:
                    return new PassthroughEncoder(stream);
                default:
                    throw new InvalidOperationException("Encoding type is invalid.");
            }
        }
    }
}
