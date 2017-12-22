using System;
using Eto.IO;
using System.IO;
using System.Net;

namespace Pablo.Gallery
{
	public class GalleryFile : EtoFileInfo
	{
		public FileInfo FileInfo { get; set; }
		MemoryStream fileStream;
		
		EtoDirectoryInfo parent;
		
		public override string FullName {
			get {
				return FileInfo.FileName;
			}
		}
		
		public override bool ReadOnly {
			get {
				return true;
			}
		}
		
		public GalleryFile (EtoDirectoryInfo parent, FileInfo fileInfo)
		{
			this.FileInfo = fileInfo;
			this.parent = parent;
		}
		
		public override EtoDirectoryInfo Directory {
			get {
				return parent;
			}
		}
		
		public override void Delete ()
		{
			throw new NotImplementedException ();
		}

		#region implemented abstract members of Eto.IO.EtoFileInfo
		
		public override Stream Open (FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (fileStream != null) {
				fileStream.Seek(0, SeekOrigin.Begin);
				var ms = new MemoryStream((int)fileStream.Length);
				fileStream.CopyTo(ms);
				ms.Seek(0, SeekOrigin.Begin);
				return ms;
			}
			Console.WriteLine("16c: Downloading file {0}", GalleryBrowser.BasePath + FileInfo.DownloadUrl);
			var req = WebRequest.Create(GalleryBrowser.BasePath + FileInfo.DownloadUrl);
			using (var response = (HttpWebResponse)req.GetResponse())
			using (var stream = response.GetResponseStream())
			{
				fileStream = new MemoryStream((int)response.ContentLength);
				stream.CopyTo(fileStream);
				fileStream.Seek(0, SeekOrigin.Begin);
				var ms = new MemoryStream((int)fileStream.Length);
				fileStream.CopyTo(ms);
				ms.Seek(0, SeekOrigin.Begin);
				return ms;
			}
		}
		
		#endregion
	}
}

