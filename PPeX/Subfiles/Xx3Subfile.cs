using PPeX.Xx2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class Xx3Subfile : ISubfile
    {
        SubfileTextureBank Provider;
        ArchiveFileSource BaseSource;

        public Xx3Subfile(ArchiveFileSource source, SubfileTextureBank provider)
        {
            BaseSource = source;
            Provider = provider;

            _size = source.Size;

            Xx3File file = Xx3Reader.Read(BaseSource.GetStream());
            
            foreach (var texRef in file.TextureRefs)
            {
                _size += provider.TextureSubfiles[texRef.Name].Size;
            }
        }

        public IDataSource Source => BaseSource;

        public string ArchiveName => BaseSource.ArchiveName;

        public string Name => BaseSource.Name;

        public string EmulatedArchiveName => BaseSource.EmulatedArchiveName;

        public string EmulatedName => BaseSource.EmulatedName;

        protected ulong _size;

        public ulong Size => _size;

        public ArchiveFileType Type => ArchiveFileType.Xx3Mesh;

        public Stream GetRawStream()
        {
            Xx3File file = Xx3Reader.Read(BaseSource.GetStream());

            TextureBank Bank = new TextureBank();

            foreach (var texRef in file.TextureRefs)
            {
                string name = texRef.Name;
                using (Stream stream = Provider.TextureSubfiles[name].GetRawStream())
                using (BinaryReader reader = new BinaryReader(stream))
                    Bank[name] = reader.ReadBytes((int)stream.Length);
            }

            MemoryStream mem = new MemoryStream();
            file.DecodeToXX(mem, Bank);

            mem.Position = 0;

            return mem;
        }
    }
}
