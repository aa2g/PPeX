using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class Xx2Writer
    {
        public int Precision { get; set; }

        public bool TrimTextures { get; set; }

        public Xx2Writer()
        {
            Precision = 0;
            TrimTextures = false;
        }

        public Xx2Writer(int precision)
        {
            Precision = precision;
        }

        public void Write(Xx2File file, string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
                Write(file, stream);
        }

        public void Write(Xx2File file, Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(file.Version);


                writer.Write(file.HeaderUnknown.Length);

                writer.Write(file.HeaderUnknown);


                WriteObject(writer, file.RootObject);


                //always length of 4
                writer.Write(file.MaterialUnknown);


                writer.Write(file.Materials.Count);

                WriteMaterials(writer, file.Materials);


                if (TrimTextures)
                    file.Textures.Clear();

                writer.Write(file.Textures.Count);

                WriteTextures(writer, file.Textures);


                writer.Write(file.UnencodedData.Length);

                writer.Write(file.UnencodedData);
            }
        }

        public void WriteObject(BinaryWriter writer, xxObject obj)
        {
            writer.Write(obj.Name);

            writer.Write(obj.Unknowns.Count);

            foreach (var unknown in obj.Unknowns)
            {
                writer.Write((ushort)unknown.Length);
                writer.Write(unknown);
            }

            writer.Write(obj.Meshes.Count);

            foreach (var mesh in obj.Meshes)
                WriteMesh(writer, mesh);


            writer.Write(obj.DuplicateVerticies.Count);

            WriteVerticies(writer, obj.DuplicateVerticies);

            float[] transforms = new float[16];

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    transforms[(x * 4) + y] = obj.Transforms[x, y];


            writer.Write(FloatEncoder.Encode(transforms));

            writer.Write(obj.Bones.Count);

            WriteBones(writer, obj.Bones);

            writer.Write((short)obj.Children.Count);

            foreach (var child in obj.Children)
                WriteObject(writer, child);
        }


        public void WriteMesh(BinaryWriter writer, xxMeshInfo mesh)
        {
            writer.Write(mesh.Unknowns.Count);

            foreach (var unknown in mesh.Unknowns)
            {
                writer.Write((ushort)unknown.Length);
                writer.Write(unknown);
            }

            writer.Write(mesh.Verticies.Count);

            WriteVerticies(writer, mesh.Verticies);


            writer.Write(mesh.Faces.Count);

            for (int i = 0; i < 3; i++)
            {
                writer.Write(IntegerEncoder.Encode(mesh.Faces.Select(x => x.VertexIndicies[i]).ToArray()));
            }
        }

        public void WriteVerticies(BinaryWriter writer, List<xxVertex> verticies)
        {
            if (verticies.Count == 0)
                return;

            //encode index sizes
            for (int i = 0; i < verticies.Count; i++)
            {
                writer.Write(verticies[i].isIndexUInt16 ? (byte)1 : (byte)0);
            }

            //encode indicies
            writer.Write(IntegerEncoder.Encode(verticies.Select(x => (uint)x.Index).ToArray()));

            //encode each position collated
            for (int i = 0; i < 3; i++)
            {
                float[] data = verticies.Select(x => x.Position[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, 10f));
            }

            //encode each normal collated
            for (int i = 0; i < 3; i++)
            {
                float[] data = verticies.Select(x => x.Normal[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, 10f));
            }

            //encode each weight collated
            for (int i = 0; i < 3; i++)
            {
                float[] data = verticies.Select(x => x.Weights[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, 10f));
            }

            //encode each UV collated
            for (int i = 0; i < 2; i++)
            {
                float[] data = verticies.Select(x => x.UV[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, 10f));
            }

            //write bone indicies
            for (int i = 0; i < verticies.Count; i++)
            {
                writer.Write(verticies[i].BoneIndicies);
            }

            //write unknowns
            for (int i = 0; i < verticies.Count; i++)
            {
                writer.Write(verticies[i].Unknown);
            }
        }

        public void WriteBones(BinaryWriter writer, List<xxBone> bones)
        {
            if (bones.Count == 0)
                return;

            //encode indicies
            writer.Write(IntegerEncoder.Encode(bones.Select(x => (uint)x.Index).ToArray()));

            //encode each name
            for (int i = 0; i < bones.Count; i++)
            {
                /*
                byte[] data = Encoding.ASCII.GetBytes(bones[i].Name);

                writer.Write((byte)data.Length);
                writer.Write(data);
                */
                writer.Write(bones[i].Name);
            }

            //encode each transform
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    writer.Write(FloatEncoder.Encode(bones.Select(b => b.Transforms[x, y]).ToArray()));
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

        public void WriteTextures(BinaryWriter writer, List<xxTexture> textures)
        {
            if (textures.Count == 0)
                return;

            //time to get tricky

            List<Tuple<byte[], byte[]>> checksums = new List<Tuple<byte[], byte[]>>();

            List<int> indexedChecksums = new List<int>();

            for (int i = 0; i < textures.Count; i++)
            {
                byte[] checksum = Utility.GetMd5(textures[i].ImageData);

                if (!checksums.Any(x => Utility.CompareBytes(x.Item1, checksum)))
                    checksums.Add(new Tuple<byte[], byte[]>(checksum, textures[i].ImageData));

                //find index
                indexedChecksums.Add(checksums.FindIndex((x) => Utility.CompareBytes(x.Item1, checksum)));
            }


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

                //write the data index
                writer.Write(indexedChecksums[i]);
            }

            //write each individual texture data
            writer.Write(checksums.Count);

            for (int i = 0; i < checksums.Count; i++)
            {
                byte[] data = checksums[i].Item2;

                writer.Write(data.Length);

                writer.Write(data);
            }
        }
    }
}
