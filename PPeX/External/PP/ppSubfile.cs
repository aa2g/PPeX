using System;
using System.IO;

namespace PPeX.External.PP
{
	/// <summary>
	/// If removed from a ppParser, CreateReadStream() is no longer guaranteed to work. The .pp file may have changed,
	/// so you have to transfer the ppSubfile's data when removing.
	/// </summary>
	public class ppSubfile : NeedsSourceStreamForWriting, IReadFile, IWriteFile
	{
		public string ppPath;
		public uint offset;
		public uint size;

		public object Metadata { get; set; }
		public Stream SourceStream { get; set; }

		public ppSubfile(string ppPath)
		{
			this.ppPath = ppPath;
		}

		public string Name { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryReader reader = new BinaryReader(SourceStream);
			BinaryWriter writer = new BinaryWriter(stream);
			for (byte[] buf; (buf = reader.ReadBytes(Utility.BufSize)).Length > 0; )
			{
				writer.Write(buf);
			}
		}

		public Stream CreateReadStream()
		{
			FileStream fs = null;
			try
			{
				fs = File.OpenRead(ppPath);
				fs.Seek(offset, SeekOrigin.Begin);
				return ppFormat_AA2.ReadStream(new PartialStream(fs, size));
			}
			catch (Exception e)
			{
				if (fs != null)
				{
					fs.Close();
				}
				throw e;
			}
		}

		public Stream CreateReadStream(Stream existingStream)
		{
			return ppFormat_AA2.ReadStream(new PartialStream(existingStream, offset, size, true));
		}

		public override string ToString()
		{
			return Name;
		}
	}
}