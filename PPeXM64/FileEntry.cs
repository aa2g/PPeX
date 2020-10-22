using System;
using PPeX;

namespace PPeXM64
{
	/// <summary>
	/// A file entry that is used for indexing subfiles.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{Archive}: {File}")]
	public struct FileEntry
	{
		/// <summary>
		/// The archive that the subfile belongs to.
		/// </summary>
		public string Archive;

		/// <summary>
		/// The name of the subfile.
		/// </summary>
		public string File;

		public FileEntry(string archive, string file)
		{
			Archive = archive.Replace(".pp", "").Trim();
			File = file.ToLower().Trim();
		}

		public FileEntry(ArchiveSubfile subfile)
		{
			Archive = subfile.ArchiveName.Replace(".pp", "");
			File = subfile.EmulatedName.ToLower().Trim();
		}

		public override bool Equals(object obj)
		{
			return obj is FileEntry other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Archive, File);
		}

		public bool Equals(FileEntry other)
		{
			return Archive == other.Archive && File == other.File;
		}

		public static bool operator ==(FileEntry left, FileEntry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FileEntry left, FileEntry right)
		{
			return !left.Equals(right);
		}
	}
}