using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPeX.Xx2;

namespace PPeX
{
    public class Xx3Provider
    {
        protected Dictionary<string, ISubfile> textureSubfiles = new Dictionary<string, ISubfile>();

        public IReadOnlyDictionary<string, ISubfile> TextureFiles => textureSubfiles;

        public List<ISubfile> XX3Subfiles = new List<ISubfile>();

        public Xx3Provider(IList<ExtendedArchiveChunk> Chunks)
        {
            List<ExtendedArchiveChunk> xxChunks = Chunks.Where(x => x.Type == ChunkType.Xx3).ToList();

            foreach (var textureFile in xxChunks.SelectMany(x => x.Files).Where(x => x.ArchiveName == "_TextureBank"))
            {
                textureSubfiles.Add(textureFile.Name, textureFile);
            }

            
            foreach (var xx3File in xxChunks.SelectMany(x => x.Files).Where(x => x.Name.EndsWith(".xx3")))
            {
                //Xx3Subfile file = new Xx3Subfile(xx3File.Source as ArchiveFileSource, this);

                //XX3Subfiles.Add(file);

                XX3Subfiles.Add(xx3File);
            }
        }
    }
}
