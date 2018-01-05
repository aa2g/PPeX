using PPeX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class SubfileTextureBank : TextureBank
    {
        public Dictionary<string, ISubfile> TextureSubfiles { get; protected set; } = new Dictionary<string, ISubfile>();

        public override byte[] this[string name]
        {
            get => TextureSubfiles[name].GetRawStream().ToByteArray();
            set { }
        }

        public SubfileTextureBank(IEnumerable<ISubfile> subfiles)
        {
            foreach (ISubfile file in subfiles)
            {
                TextureSubfiles.Add(file.Name, file);
            }
        }
    }
}
