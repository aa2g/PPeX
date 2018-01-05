using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PPeX;
using PPeX.Archives;
using PPeX.Compressors;

namespace PPeXTests
{
    [TestClass]
    public class HybridChunkWriterTests
    {
        [TestMethod]
        public void HybridChunkWriter_AssignsPropertiesOnConstructor()
        {
            Mock<IArchiveContainer> containerMock = new Mock<IArchiveContainer>();

            HybridChunkWriter writer = new HybridChunkWriter(666, ArchiveChunkCompression.Uncompressed, containerMock.Object, 0, Core.Settings.DefaultEncodingConversions);

            Assert.AreEqual((uint)666, writer.ID);
            Assert.AreEqual(ArchiveChunkCompression.Uncompressed, writer.Compression);
        }

        [TestMethod]
        public void HybridChunkWriter_IsReadyTest()
        {
            Mock<IArchiveContainer> containerMock = new Mock<IArchiveContainer>();

            HybridChunkWriter writer = new HybridChunkWriter(666, ArchiveChunkCompression.Uncompressed, containerMock.Object, 0, Core.Settings.DefaultEncodingConversions);

            Assert.IsFalse(writer.IsReady);

            writer.Compress(new List<ICompressor> { new PassthroughCompressor() });

            Assert.IsTrue(writer.IsReady);
        }

        [TestMethod]
        public void HybridChunkWriter_ContainsFilesTest()
        {
            Mock<IArchiveContainer> containerMock = new Mock<IArchiveContainer>();

            HybridChunkWriter writer = new HybridChunkWriter(666, ArchiveChunkCompression.Uncompressed, containerMock.Object, 0, Core.Settings.DefaultEncodingConversions);

            Assert.IsFalse(writer.ContainsFiles);

            byte[] data = new byte[16];
            Subfile subfileMock = new Subfile(new MemorySource(data), "", "", ArchiveFileType.Raw);

            writer.AddFile(subfileMock);

            Assert.IsTrue(writer.ContainsFiles);
        }

        [TestMethod]
        public void HybridChunkWriter_TryAddFileTest()
        {
            Mock<IArchiveContainer> containerMock = new Mock<IArchiveContainer>();

            HybridChunkWriter writer = new HybridChunkWriter(666, ArchiveChunkCompression.Uncompressed, containerMock.Object, 20, Core.Settings.DefaultEncodingConversions);

            byte[] data = new byte[16];
            Subfile subfileMock = new Subfile(new MemorySource(data), "1", "1");
            Subfile subfileMock2 = new Subfile(new MemorySource(data), "2", "1");

            Assert.IsTrue(writer.TryAddFile(subfileMock));
            
            Assert.IsTrue(writer.TryAddFile(subfileMock2), "Writer did not recognize a duplicate subfile entry.");

            byte[] data2 = new byte[32];
            Subfile subfileMock3 = new Subfile(new MemorySource(data2), "3", "2");

            Assert.IsFalse(writer.TryAddFile(subfileMock3), "Writer did not reject a subfile that went over the chunk size limit.");
        }

        [TestMethod]
        public void HybridChunkWriter_CompressTest()
        {
            Mock<BaseCompressor> compressorMock = new Mock<BaseCompressor>();
            compressorMock.CallBase = true;

            compressorMock.Setup(x => x.WriteToStream(It.IsAny<Stream>(), It.IsAny<Stream>()))
                .Callback((Stream i, Stream o) => i.CopyTo(o));

            compressorMock.Setup(x => x.Compression).Returns(ArchiveChunkCompression.Uncompressed);

            Mock<BaseCompressor> compressorMock2 = new Mock<BaseCompressor>();
            compressorMock2.CallBase = true;

            compressorMock2.Setup(x => x.WriteToStream(It.IsAny<Stream>(), It.IsAny<Stream>()))
                .Callback((Stream i, Stream o) => i.CopyTo(o));

            compressorMock2.Setup(x => x.Compression).Returns(ArchiveChunkCompression.Zstandard);


            Mock<IArchiveContainer> containerMock = new Mock<IArchiveContainer>();

            HybridChunkWriter writer = new HybridChunkWriter(666, ArchiveChunkCompression.Uncompressed, containerMock.Object, 0, Core.Settings.DefaultEncodingConversions);

            byte[] data = new byte[16];
            Subfile subfileMock = new Subfile(new MemorySource(data), "1", "1");
            Subfile subfileMock2 = new Subfile(new MemorySource(data), "2", "1");

            writer.AddFile(subfileMock);
            writer.AddFile(subfileMock2);

            byte[] data2 = new byte[32];
            Subfile subfileMock3 = new Subfile(new MemorySource(data2), "3", "2");

            writer.AddFile(subfileMock3);

            writer.Compress(new List<ICompressor> { compressorMock.Object, compressorMock2.Object });

            compressorMock.Verify(x => x.WriteToStream(It.IsAny<Stream>(), It.IsAny<Stream>()), Times.AtLeastOnce);
            compressorMock2.Verify(x => x.WriteToStream(It.IsAny<Stream>(), It.IsAny<Stream>()), Times.Never);

            Assert.IsNotNull(writer.CompressedStream);
            Assert.IsTrue(writer.CompressedStream.Length == 16 + 32);

            Assert.IsNotNull(writer.Receipt);

            Assert.AreEqual(ArchiveChunkCompression.Uncompressed, writer.Receipt.Compression);
            Assert.AreEqual((uint)(16 + 32), writer.Receipt.CompressedSize);
            Assert.AreEqual((uint)(16 + 32), writer.Receipt.UncompressedSize);
            Assert.AreEqual((ulong)0, writer.Receipt.FileOffset);
            Assert.AreEqual((uint)666, writer.Receipt.ID);
            Assert.AreEqual(PPeX.External.CRC32.CRC32.Compute(writer.CompressedStream), writer.Receipt.CRC);

            Assert.AreEqual(3, writer.Receipt.FileReceipts.Count);

            foreach (var reciept in writer.Receipt.FileReceipts)
            {
                Assert.IsNotNull(reciept.InternalName);
                Assert.AreNotEqual("", reciept.InternalName);
                Assert.AreNotEqual("", reciept.EmulatedName);
            }

        }
    }
}
