using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PPeX
{
#warning remove
    public class ImageSubfile : BaseSubfile
    {
        protected uint _size = 0;
        public override uint Size => _size;

        public ImageSubfile(IDataSource Source, string Name, string Archive) : base(Source, Name.Replace(".png", ".bmp"), Archive)
        {
            using (Stream source = Source.GetStream())
            using (Image img = Image.FromStream(source))
            {
                _size = (uint)(img.Width * img.Height * 4);
            }
        }

        public override void WriteToStream(Stream stream)
        {
            using (Stream source = Source.GetStream())
            using (Image img = Image.FromStream(source))
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }
    }
}
