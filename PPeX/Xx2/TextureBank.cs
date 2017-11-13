using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class TextureBank
    {
        public Dictionary<string, byte[]> Textures { get; protected set; }

        public TextureBank()
        {
            Textures = new Dictionary<string, byte[]>();
        }

        public void ProcessTexture(xxTexture texture)
        {
            if (!Textures.ContainsKey(texture.Name))
                Textures.Add(texture.Name, texture.ImageData);
        }
    }
}
