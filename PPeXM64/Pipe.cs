using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Threading;
using System.IO;

namespace PPeXM64
{
    /// <summary>
    /// A pipe server that handles request from the PPeX client.
    /// </summary>
    public class PipeServer
    {
        protected NamedPipeServerStream internalPipe;
        protected Thread serverThread;

        public delegate void OnRequestEventHandler(string request, string argument, StreamHandler handler);
        public event OnRequestEventHandler OnRequest;

        public event EventHandler OnDisconnect;

        public PipeServer(string name)
        {
            internalPipe = new NamedPipeServerStream(name, PipeDirection.InOut);
            serverThread = new Thread(new ThreadStart(ServerMethod));

            serverThread.Start();
        }

        protected void ServerMethod()
        {
            internalPipe.WaitForConnection();
            StreamHandler handler = new StreamHandler(internalPipe);

            while (true)
            {
                string request = handler.ReadString();

                string argument = handler.ReadString();

                if (!internalPipe.IsConnected)
                    OnDisconnect?.Invoke(this, null);

                OnRequest?.Invoke(request, argument, handler);
            }
        }
    }

    /// <summary>
    /// A wrapper for a pipe to be able to communicate over it.
    /// </summary>
    public class StreamHandler : IDisposable
    {
        private Stream ioStream;
        public Stream BaseStream => ioStream;

        public StreamHandler(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

        public void Dispose()
        {
            ((IDisposable)ioStream).Dispose();
        }

        public string ReadString()
        {
            int len;
            //Read the length
            len = ioStream.ReadByte() << 8;
            len |= ioStream.ReadByte();

            if (len < 0)
                return "";

            //Read the string
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return Encoding.Unicode.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = Encoding.Unicode.GetBytes(outString);
            //Write the length
            int len = outBuffer.Length;
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }
            ioStream.WriteByte((byte)(len >> 8));
            ioStream.WriteByte((byte)(len & 0xFF));

            //Write the string
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
