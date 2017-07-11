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
    public class PipeServer
    {
        protected NamedPipeServerStream internalPipe;
        protected Thread serverThread;

        public delegate void OnRequestEventHandler(string request, string argument, StreamHandler handler);
        public event OnRequestEventHandler OnRequest;

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

                OnRequest?.Invoke(request, argument, handler);

                //internalPipe.Disconnect();
            }
        }

        /*
        public delegate void PipeMessageRecievedHandler(int instance, string message);
        public event PipeMessageRecievedHandler PipeMessageRecieved;*/
    }

    public class StreamHandler
    {
        private Stream ioStream;
        public Stream BaseStream => ioStream;

        public StreamHandler(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() << 8;
            len |= ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);
            
            return Encoding.Unicode.GetString(inBuffer);
        }
        
        public int WriteString(string outString)
        {
            byte[] outBuffer = Encoding.Unicode.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }
            ioStream.WriteByte((byte)(len >> 8));
            ioStream.WriteByte((byte)(len & 0xFF));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
