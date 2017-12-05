using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class TextureBank : IEnumerable<KeyValuePair<string, byte[]>>
    {
        protected Dictionary<string, byte[]> Textures { get; set; }

        public TextureBank()
        {
            Textures = new Dictionary<string, byte[]>();
        }

        public int Count => Textures.Count;

        protected virtual byte[] GetTexture(string name)
        {
            return Textures[name];
        }

        public virtual byte[] this[string name]
        {
            get { return Textures[name]; }
            set { Textures[name] = value; }
        }

        public virtual void ProcessTexture(xxTexture texture)
        {
            if (!Textures.ContainsKey(texture.Name))
                this[texture.Name] = texture.ImageData;
        }

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
        {
            return Textures.Select(x => new KeyValuePair<string, byte[]>(x.Key, this[x.Key])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
