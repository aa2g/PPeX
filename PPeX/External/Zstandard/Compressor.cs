using System;
using size_t = System.UIntPtr;

namespace ZstdNet
{
	public class Compressor : IDisposable
	{
		public Compressor()
			: this(new CompressionOptions(CompressionOptions.DefaultCompressionLevel), 1)
		{ }

		public Compressor(CompressionOptions options, uint threads)
		{
			Options = options;

            cctx = ExternMethods.ZSTDMT_createCCtx(threads).EnsureZstdSuccess();

            var result = ExternMethods.ZSTDMT_initCStream(cctx, options.CompressionLevel);

            //result.EnsureZstdSuccess();



            /*
			cctx = ExternMethods.ZSTD_createCCtx().EnsureZstdSuccess();

            ccparams = ExternMethods.ZSTD_createCCtxParams();

            ExternMethods.ZSTD_initCCtxParams(ccparams, options.CompressionLevel).EnsureZstdSuccess();

            var result = ExternMethods.ZSTD_CCtxParam_setParameter(ccparams, ZSTD_cParameter.ZSTD_p_nbThreads, 8).EnsureZstdSuccess();

            ExternMethods.ZSTD_CCtx_setParametersUsingCCtxParams(cctx, ccparams);*/
        }

		~Compressor()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(disposed)
				return;

            //ExternMethods.ZSTD_freeCCtx(cctx);
            ExternMethods.ZSTDMT_freeCCtx(cctx);

            disposed = true;
		}

		private bool disposed = false;

		public byte[] Wrap(byte[] src)
		{
			return Wrap(new ArraySegment<byte>(src));
		}

		public byte[] Wrap(ArraySegment<byte> src)
		{
			if (src.Count == 0)
				return new byte[0];

			var dstCapacity = GetCompressBound(src.Count);
			var dst = new byte[dstCapacity];

			var dstSize = Wrap(src, dst, 0);

			if(dstCapacity == dstSize)
				return dst;
			var result = new byte[dstSize];
			Array.Copy(dst, result, dstSize);
			return result;
		}

		public static int GetCompressBound(int size)
		{
			return (int)ExternMethods.ZSTD_compressBound((size_t)size);
		}

		public int Wrap(byte[] src, byte[] dst, int offset)
		{
			return Wrap(new ArraySegment<byte>(src), dst, offset);
		}

		public int Wrap(ArraySegment<byte> src, byte[] dst, int offset)
		{
			if (offset < 0 || offset >= dst.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));

			if (src.Count == 0)
				return 0;

			var dstCapacity = dst.Length - offset;
			size_t dstSize;
			using(var srcPtr = new ArraySegmentPtr(src))
			using(var dstPtr = new ArraySegmentPtr(new ArraySegment<byte>(dst, offset, dstCapacity)))
			{
				if(Options.Cdict == IntPtr.Zero)
					//dstSize = ExternMethods.ZSTD_compressCCtx(cctx, dstPtr, (size_t)dstCapacity, srcPtr, (size_t)src.Count, Options.CompressionLevel);
                {

                    dstSize = ExternMethods.ZSTDMT_compressCCtx(cctx, dstPtr, (size_t)dstCapacity, srcPtr, (size_t)src.Count, Options.CompressionLevel);

                    // streaming code //
                    /*
                    var bSrc = new ExternMethods.ZSTD_Buffer
                    {
                        array = srcPtr,
                        size = (size_t)((src.Count / 2) - 1),
                        pos = (size_t)0
                    };

                    var bDst = new ExternMethods.ZSTD_Buffer
                    {
                        array = dstPtr,
                        size = (size_t)dstCapacity,
                        pos = (size_t)0
                    };

                    ExternMethods.ZSTDMT_compressStream_generic(cctx, ref bDst, ref bSrc, 2).EnsureZstdSuccess();

                    dstSize = bDst.pos;*/
                }
				else
					dstSize = ExternMethods.ZSTD_compress_usingCDict(cctx, dstPtr, (size_t)dstCapacity, srcPtr, (size_t)src.Count, Options.Cdict);
			}
			dstSize.EnsureZstdSuccess();
			return (int)dstSize;
		}

		public readonly CompressionOptions Options;

        private readonly IntPtr cctx;

        private readonly IntPtr ccparams;
    }
}
