using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.External.libresample
{
    public class LibResampler : IDisposable
    {
        protected IntPtr[] handles;

        protected double factor;

        public readonly int resamplerQuality = 1;

        public int SampleRate { get; protected set; }
        public int Channels { get; protected set; }

        public LibResampler(int originalRate, int newRate, int channels)
        {
            SampleRate = newRate;
            Channels = channels;

            factor = SampleRate / (double)originalRate;

            handles = new IntPtr[Channels];

            for (int i = 0; i < Channels; i++)
                handles[i] = API.resample_open(resamplerQuality, factor, factor);
        }

        public float[] Resample(float[] samples, out int sampleBufferUsed)
        {
            if (samples.Length % Channels != 0)
                throw new DataMisalignedException();

            sampleBufferUsed = 0;

            float[][] splitChannels = new float[Channels][];

            for (int i = 0; i < Channels; i++)
            {
                float[] channel = GetNth(samples, Channels, i);

                splitChannels[i] = ResampleStream(channel, i, out int localBufferUsed);
                sampleBufferUsed += localBufferUsed;
            }

            float[] reconstructed = new float[splitChannels.Sum(x => x.Length)];

            for (int c = 0; c < Channels; c++)
            {
                for (int i = 0; i < splitChannels[c].Length; i++)
                {
                    reconstructed[c + (Channels * i)] = splitChannels[c][i];
                }
            }

            return reconstructed;
        }

        protected static T[] GetNth<T>(T[] original, int divisor, int offset)
        {
            if (original.Length % divisor != 0)
                throw new DataMisalignedException();

            int newLength = original.Length / divisor;
            T[] output = new T[newLength];

            int index = 0;
            for (int i = offset; i < original.Length; i += divisor)
            {
                output[index++] = original[i];
            }

            return output;
        }
        
        protected float[] ResampleStream(float[] samples, int channel, out int sampleBufferUsed)
        {
            if (handles[channel] == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(handles));

            int estOutSamples = (int)Math.Round(samples.Length * factor);
            float[] outBuffer = new float[estOutSamples];

            int processedSamples;

            processedSamples = API.resample_process(handles[channel], factor, samples, samples.Length, 0, out sampleBufferUsed, outBuffer, estOutSamples);

            processedSamples = Math.Max(processedSamples, 0);

            Array.Resize(ref outBuffer, processedSamples);
            return outBuffer;
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                for (int i = 0; i < Channels; i++)
                {
                    API.resample_close(handles[i]);
                    handles[i] = IntPtr.Zero;
                }

                disposedValue = true;
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
