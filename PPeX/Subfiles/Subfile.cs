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

        protected bool OnlyReport;

        public Subfile(IDataSource source, string name, string archiveName, ArchiveFileType type, bool onlyReport = false) : base(source, name, archiveName)
        {
            Type = type;
            OnlyReport = onlyReport;

            using (IEncoder encoder = EncoderFactory.GetEncoder(Source, Type))
            {
                Name = encoder.NameTransform(name);
            }
        }

        public Subfile(IDataSource source, string name, string archiveName, bool onlyReport = false) : this(source, name, archiveName, ArchiveFileType.Raw, onlyReport)
        {
            if (name.EndsWith(".wav"))
                Type = ArchiveFileType.XggAudio;
            else
                Type = ArchiveFileType.Raw;

            using (IEncoder encoder = EncoderFactory.GetEncoder(Source, Type))
            {
                Name = encoder.NameTransform(name);
            }
        }

        public override void WriteToStream(Stream stream)
        {
            if (!OnlyReport)
                using (IEncoder encoder = EncoderFactory.GetEncoder(Source, Type))
                {
                    encoder.Encode().CopyTo(stream);
                    _size = encoder.EncodedLength;
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
