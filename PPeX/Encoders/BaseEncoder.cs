﻿using System;
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
        public static IEncoder GetEncoder(this ISubfile subfile, IArchiveWriter writer)
        {
            return GetEncoder(subfile.Source, writer, subfile.Type);
        }

        public static IEncoder GetEncoder(IDataSource source, IArchiveWriter writer, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.OpusAudio:
                    return new OpusEncoder(source, true);
                case ArchiveFileType.Xx2Mesh:
                    return new Xx2Encoder(source);
                case ArchiveFileType.Xx3Mesh:
                    return new Xx3Encoder(source, writer.TextureBank);
                case ArchiveFileType.Raw:
                    return new PassthroughEncoder(source.GetStream());
                default:
                    throw new InvalidOperationException("Encoding type is invalid.");
            }
        }

        public static IDecoder GetDecoder(Stream stream, ExtendedArchive archive, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.OpusAudio:
                    return new OpusDecoder(stream);
                case ArchiveFileType.Xx2Mesh:
                    return new Xx2Decoder(stream);
                case ArchiveFileType.Xx3Mesh:
                    return new Xx3Decoder(stream, archive.Xx3Provider);
                case ArchiveFileType.Raw:
                    return new PassthroughEncoder(stream);
                default:
                    throw new InvalidOperationException("Encoding type is invalid.");
            }
        }

        public static IEncoder GetGenericEncoder(IDataSource source, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.OpusAudio:
                    return new OpusEncoder(source, true);
                case ArchiveFileType.Xx2Mesh:
                    return new Xx2Encoder(source);
                case ArchiveFileType.Raw:
                    return new PassthroughEncoder(source.GetStream());
                default:
                    throw new InvalidOperationException("Encoding type is invalid.");
            }
        }

        public static IDecoder GetGenericDecoder(Stream stream, ArchiveFileType encoding)
        {
            switch (encoding)
            {
                case ArchiveFileType.OpusAudio:
                    return new OpusDecoder(stream);
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
