using PPeX.Compressors;
using PPeX.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class Subfile : BaseSubfile
    {
        protected uint _size;
        public override uint Size => _size;

        public ArchiveFileCompression Compression { get; protected set; }
        public ArchiveFileEncoding Encoding { get; protected set; }

        protected bool OnlyReport;

        public Subfile(IDataSource source, string name, string archiveName, ArchiveFileCompression compression, ArchiveFileEncoding encoding, bool onlyReport = false) : base(source, name, archiveName)
        {
            Compression = compression;
            Encoding = encoding;
            OnlyReport = onlyReport;
        }

        public override void WriteToStream(Stream stream)
        {
            if (!OnlyReport)
                using (IEncoder encoder = EncoderFactory.GetEncoder(Source, Encoding))
                using (ICompressor compressor = CompressorFactory.GetCompressor(encoder.Encode(), Compression))
                {
                    compressor.WriteToStream(stream);
                    _size = compressor.CompressedSize;
                }
            else
                using (Stream sourceStream = Source.GetStream())
                {
                    sourceStream.CopyTo(stream);
                    _size = (uint)sourceStream.Length;
                }
                    
        }
    }
}
