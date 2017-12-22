using System;
using Eto.IO;
using System.Collections.Generic;
using System.Threading;

namespace Pablo.WebDav
{
	public class WebDavDirectory : EtoDirectoryInfo
	{
		EtoDirectoryInfo parent;
		List<string> files;
		
		public string Path { get; private set; }
		public string Url { get; private set; }
		
		public override string Name {
			get { return Path.Trim ('/'); }
		}
		
		public override string FullName {
			get {
				return CreateClient ().FullPath;
			}
		}
		
		public WebDavClient CreateClient ()
		{
			return new WebDavClient{
				BasePath = Path,
				Server = Url
			};
		}

		public WebDavDirectory (EtoDirectoryInfo parent, string url, string path = "/")
		{
			this.parent = parent;
			this.Path = path;
			this.Url = url;
		}
		
		void LoadFiles ()
		{
			if (this.files != null)
				return;
			var finished = new ManualResetEvent (false);
			var client = CreateClient ();
			client.ListComplete += delegate(List<string> files, int statusCode) {
				this.files = files;
				finished.Set ();
			};
			client.List ();
			finished.WaitOne ();
		}
		
		protected override IEnumerable<EtoDirectoryInfo> GetPathDirectories ()
		{
			LoadFiles ();
			if (files != null) {
				foreach (var file in files) {
					if (file.EndsWith ("/") && !file.Trim ('/').StartsWith (".")) {
						yield return new WebDavDirectory(this, this.Url, this.Path.TrimEnd ('/') + file);
					}
				}
			}
		}
		
		public override IEnumerable<EtoFileInfo> GetFiles ()
		{
			LoadFiles ();
			if (files != null) {
				foreach (var file in files) {
					if (!file.EndsWith ("/")) {
						yield return new WebDavFile(this, file);
					}
				}
			}
		}

		public override EtoDirectoryInfo Parent {
			get { return parent; }
		}
	}
}

