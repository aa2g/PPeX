using System.IO;
using System.Threading.Tasks;

namespace PPeX
{
    /// <summary>
    /// A data source from a file.
    /// </summary>
    public class FileSource : BaseSource
    {
        /// <summary>
        /// The filename of the file in use.
        /// </summary>
        public string Filename { get; protected set; }

        public FileSource(string Filename)
        {
            this.Filename = Filename;
        }

        public override Task GenerateMd5HashAsync()
        {
	        throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns a stream of uncompressed data.
        /// </summary>
        /// <returns></returns>
        public override Stream GetStream()
        {
            return new FileStream(Filename, FileMode.Open, FileAccess.Read);
        }

        protected ulong _size = ulong.MaxValue;
        public override ulong Size {
            get
            {
                if (_size == ulong.MaxValue)
                    _size = (ulong)new FileInfo(Filename).Length;

                return _size;
            }
        }

        public override void Dispose()
        {

        }
    }
}
