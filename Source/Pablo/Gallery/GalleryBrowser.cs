using System;
using Eto.IO;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Pablo.Gallery
{
	public class GalleryBrowser : GenericDirectory<GalleryPacks>
	{
		public const string BasePath = "http://gallery.picoe.ca";
		public const string BaseApiPath = "http://gallery.picoe.ca/api/v0";
		
		public override string ApiPath {
			get { return BaseApiPath + "/year?rows=0"; }
		}
		
		public override string FullName {
			get { return "Gallery"; }
		}
		
		public override string Name {
			get { return "Gallery"; }
		}
		
		
		public GalleryBrowser(EtoDirectoryInfo parent)
			: base(parent)
		{
		}
		
		protected override IEnumerable<GalleryPacks> GetEntries ()
		{
			var json = LoadJsonString();
			var years = JsonConvert.DeserializeObject<List<YearInfo>>(json);
			foreach (var year in years)
			{
				yield return new GalleryPacks(this, year);
			}
		}
		
		public override IEnumerable<EtoFileInfo> GetFiles ()
		{
			yield break;
		}
		
	}
}

