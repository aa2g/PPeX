using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color4
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color4(BinaryReader reader)
        {
            R = reader.ReadSingle();
            G = reader.ReadSingle();
            B = reader.ReadSingle();
            A = reader.ReadSingle();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
            writer.Write(A);
        }
    }
    
    public class xxMaterialTexture
    {
        public string Name;
        public byte[] Unknown;

        public xxMaterialTexture(BinaryReader reader)
        {
            Name = reader.ReadEncryptedString();

            Unknown = reader.ReadBytes(16);
        }

        internal xxMaterialTexture()
        {

        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteEncryptedString(Name);

            writer.Write(Unknown);
        }
    }

    public class xxMaterial
    {
        public string Name;

        public Color4 Diffuse;

        public Color4 Ambient;

        public Color4 Specular;

        public Color4 Emissive;

        public float Power;

        public List<xxMaterialTexture> Textures = new List<xxMaterialTexture>();

        public byte[] Unknown;

        public xxMaterial(BinaryReader reader, int version)
        {
            Name = reader.ReadEncryptedString();

            Diffuse = new Color4(reader);
            Ambient = new Color4(reader);
            Specular = new Color4(reader);
            Emissive = new Color4(reader);

            Power = reader.ReadSingle();

            for (int i = 0; i < 4; i++)
                Textures.Add(new xxMaterialTexture(reader));

            if (version < 0)
                Unknown = reader.ReadBytes(4);
            else
                Unknown = reader.ReadBytes(88);
        }

        internal xxMaterial()
        {

        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteEncryptedString(Name);

            Diffuse.Write(writer);
            Ambient.Write(writer);
            Specular.Write(writer);
            Emissive.Write(writer);

            writer.Write(Power);

            for (int i = 0; i < Textures.Count; i++)
                Textures[i].Write(writer);

            writer.Write(Unknown);
        }
    }
}
