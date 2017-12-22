using System;
using Eto.IO;
using System.IO;
using System.Threading;

namespace Pablo.WebDav
{
	public class WebDavFile : EtoFileInfo
	{
		WebDavDirectory directory;
		Stream stream;
		
		public override string Name {
			get { return FileName; }
		}
		
		public string FileName { get; private set; }
		
		public override string FullName {
			get { return directory.FullName + FileName; }
		}
		
		public WebDavFile (WebDavDirectory directory, string file)
		{
			this.directory = directory;
			this.FileName = file;
		}

		public override Stream Open (FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (this.stream == null) {
				var finished = new ManualResetEvent (false);
				var client = directory.CreateClient ();
				client.DownloadComplete += delegate(Stream stream, int statusCode) {
					this.stream = stream;
					finished.Set ();
				};
				client.Download (FileName);
				finished.WaitOne ();
			}
			this.stream.Position = 0;
			var result = new MemoryStream((int)this.stream.Length);
			this.stream.CopyTo (result);
			result.Position = 0;
			return result;
		}

		public override void Delete ()
		{
		}

		public override bool ReadOnly {
			get {
				return true;
			}
		}

		public override EtoDirectoryInfo Directory {
			get { return directory; }
		}
	}
}

