using System.IO;
using System.Threading.Tasks;

namespace PPeX
{
    public abstract class BaseSource : IDataSource
    {
	    /// <inheritdoc />
        public virtual ulong Size { get; protected set; }

        protected byte[] _md5;

        /// <inheritdoc />
        public virtual byte[] Md5
        {
            get
            {
                if (_md5 == null)
                    using (Stream stream = GetStream())
                        _md5 = Utility.GetMd5(stream);

                return _md5;
            }
            set => _md5 = value;
        }

        /// <inheritdoc />
        public abstract void Dispose();

        public abstract Task GenerateMd5HashAsync();

        /// <inheritdoc />
        public abstract Stream GetStream();
    }
}