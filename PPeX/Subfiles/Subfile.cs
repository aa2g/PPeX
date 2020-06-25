using System.IO;
using PPeX.Encoders;

namespace PPeX
{
    public class Subfile : ISubfile
    {
        /// <summary>
        /// The uncompressed size of the data.
        /// </summary>
        public ulong Size { get; protected set; }

        /// <summary>
        /// The name of the .pp file the subfile is associated with.
        /// </summary>
        public string ArchiveName { get; protected set; }

        public string EmulatedName { get; protected set; }

        /// <summary>
        /// The name of the subfile as it is stored in a .pp file.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The content type of the subfile.
        /// </summary>
        public ArchiveFileType Type { get; protected set; }

        /// <summary>
        /// The data source of the subfile.
        /// </summary>
        public IDataSource Source { get; protected set; }

        public RequestedConversion RequestedConversion { get; }

        /// <summary>
        /// Writes an uncompressed version of the data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the uncompressed data to.</param>
        public Stream GetStream()
        {
            return Source.GetStream();
        }

        public Subfile(IDataSource source, string name, string archiveName) : this(source, name, archiveName, ArchiveFileType.Raw)
        {
	        if (name.EndsWith(".wav"))
	        {
		        Type = ArchiveFileType.WaveAudio;
		        RequestedConversion = OpusEncoder.CreateConversionArgs();
	        }
	        else if (name.EndsWith(".opus"))
		        Type = ArchiveFileType.OpusAudio;
	        else
		        Type = ArchiveFileType.Raw;
        }

        public Subfile(IDataSource source, string name, string archiveName, ArchiveFileType type) : this(source, name, archiveName, type, null) { }

        public Subfile(IDataSource source, string name, string archiveName, ArchiveFileType type, RequestedConversion requestedConversion)
        {
	        if (!archiveName.EndsWith(".pp"))
		        archiveName += ".pp";

            ArchiveName = archiveName;
            Name = name;
            Source = source;
            Size = source.Size;
            Type = type;
            RequestedConversion = requestedConversion;

            EmulatedName = name;
        }
    }
}
