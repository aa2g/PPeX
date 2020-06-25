using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPeX.External.PP
{
	public static class ppHeader_Wakeari
	{
		const byte FirstByte = 0x01;
		const int Version = 0x6C;
		static readonly byte[] ppVersionBytes = Encoding.ASCII.GetBytes("[PPVER]\0");

		public static uint HeaderSize(int numFiles)
		{
			return (uint)((288 * numFiles) + 9 + 12);
		}

		public static byte[] TransformHeaderBytes(byte[] buf)
		{
			byte[] table = new byte[]
			{
				0xFA, 0x49, 0x7B, 0x1C, // var48
				0xF9, 0x4D, 0x83, 0x0A,
				0x3A, 0xE3, 0x87, 0xC2, // var24
				0xBD, 0x1E, 0xA6, 0xFE
			};

			for (int i = 0; i < buf.Length; i++)
			{
				byte modulo = (byte)(i & 0x7);
				table[modulo] += table[8 + modulo];
				buf[i] ^= table[modulo];
			}

			return buf;
		}

		public static List<IWriteFile> ReadHeader(FileStream stream)
		{
			stream.Position = 0;
			BinaryReader reader = new BinaryReader(stream);

			byte[] versionHeader = reader.ReadBytes(8);

			reader.ReadBytes(4);
			//Version = BitConverter.ToInt32(DecryptHeaderBytes(reader.ReadBytes(4)), 0);

			TransformHeaderBytes(reader.ReadBytes(1));  // first byte
			int numFiles = BitConverter.ToInt32(TransformHeaderBytes(reader.ReadBytes(4)), 0);
			byte[] buf = TransformHeaderBytes(reader.ReadBytes(numFiles * 288));

			var subfiles = new List<IWriteFile>(numFiles);
			for (int i = 0; i < numFiles; i++)
			{
				int offset = i * 288;
				ppSubfile subfile = new ppSubfile(stream.Name);
				int length = 260;
				for (int j = 0; j < length; j++)
				{
					if (buf[offset + j] == 0x00)
					{
						length = j;
						break;
					}
				}
				subfile.Name = Utility.EncodingShiftJIS.GetString(buf, offset, length);
				subfile.size = BitConverter.ToUInt32(buf, offset + 260);
				subfile.offset = BitConverter.ToUInt32(buf, offset + 264);

				Metadata metadata = new Metadata();
				metadata.LastBytes = new byte[20];
				System.Array.Copy(buf, offset + 268, metadata.LastBytes, 0, 20);
				subfile.Metadata = metadata;

				subfiles.Add(subfile);
			}

			return subfiles;
		}

		public static void WriteHeader(Stream stream, List<IWriteFile> files, uint[] sizes, object[] metadata)
		{
			byte[] headerBuf = new byte[HeaderSize(files.Count)];
			BinaryWriter writer = new BinaryWriter(new MemoryStream(headerBuf));

			writer.Write(ppVersionBytes);
			writer.Write(TransformHeaderBytes(BitConverter.GetBytes(Version)));
			
			writer.Write(TransformHeaderBytes(new byte[] { FirstByte }));
			writer.Write(TransformHeaderBytes(BitConverter.GetBytes(files.Count)));

			byte[] fileHeaderBuf = new byte[288 * files.Count];
			uint fileOffset = (uint)headerBuf.Length;
			for (int i = 0; i < files.Count; i++)
			{
				int idx = i * 288;
				Utility.EncodingShiftJIS.GetBytes(files[i].Name).CopyTo(fileHeaderBuf, idx);
				BitConverter.GetBytes(sizes[i]).CopyTo(fileHeaderBuf, idx + 260);
				BitConverter.GetBytes(fileOffset).CopyTo(fileHeaderBuf, idx + 264);

				Metadata wakeariMetadata = (Metadata)metadata[i];
				Array.Copy(wakeariMetadata.LastBytes, 0, fileHeaderBuf, idx + 268, 20);
				BitConverter.GetBytes(sizes[i]).CopyTo(fileHeaderBuf, idx + 284);

				fileOffset += sizes[i];
			}

			writer.Write(TransformHeaderBytes(fileHeaderBuf));
			writer.Write(TransformHeaderBytes(BitConverter.GetBytes(headerBuf.Length)));
			writer.Flush();
			stream.Write(headerBuf, 0, headerBuf.Length);
		}

		public struct Metadata
		{
			public byte[] LastBytes { get; set; }
		}
	}
}