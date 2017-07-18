using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Manager
{
    /// <summary>
    /// A client for the named pipe transfers.
    /// </summary>
    public class PipeClient
    {
        protected string Name;
        protected StreamHandler temp;

        public PipeClient(string name)
        {
            Name = name;
            NamedPipeClientStream client = new NamedPipeClientStream(Name);
            client.Connect();
            temp = new StreamHandler(client);
        }

        public StreamHandler CreateConnection()
        {
            return temp;
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
