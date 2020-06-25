namespace PPeX.External.PP.Crypto
{
	public interface ISpanCryptoTransform
	{
		public bool CanReuseTransform { get; }
		public bool CanTransformMultipleBlocks { get; }
		public int InputBlockSize { get; }
		public int OutputBlockSize { get; }

		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
	}
}