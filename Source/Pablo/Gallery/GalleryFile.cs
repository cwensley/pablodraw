using System;
using Eto.IO;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

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
			using var client = new HttpClient();
			using var response = Task.Run(async () => await client.GetAsync(GalleryBrowser.BasePath + FileInfo.DownloadUrl)).GetAwaiter().GetResult();

			{
				fileStream = new MemoryStream((int)response.Content.Headers.ContentLength);
				response.Content.CopyTo(fileStream, null, CancellationToken.None);
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

