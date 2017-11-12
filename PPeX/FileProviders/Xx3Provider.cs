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
        protected List<ISubfile> textureSubfiles = new List<ISubfile>();

        public IReadOnlyList<ISubfile> TextureFiles => textureSubfiles.AsReadOnly();

        public List<ISubfile> XX3Subfiles = new List<ISubfile>();

        public Xx3Provider(IList<ExtendedArchiveChunk> Chunks)
        {
            List<ExtendedArchiveChunk> xxChunks = Chunks.Where(x => x.Type == ChunkType.Xx3).ToList();

            foreach (var textureFile in xxChunks.SelectMany(x => x.Files).Where(x => x.ArchiveName == "_xx3_TextureBank").OrderBy(x => int.Parse(x.Name.Remove(x.Name.Length - 4))))
            {
                textureSubfiles.Add(textureFile);
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
