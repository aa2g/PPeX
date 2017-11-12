﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class IndexedTexture
    {
        public byte[] Checksum;
        public byte[] Data;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Checksum);

            writer.Write(Data.Length);
            writer.Write(Data);
        }

        public static IndexedTexture Read(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, true))
                return Read(reader);
        }

        public static IndexedTexture Read(BinaryReader reader)
        {
            IndexedTexture tex = new IndexedTexture();

            tex.Checksum = reader.ReadBytes(16);

            int length = reader.ReadInt32();
            tex.Data = reader.ReadBytes(length);

            return tex;
        }
    }

    public class TextureBank
    {
        public List<IndexedTexture> Textures { get; protected set; }

        public TextureBank()
        {
            Textures = new List<IndexedTexture>();
        }

        public int ProcessTexture(xxTexture texture)
        {
            byte[] checksum = Utility.GetMd5(texture.ImageData);

            int position = Textures.FindIndex(x => Utility.CompareBytes(x.Checksum, checksum));

            if (position < 0)
            {
                Textures.Add(new IndexedTexture
                {
                    Checksum = checksum,
                    Data = texture.ImageData
                });

                return Textures.Count - 1;
            }
            else
                return position;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Textures.Count);

            for (int i = 0; i < Textures.Count; i++)
            {
                Textures[i].Write(writer);
            }
        }
    }
}
