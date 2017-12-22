using System;
using Eto.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace Pablo.Gallery
{
	public class GalleryPacks : GenericDirectory<GalleryFiles>
	{
		public override string ApiPath {
			get { return GalleryBrowser.BaseApiPath + "/year/" + Info.Year + "?rows=0"; }
		}

		public override string Name {
			get { return string.Format ("{0} ({1} packs)", Info.Year, Info.Packs); }
		}
		
		public override string FullName {
			get { return Name; }
		}
		
		public YearInfo Info { get; private set; }
		
		public GalleryPacks(EtoDirectoryInfo parent, YearInfo year)
			: base(parent)
		{
			this.Info = year;
		}
		
		protected override IEnumerable<GalleryFiles> GetEntries ()
		{
			var packs = JsonConvert.DeserializeObject<IList<PackInfo>>(LoadJsonString());
			foreach (var pack in packs)
			{
				yield return new GalleryFiles(this, pack);
			}
		}

	}
}

