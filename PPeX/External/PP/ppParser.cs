using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;

namespace PPeX.External.PP
{
	public class ppParser
	{
		public string FilePath { get; protected set; }
		public List<IWriteFile> Subfiles { get; protected set; }

		private string destPath;
		private bool keepBackup;
		private string backupExt;


        public ppParser(string path)
		{
			FilePath = path;
			using (FileStream stream = File.OpenRead(path))
				Subfiles = ppHeader_Wakeari.ReadHeader(stream);
		}

        public ppParser(FileStream stream)
		{
			FilePath = stream.Name;
			Subfiles = ppHeader_Wakeari.ReadHeader(stream);
		}

		public BackgroundWorker WriteArchive(string destPath, bool keepBackup, string backupExtension, bool background)
		{
			this.destPath = destPath;
			this.keepBackup = keepBackup;
			backupExt = backupExtension;

			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;
			worker.WorkerReportsProgress = true;

			worker.DoWork += new DoWorkEventHandler(writeArchiveWorker_DoWork);

			if (!background)
			{
				writeArchiveWorker_DoWork(worker, new DoWorkEventArgs(null));
			}

			return worker;
		}

		void writeArchiveWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;
			string backup = null;

			string dirName = Path.GetDirectoryName(destPath);
			if (dirName == String.Empty)
			{
				dirName = @".\";
			}
			DirectoryInfo dir = new DirectoryInfo(dirName);
			if (!dir.Exists)
			{
				dir.Create();
			}

			if (File.Exists(destPath))
			{
				backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", backupExt);
				File.Move(destPath, backup);

				if (destPath.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase))
				{
					for (int i = 0; i < Subfiles.Count; i++)
					{
						ppSubfile subfile = Subfiles[i] as ppSubfile;
						/*if ((subfile != null) && subfile.ppPath.Equals(this.FilePath, StringComparison.InvariantCultureIgnoreCase))
						{
							subfile.ppPath = backup;
						}*/
					}
				}
			}

			try
			{
				using (BinaryWriter writer = new BinaryWriter(File.Create(destPath)))
				{
					writer.BaseStream.Seek(ppHeader_Wakeari.HeaderSize(Subfiles.Count), SeekOrigin.Begin);
					uint offset = (uint)writer.BaseStream.Position;
					uint[] sizes = new uint[Subfiles.Count];
					object[] metadata = new object[Subfiles.Count];

					using (BinaryReader reader = new BinaryReader(File.OpenRead(backup != null ? backup : FilePath)))
					{
						for (int i = 0; i < Subfiles.Count; i++)
						{
							if (worker.CancellationPending)
							{
								e.Cancel = true;
								break;
							}

							worker.ReportProgress(i * 100 / Subfiles.Count);

							if (Subfiles[i] is ppSubfile subfile)
							{
								reader.BaseStream.Seek(subfile.offset, SeekOrigin.Begin);

								uint readSteps = (uint)(subfile.size / (double)Utility.BufSize);
								for (int j = 0; j < readSteps; j++)
								{
									writer.Write(reader.ReadBytes(Utility.BufSize));
								}
								writer.Write(reader.ReadBytes((int)(subfile.size % Utility.BufSize)));

								metadata[i] = subfile.Metadata;
							}
							else
							{
								Stream stream = ppFormat_AA2.WriteStream(writer.BaseStream);
								Subfiles[i].WriteTo(stream);
								metadata[i] = ppFormat_AA2.FinishWriteTo(stream);
							}

							uint pos = (uint)writer.BaseStream.Position;
							sizes[i] = pos - offset;
							offset = pos;
						}
					}

					if (!e.Cancel)
					{
						writer.BaseStream.Seek(0, SeekOrigin.Begin);
						ppHeader_Wakeari.WriteHeader(writer.BaseStream, Subfiles, sizes, metadata);
						offset = (uint)writer.BaseStream.Position;
						for (int i = 0; i < Subfiles.Count; i++)
						{
							if (Subfiles[i] is ppSubfile subfile)
							{
								subfile.offset = offset;
								subfile.size = sizes[i];
							}
							offset += sizes[i];
						}
					}
				}

				if (e.Cancel)
				{
					RestoreBackup(destPath, backup);
				}
				else
				{
					if (destPath.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase))
					{
						for (int i = 0; i < Subfiles.Count; i++)
						{
							ppSubfile subfile = Subfiles[i] as ppSubfile;
							/*if ((subfile != null) && subfile.ppPath.Equals(backup, StringComparison.InvariantCultureIgnoreCase))
							{
								subfile.ppPath = this.FilePath;
							}*/
						}
					}
					else
					{
						FilePath = destPath;
					}

					if ((backup != null) && !keepBackup)
					{
						File.Delete(backup);
					}
				}
			}
			catch
			{
				RestoreBackup(destPath, backup);
			}
		}

		void RestoreBackup(string destPath, string backup)
		{
			if (File.Exists(destPath) && File.Exists(backup))
			{
				File.Delete(destPath);

				if (backup != null)
				{
					File.Move(backup, destPath);

					if (destPath.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase))
					{
						for (int i = 0; i < Subfiles.Count; i++)
						{
							ppSubfile subfile = Subfiles[i] as ppSubfile;
							/*if ((subfile != null) && subfile.ppPath.Equals(backup, StringComparison.InvariantCultureIgnoreCase))
							{
								subfile.ppPath = this.FilePath;
							}*/
						}
					}
				}
			}
		}
	}
}
