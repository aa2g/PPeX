using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX.Xx2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXTests
{
    public static class xxCommon
    {
        public static void VerifyFaces(IList<xxFace> original, IList<xxFace> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Face count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].VertexIndicies.Length, saved[i].VertexIndicies.Length, "Face vertex count does not match.");

                for (int x = 0; x < original[i].VertexIndicies.Length; x++)
                    Assert.AreEqual(original[i].VertexIndicies[x], saved[i].VertexIndicies[x], "Face verticies do not match.");
            }
        }

        public static void VerifyBones(IList<xxBone> original, IList<xxBone> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Bone count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].Name, saved[i].Name, "Bone name does not match.");

                Assert.AreEqual(original[i].Index, saved[i].Index, "Bone index does not match.");

                for (int x = 0; x < 4; x++)
                    for (int y = 0; y < 4; y++)
                        Assert.AreEqual(original[i].Transforms[x, y], saved[i].Transforms[x, y], "Bone transforms do not match.");
            }
        }

        public static void VerifyUnknowns(IList<byte[]> original, IList<byte[]> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Unknown count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].Length, saved[i].Length, "Unknown length does not match.");

                for (int x = 0; x < original[i].Length; x++)
                    Assert.AreEqual(original[i][x], saved[i][x], "Unknown does not match.");
            }
        }

        public static void VerifyVerticies(IList<xxVertex> original, IList<xxVertex> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Vertex count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].isIndexUInt16, saved[i].isIndexUInt16, "Vertex index size does not match.");

                Assert.AreEqual(original[i].Index, saved[i].Index, "Vertex index does not match.");

                //verify positions
                Assert.AreEqual(original[i].Position.Length, saved[i].Position.Length, "Vertex position count does not match.");

                for (int x = 0; x < original[i].Position.Length; x++)
                    Assert.AreEqual(original[i].Position[x], saved[i].Position[x], "Vertex positions do not match.");

                //verify normals
                Assert.AreEqual(original[i].Normal.Length, saved[i].Normal.Length, "Vertex normal count does not match.");

                for (int x = 0; x < original[i].Normal.Length; x++)
                    Assert.AreEqual(original[i].Normal[x], saved[i].Normal[x], "Vertex normals do not match.");

                //verify weights
                Assert.AreEqual(original[i].Weights.Length, saved[i].Weights.Length, "Vertex weight count does not match.");

                for (int x = 0; x < original[i].Weights.Length; x++)
                    Assert.AreEqual(original[i].Weights[x], saved[i].Weights[x], "Vertex weights do not match.");

                //verify UVs
                Assert.AreEqual(original[i].UV.Length, saved[i].UV.Length, "Vertex UV count does not match.");

                for (int x = 0; x < original[i].UV.Length; x++)
                    Assert.AreEqual(original[i].UV[x], saved[i].UV[x], "Vertex UVs do not match.");

                //verify bone indicies
                Assert.AreEqual(original[i].BoneIndicies.Length, saved[i].BoneIndicies.Length, "Vertex bone index count does not match.");

                for (int x = 0; x < original[i].BoneIndicies.Length; x++)
                    Assert.AreEqual(original[i].BoneIndicies[x], saved[i].BoneIndicies[x], "Vertex bone indicies do not match.");

                //verify unknown
                Assert.AreEqual(original[i].Unknown.Length, saved[i].Unknown.Length, "Vertex unknown length does not match.");

                for (int x = 0; x < original[i].Unknown.Length; x++)
                    Assert.AreEqual(original[i].Unknown[x], saved[i].Unknown[x], "Vertex unknown does not match.");
            }
        }

        public static void VerifyMesh(xxMeshInfo original, xxMeshInfo saved)
        {
            //verify unknowns
            VerifyUnknowns(original.Unknowns, saved.Unknowns);

            //verify faces
            VerifyFaces(original.Faces, saved.Faces);

            //verify verticies
            VerifyVerticies(original.Verticies, saved.Verticies);
        }

        public static void VerifyObject(xxObject original, xxObject saved)
        {
            //verify unknowns
            VerifyUnknowns(original.Unknowns, saved.Unknowns);

            //verify name
            Assert.AreEqual(original.Name, saved.Name, "Object name does not match.");

            //verify bones
            VerifyBones(original.Bones, saved.Bones);

            //verify transforms
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    Assert.AreEqual(original.Transforms[x, y], saved.Transforms[x, y], "Object transforms do not match.");

            //verify meshes
            Assert.AreEqual(original.Meshes.Count, saved.Meshes.Count, "Object mesh count does not match.");

            for (int i = 0; i < original.Meshes.Count; i++)
            {
                VerifyMesh(original.Meshes[i], saved.Meshes[i]);
            }

            //verify duplicate verticies
            VerifyVerticies(original.DuplicateVerticies, saved.DuplicateVerticies);

            //verify children
            Assert.AreEqual(original.Meshes.Count, saved.Meshes.Count, "Object child count does not match.");

            for (int i = 0; i < original.Meshes.Count; i++)
            {
                VerifyObject(original.Children[i], saved.Children[i]);
            }
        }

        public static void VerifyFile(Xx2File original, Xx2File saved)
        {
            //verify version
            Assert.AreEqual(original.Version, saved.Version, "Version does not match.");

            //verify unknown
            Assert.AreEqual(original.HeaderUnknown.Length, saved.HeaderUnknown.Length, "Header unknown length does not match.");

            for (int x = 0; x < original.HeaderUnknown.Length; x++)
                Assert.AreEqual(original.HeaderUnknown[x], saved.HeaderUnknown[x], "Header unknown does not match.");

            //verify object
            VerifyObject(original.RootObject, saved.RootObject);

            //verify unencoded data
            Assert.AreEqual(original.UnencodedData.Length, saved.UnencodedData.Length, "Unencoded data length does not match.");

            for (int x = 0; x < original.UnencodedData.Length; x++)
                Assert.AreEqual(original.UnencodedData[x], saved.UnencodedData[x], "Unencoded data does not match.");
        }

        public static void VerifyMaterialTextures(IList<xxMaterialTexture> original, IList<xxMaterialTexture> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Material texture count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].Name, saved[i].Name, "Material texture name does not match.");


                Assert.AreEqual(original[i].Unknown.Length, saved[i].Unknown.Length, "Material texture unknown length does not match.");

                for (int x = 0; x < original[i].Unknown.Length; x++)
                    Assert.AreEqual(original[i].Unknown[x], saved[i].Unknown[x], "Material texture unknown does not match.");
            }
        }

        public static void VerifyMaterials(IList<xxMaterial> original, IList<xxMaterial> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Material count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].Name, saved[i].Name, "Material name does not match.");

                Assert.AreEqual(original[i].Power, saved[i].Power, "Material power does not match.");

                //verify ambient
                Assert.AreEqual(original[i].Ambient.R, saved[i].Ambient.R, "Material Ambient R do not match.");
                Assert.AreEqual(original[i].Ambient.G, saved[i].Ambient.G, "Material Ambient G do not match.");
                Assert.AreEqual(original[i].Ambient.B, saved[i].Ambient.B, "Material Ambient B do not match.");
                Assert.AreEqual(original[i].Ambient.A, saved[i].Ambient.A, "Material Ambient A do not match.");

                //verify emissive
                Assert.AreEqual(original[i].Emissive.R, saved[i].Emissive.R, "Material Emissive R do not match.");
                Assert.AreEqual(original[i].Emissive.G, saved[i].Emissive.G, "Material Emissive G do not match.");
                Assert.AreEqual(original[i].Emissive.B, saved[i].Emissive.B, "Material Emissive B do not match.");
                Assert.AreEqual(original[i].Emissive.A, saved[i].Emissive.A, "Material Emissive A do not match.");

                //verify diffuse
                Assert.AreEqual(original[i].Diffuse.R, saved[i].Diffuse.R, "Material Diffuse R do not match.");
                Assert.AreEqual(original[i].Diffuse.G, saved[i].Diffuse.G, "Material Diffuse G do not match.");
                Assert.AreEqual(original[i].Diffuse.B, saved[i].Diffuse.B, "Material Diffuse B do not match.");
                Assert.AreEqual(original[i].Diffuse.A, saved[i].Diffuse.A, "Material Diffuse A do not match.");

                //verify specular
                Assert.AreEqual(original[i].Specular.R, saved[i].Specular.R, "Material Specular R do not match.");
                Assert.AreEqual(original[i].Specular.G, saved[i].Specular.G, "Material Specular G do not match.");
                Assert.AreEqual(original[i].Specular.B, saved[i].Specular.B, "Material Specular B do not match.");
                Assert.AreEqual(original[i].Specular.A, saved[i].Specular.A, "Material Specular A do not match.");

                //verify unknown
                Assert.AreEqual(original[i].Unknown.Length, saved[i].Unknown.Length, "Material unknown length does not match.");

                for (int x = 0; x < original[i].Unknown.Length; x++)
                    Assert.AreEqual(original[i].Unknown[x], saved[i].Unknown[x], "Material unknown does not match.");

                //verify material textures
                VerifyMaterialTextures(original[i].Textures, saved[i].Textures);
            }
        }

        public static void VerifyTextures(IList<xxTexture> original, IList<xxTexture> saved)
        {
            Assert.AreEqual(original.Count, saved.Count, "Texture count does not match.");

            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].Name, saved[i].Name, "Texture name does not match.");

                Assert.AreEqual(original[i].Checksum, saved[i].Checksum, "Texture checksum does not match.");

                Assert.AreEqual(original[i].Depth, saved[i].Depth, "Texture depth does not match.");

                Assert.AreEqual(original[i].Format, saved[i].Format, "Texture format does not match.");

                Assert.AreEqual(original[i].Height, saved[i].Height, "Texture height does not match.");

                Assert.AreEqual(original[i].ImageFileFormat, saved[i].ImageFileFormat, "Texture image file format does not match.");

                Assert.AreEqual(original[i].MipLevels, saved[i].MipLevels, "Texture checksum does not match.");

                Assert.AreEqual(original[i].ResourceType, saved[i].ResourceType, "Texture checksum does not match.");

                Assert.AreEqual(original[i].Width, saved[i].Width, "Texture checksum does not match.");

                //verify unknown
                Assert.AreEqual(original[i].Unknown.Length, saved[i].Unknown.Length, "Texture unknown length does not match.");

                for (int x = 0; x < original[i].Unknown.Length; x++)
                    Assert.AreEqual(original[i].Unknown[x], saved[i].Unknown[x], "Texture unknown does not match.");

                //verify image data
                Assert.AreEqual(original[i].ImageData.Length, saved[i].ImageData.Length, "Texture image data length does not match.");

                for (int x = 0; x < original[i].ImageData.Length; x++)
                    Assert.AreEqual(original[i].ImageData[x], saved[i].ImageData[x], "Texture image data does not match.");
            }
        }

        public static void VerifyFile(Xx3File original, Xx3File saved)
        {
            //verify version
            Assert.AreEqual(original.Version, saved.Version, "Version does not match.");

            //verify unknown
            Assert.AreEqual(original.HeaderUnknown.Length, saved.HeaderUnknown.Length, "Header unknown length does not match.");

            for (int x = 0; x < original.HeaderUnknown.Length; x++)
                Assert.AreEqual(original.HeaderUnknown[x], saved.HeaderUnknown[x], "Header unknown does not match.");

            //verify object
            VerifyUnknowns(new[] { original.RootObject }, new[] { saved.RootObject });

            //verify unencoded data
            Assert.AreEqual(original.UnencodedData.Length, saved.UnencodedData.Length, "Unencoded data length does not match.");

            for (int x = 0; x < original.UnencodedData.Length; x++)
                Assert.AreEqual(original.UnencodedData[x], saved.UnencodedData[x], "Unencoded data does not match.");
        }

        public static void VerifyFile(Xx4File original, Xx4File saved)
        {
            //verify version
            Assert.AreEqual(original.Version, saved.Version, "Version does not match.");

            //verify unknown
            Assert.AreEqual(original.HeaderUnknown.Length, saved.HeaderUnknown.Length, "Header unknown length does not match.");

            for (int x = 0; x < original.HeaderUnknown.Length; x++)
                Assert.AreEqual(original.HeaderUnknown[x], saved.HeaderUnknown[x], "Header unknown does not match.");

            //verify object
            VerifyObject(original.RootObject, saved.RootObject);

            //verify unencoded data
            Assert.AreEqual(original.UnencodedData.Length, saved.UnencodedData.Length, "Unencoded data length does not match.");

            for (int x = 0; x < original.UnencodedData.Length; x++)
                Assert.AreEqual(original.UnencodedData[x], saved.UnencodedData[x], "Unencoded data does not match.");
        }
    }
}
