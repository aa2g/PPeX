using System;
using System.Buffers;

namespace PPeX.External.libresample
{
    public class LibResampler : IDisposable
    {
        protected IntPtr[] handles;

        protected double factor;

        public int SampleRate { get; protected set; }
        public int Channels { get; protected set; }

        public LibResampler(int originalRate, int newRate, int channels)
        {
            SampleRate = newRate;
            Channels = channels;

            factor = SampleRate / (double)originalRate;

            handles = new IntPtr[Channels];

            for (int i = 0; i < Channels; i++)
                handles[i] = LibResampleAPI.resample_open(1, factor, factor);
        }

        public void Resample(ReadOnlySpan<float> samples, Span<float> output, bool last, out int outputLength)
        {
            if (samples.Length % Channels != 0)
                throw new DataMisalignedException();

            outputLength = 0;

            IMemoryOwner<float>[] splitChannels = new IMemoryOwner<float>[Channels];

            int estChannelSamples = (int)Math.Ceiling((samples.Length / Channels) * factor);
            int actualChannelSamples = 0;

            for (int i = 0; i < Channels; i++)
            {
                using var tempChannelBuffer = GetNth<float>(samples, Channels, i, out int dataLength);

                splitChannels[i] = MemoryPool<float>.Shared.Rent(estChannelSamples);

                ResampleStream(tempChannelBuffer.Memory.Span.Slice(0, dataLength),
	                splitChannels[i].Memory.Span,
	                i,
	                last,
	                out _,
	                out actualChannelSamples);

                outputLength += actualChannelSamples;
            }

            for (int c = 0; c < Channels; c++)
            {
	            var span = splitChannels[c].Memory.Span.Slice(0, actualChannelSamples);

                for (int i = 0; i < actualChannelSamples; i++)
                {
                    output[c + (Channels * i)] = span[i];
                }

                splitChannels[c].Dispose();
            }
        }

        public int ResampleUpperBound(int sampleCount)
        {
	        //return (int)Math.Ceiling(sampleCount * factor);
	        return sampleCount;
        }

        protected static IMemoryOwner<T> GetNth<T>(ReadOnlySpan<T> original, int divisor, int offset, out int newLength)
        {
            if (original.Length % divisor != 0)
                throw new DataMisalignedException();

            newLength = original.Length / divisor;
	        var output = MemoryPool<T>.Shared.Rent(newLength);
	        var outputSpan = output.Memory.Span;

            int index = 0;
            for (int i = offset; i < original.Length; i += divisor)
            {
                outputSpan[index++] = original[i];
            }

            return output;
        }
        
        protected unsafe void ResampleStream(ReadOnlySpan<float> samples, Span<float> output, int channel, bool last, out int inputSamplesUsed, out int outputSamplesUsed)
        {
            if (handles[channel] == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(handles));

            int processedSamples;
            int sampleBufferUsed;

            fixed (float* samplePtr = samples)
            fixed (float* outputPtr = output)
            {
	            processedSamples = LibResampleAPI.resample_process(handles[channel],
		            factor,
		            samplePtr,
		            samples.Length,
		            last ? 1 : 0,
		            out sampleBufferUsed,
		            outputPtr,
		            output.Length);
            }

            inputSamplesUsed = Math.Max(sampleBufferUsed, 0);
            outputSamplesUsed = Math.Max(processedSamples, 0);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                for (int i = 0; i < Channels; i++)
                {
                    LibResampleAPI.resample_close(handles[i]);
                    handles[i] = IntPtr.Zero;
                }

                disposed = true;
            }
        }
        
        ~LibResampler()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}