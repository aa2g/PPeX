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
        public virtual Stream BaseStream { get; protected set; }

        public BaseEncoder(Stream baseStream)
        {
            BaseStream = baseStream;
        }

        public abstract ArchiveFileType Encoding { get; }
        public abstract ArchiveDataType DataType { get; }
        public abstract Stream Encode();
        public abstract Stream Decode();
        public abstract string NameTransform(string original);

        public virtual void Dispose()
        {
            BaseStream.Dispose();
        }
    }

    public static class EncoderFactory
    {
        public static IEncoder GetEncoder(this ISubfile subfile, IArchiveContainer container)
        {
            return GetEncoder(subfile.GetRawStream(), container, subfile.Type);
        }
        public static IEncoder GetEncoder(this ISubfile subfile)
        {
            return GetGenericEncoder(subfile.GetRawStream(), subfile.Type);
        }

        public static IEncoder GetEncoder(Stream source, IArchiveContainer writer, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.Xx3Mesh:
                    return new Xx3Encoder(source, writer.TextureBank);
                default:
                    return GetGenericEncoder(source, encoding);
            }
        }

        public static IEncoder GetGenericEncoder(Stream source, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.WaveAudio:
                    return new WaveEncoder(source);
                case ArchiveFileType.OpusAudio:
                    return new OpusEncoder(source);
                case ArchiveFileType.XxMesh:
                    return new XxEncoder(source);
                case ArchiveFileType.Xx2Mesh:
                    return new Xx2Encoder(source);
                case ArchiveFileType.Raw:
                    return new PassthroughEncoder(source);
                default:
                    throw new InvalidOperationException("Encoding type is invalid.");
            }
        }

        public static string TransformName(this string original, ArchiveFileType type)
        {
            using (var encoder = GetEncoder(null, null, type))
            {
                return encoder.NameTransform(original);
            }
        }
    }
}
