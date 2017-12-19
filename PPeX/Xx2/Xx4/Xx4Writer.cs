using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class Xx4Writer
    {
        public int Precision { get; set; }

        public Xx4Writer() : this(0)
        {
            
        }

        public Xx4Writer(int precision)
        {
            Precision = precision;
        }

        public void Write(Xx4File file, string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
                Write(file, stream);
        }

        protected int version;

        public void Write(Xx4File file, Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(file.Version);
                version = file.Version;

                writer.Write(file.HeaderUnknown.Length);

                writer.Write(file.HeaderUnknown);

                using (MemoryStream mem = new MemoryStream())
                using (BinaryWriter tempwriter = new BinaryWriter(mem))
                {
                    WriteObject(tempwriter, file.RootObject);
                    mem.Position = 0;

                    writer.Write((int)mem.Length);
                    mem.CopyTo(stream);
                }

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

            //encode each position
            foreach (float f in verticies.SelectMany(x => x.Position))
            {
                writer.Write(BitConverter.GetBytes(f));
            }

            //encode each normal
            foreach (float f in verticies.SelectMany(x => x.Normal))
            {
                writer.Write(BitConverter.GetBytes(f));
            }

            //encode each weight
            foreach (float f in verticies.SelectMany(x => x.Weights))
            {
                writer.Write(BitConverter.GetBytes(f));
            }

            //encode each UV
            foreach (float f in verticies.SelectMany(x => x.UV))
            {
                writer.Write(BitConverter.GetBytes(f));
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
