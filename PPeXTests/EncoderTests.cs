using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX;
using PPeX.Encoders;
using PPeX.External.Wave;
using PPeX.Xx2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeXTests
{
    [DeploymentItem("opus32.dll")]
    [DeploymentItem("libzstd32.dll")]
    [DeploymentItem("libresample32.dll")]
    [TestClass]
    public class EncoderTests
    {
        const string OpusTestFile = "AA2BGM13.opus";
        const string WavTestFile = "AA2BGM13.wav";
        const string XxTestFile = "A00_00_00_03.xx";

        [DeploymentItem(WavTestFile)]
        [TestMethod]
        public void OpusEncoderTest()
        {
            using (Stream source = File.OpenRead(WavTestFile))
            {
                using (OpusEncoder encoder = new OpusEncoder(source))
                {
                    Stream encoded = encoder.Encode();

                    using (OpusEncoder decoder = new OpusEncoder(encoded))
                    {
                        Stream decoded = decoder.Decode();

                        //Ensure it can be read
                        using (WaveReader wavreader = new WaveReader(decoded)) { }
                    }
                }
            }
        }

        [DeploymentItem(OpusTestFile)]
        [TestMethod]
        public void OpusDecoderTest()
        {
            using (Stream opus = File.OpenRead(OpusTestFile))
            using (OpusEncoder decoder = new OpusEncoder(opus))
            {
                Stream decoded = decoder.Decode();

                //Ensure it can be read
                using (WaveReader wavreader = new WaveReader(decoded)) { }
            }
        }

        [DeploymentItem(WavTestFile)]
        [TestMethod]
        public void OpusArchiveEncoderTest()
        {
            ExtendedArchiveWriter writer = new ExtendedArchiveWriter("opusencoder");
            writer.Files.Add(new Subfile(new FileSource(WavTestFile), "audio.wav", "arc"));
            
            writer.Write("opusencodertest.ppx");

            ExtendedArchive arc = new ExtendedArchive("opusencodertest.ppx");

            var subfile = arc.Files.First();

            Assert.AreEqual(ArchiveFileType.OpusAudio, subfile.Type);
            Assert.IsTrue(subfile.Name == "audio.opus", $"Internal name did not switch to \"audio.opus\". Actual: {subfile.Name}");
            Assert.IsTrue(subfile.EmulatedName == "audio.wav", $"Emulated name did stay as \"audio.wav\". Actual: {subfile.EmulatedName}");
            Assert.IsTrue(subfile.ArchiveName == "arc", $"Archive name did not stay as \"arc\". Actual: {subfile.ArchiveName}");
            Assert.IsTrue(subfile.EmulatedArchiveName == "arc.pp", $"Emulated archive name did not change to \"arc.pp\". Actual: {subfile.EmulatedArchiveName}");

            using (OpusEncoder decoder = new OpusEncoder(subfile.GetRawStream()))
            {
                Stream decoded = decoder.Decode();

                //Ensure it can be read
                using (WaveReader wavreader = new WaveReader(decoded)) { }
            }

            File.Delete("opusencodertest.ppx");
        }

        [DeploymentItem(XxTestFile)]
        [TestMethod]
        public void Xx3EncoderTest()
        {
            using (Stream source = File.OpenRead(XxTestFile))
            {
                CompressedTextureBank bank = new CompressedTextureBank(ArchiveChunkCompression.LZ4);

                using (Xx3Encoder encoder = new Xx3Encoder(source, bank))
                {
                    Stream encoded = encoder.Encode();

                    using (Xx3Encoder decoder = new Xx3Encoder(encoded, bank))
                    {
                        Stream decoded = decoder.Decode();
                    }
                }
            }
        }

        [DeploymentItem(XxTestFile)]
        [TestMethod]
        public void Xx4ArchiveEncoderTest()
        {
            ExtendedArchiveWriter writer = new ExtendedArchiveWriter("xx4encoder");
            writer.Files.Add(new Subfile(new FileSource(XxTestFile), "mesh.xx", "arc"));

            writer.Write("xx4encodertest.ppx");

            ExtendedArchive arc = new ExtendedArchive("xx4encodertest.ppx");

            var subfile = arc.Files.First();

            Assert.AreEqual(ArchiveFileType.Xx4Mesh, subfile.Type);
            Assert.IsTrue(subfile.Name == "mesh.xx4", $"Internal name did not switch to \"mesh.xx4\". Actual: {subfile.Name}");
            Assert.IsTrue(subfile.EmulatedName == "mesh.xx", $"Emulated name did stay as \"mesh.xx\". Actual: {subfile.EmulatedName}");
            Assert.IsTrue(subfile.ArchiveName == "arc", $"Archive name did not stay as \"arc\". Actual: {subfile.ArchiveName}");
            Assert.IsTrue(subfile.EmulatedArchiveName == "arc.pp", $"Emulated archive name did not change to \"arc.pp\". Actual: {subfile.EmulatedArchiveName}");

            using (Xx4Encoder decoder = new Xx4Encoder(subfile.GetRawStream(), arc.TextureBank))
            {
                Stream decoded = decoder.Decode();
            }

            File.Delete("xx4encodertest.ppx");
        }

        public void Cleanup()
        {
            if (File.Exists("opusencodertest.ppx"))
                File.Delete("opusencodertest.ppx");

            if (File.Exists("xx3encodertest.ppx"))
                File.Delete("xx3encodertest.ppx");
        }
    }
}
