using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class Xx3Writer
    {
        public int Precision { get; set; }

        public Xx3Writer() : this(0)
        {
            
        }

        public Xx3Writer(int precision)
        {
            Precision = precision;
        }

        public void Write(Xx3File file, string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
                Write(file, stream);
        }

        protected int version;

        public void Write(Xx3File file, Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(file.Version);
                version = file.Version;

                writer.Write(file.HeaderUnknown.Length);

                writer.Write(file.HeaderUnknown);


                writer.Write(file.RootObject.Length);

                writer.Write(file.RootObject);


                //always length of 4
                writer.Write(file.MaterialUnknown);


                writer.Write(file.Materials.Count);

                foreach (var mat in file.Materials)
                    mat.Write(writer);

                //WriteMaterials(writer, file.Materials);


                writer.Write(file.TextureRefs.Count);

                WriteTextureRefs(writer, file.TextureRefs);


                writer.Write(file.UnencodedData.Length);

                writer.Write(file.UnencodedData);
            }
        }

        public void WriteMaterials(BinaryWriter writer, List<xxMaterial> materials)
        {
            if (materials.Count == 0)
                return;

            //we can't really strategically encode much here
            int unknownSize = materials[0].Unknown.Length;

            //encode universal unknown size
            writer.Write(unknownSize);
            
            for (int i = 0; i < materials.Count; i++)
            {
                //write name
                writer.Write(materials[i].Name);

                //write color sets
                materials[i].Ambient.Write(writer);
                materials[i].Diffuse.Write(writer);
                materials[i].Emissive.Write(writer);
                materials[i].Specular.Write(writer);

                //write power
                writer.Write(materials[i].Power);

                //write material textures
                //*always* in groups of 4
                for (int x = 0; x < 4; x++)
                {
                    var materialTexture = materials[i].Textures[x];

                    writer.Write(materialTexture.Name);

                    //always a size of 16
                    writer.Write(materialTexture.Unknown);
                }

                //write universally sized unknown
                writer.Write(materials[i].Unknown);
            }
        }

        public void WriteTextureRefs(BinaryWriter writer, List<xxTextureReference> textures)
        {
            if (textures.Count == 0)
                return;
            
            //write each texture metadata
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                writer.Write(texture.Name);

                writer.Write(texture.Checksum);
                writer.Write(texture.Depth);
                writer.Write(texture.Format);
                writer.Write(texture.Height);
                writer.Write(texture.ImageFileFormat);
                writer.Write(texture.MipLevels);
                writer.Write(texture.ResourceType);
                writer.Write(texture.Width);

                //unknown is always 4 bytes long
                writer.Write(texture.Unknown);
            }
        }
    }
}
