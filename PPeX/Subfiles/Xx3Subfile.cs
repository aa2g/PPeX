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
                _size += provider.TextureFiles[texRef.Reference].Size;
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
            //fill up space so we can index properly
            for (int i = 0; i < Provider.TextureFiles.Count; i++)
                Bank.Textures.Add(null);

            foreach (var texRef in file.TextureRefs)
            {
                int index = texRef.Reference;
                using (Stream stream = Provider.TextureFiles[index].GetRawStream())
                    Bank.Textures[index] = IndexedTexture.Read(stream);
            }

            MemoryStream mem = new MemoryStream();
            file.DecodeToXX(mem, Bank);

            mem.Position = 0;

            return mem;
        }
    }
}
