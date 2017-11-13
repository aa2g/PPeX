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
        Xx3Provider Provider;
        ArchiveFileSource BaseSource;

        public Xx3Subfile(ArchiveFileSource source, Xx3Provider provider)
        {
            BaseSource = source;
            Provider = provider;

            _size = source.Size;

            Xx3File file = Xx3Reader.Read(BaseSource.GetRawStream());
            
            foreach (var texRef in file.TextureRefs)
            {
                _size += provider.TextureFiles[texRef.Name].Size;
            }
        }

        public IDataSource Source => BaseSource;

        public string ArchiveName => BaseSource.ArchiveName;

        public string Name => BaseSource.Name.Replace(".xx3", ".xx");

        protected ulong _size;

        public ulong Size => _size;

        public ArchiveFileType Type => ArchiveFileType.Xx3Mesh;

        public Stream GetRawStream()
        {
            Xx3File file = Xx3Reader.Read(BaseSource.GetRawStream());

            TextureBank Bank = new TextureBank();

            foreach (var texRef in file.TextureRefs)
            {
                string name = texRef.Name;
                using (Stream stream = Provider.TextureFiles[name].GetRawStream())
                using (BinaryReader reader = new BinaryReader(stream))
                    Bank.Textures[name] = reader.ReadBytes((int)stream.Length);
            }

            MemoryStream mem = new MemoryStream();
            file.DecodeToXX(mem, Bank);

            mem.Position = 0;

            return mem;
        }
    }
}
