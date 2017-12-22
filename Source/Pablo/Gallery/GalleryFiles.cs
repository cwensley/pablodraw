using System;
using Eto.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pablo.Gallery
{
	public class GalleryFiles : GenericDirectory<GalleryFiles, GalleryFile>
	{
		public PackInfo Info { get; set; }

		public override string FullName
		{
			get { return Info.FileName; }
		}

		public override string Name
		{
			get { return Info.Name; }
		}

		public GalleryFiles(EtoDirectoryInfo parent, PackInfo packInfo)
			: base(parent)
		{
			this.Info = packInfo;
		}

		public override string ApiPath
		{
			get { return GalleryBrowser.BaseApiPath + "/pack/" + Info.Name + "?rows=0"; }
		}

		protected override IEnumerable<GalleryFiles> GetEntries()
		{
			yield break;
		}

		public override IEnumerable<GalleryFile> GetFileEntries()
		{
			var details = JsonConvert.DeserializeObject<PackDetails>(LoadJsonString());
			foreach (var file in details.Files)
			{
				yield return new GalleryFile(this, file);
			}
		}
	}
}

