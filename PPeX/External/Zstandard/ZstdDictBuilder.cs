using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPeX.External.Zstandard
{
	public static class ZstdDictBuilder
	{
		public static byte[] TrainFromBuffer(IEnumerable<byte[]> samples, int dictCapacity = DefaultDictCapacity)
		{
			var ms = new MemoryStream();
			var samplesSizes = samples.Select(sample =>
			{
				ms.Write(sample, 0, sample.Length);
				return (UIntPtr)sample.Length;
			}).ToArray();

			var dictBuffer = new byte[dictCapacity];
			var dictSize = ExternMethods.ZDICT_trainFromBuffer(dictBuffer, (UIntPtr)dictCapacity, ms.ToArray(), samplesSizes, (uint)samplesSizes.Length);

			ReturnValueExtensions.EnsureZdictSuccess(dictSize);

			if (dictCapacity != (int)dictSize)
				Array.Resize(ref dictBuffer, (int)dictSize);

			return dictBuffer;
		}

		public const int DefaultDictCapacity = 112640; // Used by zstd utility by default
	}
}