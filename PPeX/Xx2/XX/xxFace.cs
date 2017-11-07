using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX.Xx2
{
    public class xxFace
    {
        public ushort[] VertexIndicies = new ushort[3];

        internal xxFace(BinaryReader reader)
        {
            for (int i = 0; i < 3; i++)
                VertexIndicies[i] = reader.ReadUInt16();
        }

        internal xxFace()
        {

        }

        public void Write(BinaryWriter writer)
        {
            for (int i = 0; i < 3; i++)
                writer.Write(VertexIndicies[i]);
        }
    }
}
