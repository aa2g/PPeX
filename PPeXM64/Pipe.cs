using System;
using System.Text;
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
            serverThread = new Thread(ServerMethod);

            serverThread.Start();
        }

        protected void ServerMethod()
        {
            internalPipe.WaitForConnection();
            StreamHandler handler = new StreamHandler(new BufferedStream(internalPipe));

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
	    public Stream BaseStream { get; }

        public StreamHandler(Stream ioStream)
        {
            BaseStream = ioStream;
        }

        public void Dispose()
        {
            ((IDisposable)BaseStream).Dispose();
        }

        public string ReadString()
        {
            int len;
            //Read the length
            len = BaseStream.ReadByte() << 8;
            len |= BaseStream.ReadByte();

            if (len < 0)
                return "";

            //Read the string
            byte[] inBuffer = new byte[len];
            BaseStream.Read(inBuffer, 0, len);

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
            BaseStream.WriteByte((byte)(len >> 8));
            BaseStream.WriteByte((byte)(len & 0xFF));

            //Write the string
            BaseStream.Write(outBuffer, 0, len);
            BaseStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
