using System;
using SharpCompress.Reader;
using SharpCompress.Archive;
using System.IO;
using System.Collections.Generic;
using Eto.IO;
using SharpCompress.Common.Rar;

namespace Pablo
{
	public class SharpCompressDirectoryInfo : VirtualDirectoryInfo
	{
		public SharpCompressDirectoryInfo(EtoFileInfo fileInfo)
			: base(fileInfo)
		{
		}

		private SharpCompressDirectoryInfo(SharpCompressDirectoryInfo parent, string path)
			: base(parent, path)
		{
		}

		protected override VirtualDirectoryInfo CreateDirectory(VirtualDirectoryInfo parent, string path)
		{
			return new SharpCompressDirectoryInfo((SharpCompressDirectoryInfo)parent, path);
		}

		protected override IEnumerable<VirtualFileEntry> ReadEntries(Stream stream)
		{
			/**
			using (var reader = ReaderFactory.Open(stream, SharpCompress.Common.Options.None))
			{
				while (reader.MoveToNextEntry())
				{
					var entry = reader.Entry;
					yield return new VirtualFileEntry(entry.FilePath.TrimEnd(Path.DirectorySeparatorChar), entry.IsDirectory);
				}
			}
			/**/
			using (var archive = ArchiveFactory.Open(stream, SharpCompress.Common.Options.LookForHeader))
			{
				foreach (var entry in archive.Entries)
				{
					yield return new VirtualFileEntry(entry.FilePath.TrimEnd(Path.DirectorySeparatorChar), entry.IsDirectory);
				}
			}
			/**/
		}

		public override Stream OpenRead(string fileName)
		{
			MemoryStream ms = null;
			/**
			using (var stream = FileInfo.OpenRead())
			using (var reader = ReaderFactory.Open(stream, SharpCompress.Common.Options.None))
			{
				while (reader.MoveToNextEntry())
				{
					var entry = reader.Entry;
					if (!entry.IsDirectory && string.Equals(entry.FilePath, fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						ms = new MemoryStream((int)entry.Size);
						reader.WriteEntryTo(ms);
						ms.Position = 0;
						break;
					}
				}
			}
			/**/
			using (var stream = FileInfo.OpenRead())
			using (var archive = ArchiveFactory.Open(stream, SharpCompress.Common.Options.None))
			{
				foreach (var entry in archive.Entries)
				{
					if (!entry.IsDirectory && string.Equals(entry.FilePath, fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						ms = new MemoryStream((int)entry.Size);
						entry.WriteTo(ms);
						ms.Position = 0;
						break;
					}
				}
			}
			/**/
			return ms;
		}
	}
}
