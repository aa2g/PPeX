using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPeX;
using PPeX.Encoders;
using PPeX.External.Wave;
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
            Assert.IsTrue(subfile.ArchiveName == "arc", $"Archive name did stay as \"arc\". Actual: {subfile.ArchiveName}");
            Assert.IsTrue(subfile.EmulatedArchiveName == "arc", $"Emulated archive name did stay as \"arc\". Actual: {subfile.EmulatedArchiveName}");

            using (OpusEncoder decoder = new OpusEncoder(subfile.GetRawStream()))
            {
                Stream decoded = decoder.Decode();

                //Ensure it can be read
                using (WaveReader wavreader = new WaveReader(decoded)) { }
            }

            File.Delete("opusencodertest.ppx");
        }

        public void Cleanup()
        {
            if (File.Exists("opusencodertest.ppx"))
                File.Delete("opusencodertest.ppx");
        }
    }
}
