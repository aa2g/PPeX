using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace PPeX.External.PP
{
	public static class ppFormat_AA2
	{
		public static Stream ReadStream(Stream stream)
		{
			return new SeekableCryptoStream(stream, CryptoTransform(), CryptoStreamMode.Read);
		}

		public static Stream WriteStream(Stream stream)
		{
			return new WakeariStream(stream, CryptoTransform(), CryptoStreamMode.Write);
		}

		public static ppHeader_Wakeari.Metadata FinishWriteTo(Stream stream)
		{
			((CryptoStream)stream).FlushFinalBlock();

			ppHeader_Wakeari.Metadata metadata = new ppHeader_Wakeari.Metadata();
			metadata.LastBytes = ((WakeariStream)stream).LastBytes;
			return metadata;
		}

		private static ICryptoTransform CryptoTransform()
		{
			return new CryptoTransformOneCode(new byte[] {
				0x4D, 0x2D, 0xBF, 0x6A, 0x5B, 0x4A, 0xCE, 0x9D,
				0xF4, 0xA5, 0x16, 0x87, 0x92, 0x9B, 0x13, 0x03,
				0x8F, 0x92, 0x3C, 0xF0, 0x98, 0x81, 0xDB, 0x8E,
				0x5F, 0xB4, 0x1D, 0x2B, 0x90, 0xC9, 0x65, 0x00 });
		}
	}

	public class CryptoTransformOneCode : ICryptoTransform
	{
		public bool CanReuseTransform => true;

		public bool CanTransformMultipleBlocks => true;

		public int InputBlockSize => OutputBlockSize;

		public int OutputBlockSize { get; }

		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			int byteCount = inputCount - (inputCount % OutputBlockSize);
			int vectorCount = Vector<byte>.Count;

			for (int i = 0; i <= byteCount - vectorCount; i += vectorCount)
			{
				var v = new Vector<byte>(inputBuffer, i + inputOffset);
				var ev = new Vector<byte>(code, i % OutputBlockSize);
				Vector.Xor(v, ev).CopyTo(outputBuffer, outputOffset + i);
			}

			return byteCount;
		}

		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			byte[] outputBuffer = new byte[inputCount];
			int remainder = inputCount % 4;
			int transformLength = inputCount - remainder;
			int vectorCount = Vector<byte>.Count;

			int i;

			for (i = 0; i <= transformLength - vectorCount; i += vectorCount)
			{
				var v = new Vector<byte>(inputBuffer, i + inputOffset);
				var ev = new Vector<byte>(code, i % OutputBlockSize);
				Vector.Xor(v, ev).CopyTo(outputBuffer, i);
			}

			for (; i < transformLength; i++)
			{
				outputBuffer[i] = (byte)(inputBuffer[inputOffset + i] ^ code[i]);
			}

			Array.Copy(inputBuffer, inputOffset + transformLength, outputBuffer, transformLength, remainder);
			return outputBuffer;
		}

		public void Dispose()
		{

		}

		private readonly byte[] code;

		public CryptoTransformOneCode(byte[] code)
		{
			this.code = code;
			OutputBlockSize = code.Length;
		}
	}
}