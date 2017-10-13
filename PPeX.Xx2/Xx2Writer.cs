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

        public Xx2Writer()
        {
            Precision = 0;
        }

        public Xx2Writer(int precision)
        {
            Precision = precision;
        }


        public void Write(Xx2File file, string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(file.Version);


                writer.Write(file.Unknown.Length);

                writer.Write(file.Unknown);


                WriteObject(writer, file.RootObject);


                writer.Write(file.UnencodedData.Length);

                writer.Write(file.UnencodedData);
            }
        }

        protected void WriteObject(BinaryWriter writer, xxObject obj)
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
                for (int y = 0; x < 4; x++)
                    transforms[(x * 4) + y] = obj.Transforms[x, y];


            writer.Write(FloatEncoder.Encode(transforms));

            writer.Write(obj.Bones.Count);

            WriteBones(writer, obj.Bones);

            writer.Write((short)obj.Children.Count);

            foreach (var child in obj.Children)
                WriteObject(writer, child);
        }


        protected void WriteMesh(BinaryWriter writer, xxMeshInfo mesh)
        {
            writer.Write(mesh.Unknowns.Count);

            foreach (var unknown in mesh.Unknowns)
            {
                writer.Write((ushort)unknown.Length);
                writer.Write(unknown);
            }

            writer.Write(mesh.Verticies.Count);

            WriteVerticies(writer, mesh.Verticies);


            writer.Write(mesh.Faces.Length);

            writer.Write(IntegerEncoder.Encode(mesh.Faces));
        }

        protected void WriteVerticies(BinaryWriter writer, List<xxVertex> verticies)
        {
            if (verticies.Count == 0)
                return;

            //encode indicies
            writer.Write(IntegerEncoder.Encode(verticies.Select(x => x.Index).ToArray()));

            //encode each position collated
            for (int i = 0; i < 3; i++)
            {
                float[] data = verticies.Select(x => x.Position[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, Precision));
            }

            //encode each normal collated
            for (int i = 0; i < 3; i++)
            {
                float[] data = verticies.Select(x => x.Normal[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, Precision));
            }

            //encode each weight collated
            for (int i = 0; i < 3; i++)
            {
                float[] data = verticies.Select(x => x.Weights[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, Precision));
            }

            //encode each UV collated
            for (int i = 0; i < 2; i++)
            {
                float[] data = verticies.Select(x => x.UV[i]).ToArray();

                writer.Write(FloatEncoder.Encode(data, Precision));
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

        protected void WriteBones(BinaryWriter writer, List<xxBone> bones)
        {
            if (bones.Count == 0)
                return;

            //encode indicies
            writer.Write(IntegerEncoder.Encode(bones.Select(x => x.Index).ToArray()));

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
                for (int y = 0; x < 4; x++)
                {
                    writer.Write(FloatEncoder.Encode(bones.Select(b => b.Transforms[x, y]).ToArray()));
                }
        }
    }
}
