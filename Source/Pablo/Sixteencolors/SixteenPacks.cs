using System;
using Eto.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace Pablo.Sixteencolors
{
	public class SixteenPacks : GenericDirectory<SixteenFiles>
	{
		public override string ApiPath {
			get { return SixteenBrowser.API_PATH + "/year/" + Info.Year + "?rows=0"; }
		}

		public override string Name {
			get { return string.Format ("{0} ({1} packs)", Info.Year, Info.Packs); }
		}
		
		public override string FullName {
			get { return Name; }
		}
		
		public YearInfo Info { get; private set; }
		
		public SixteenPacks(EtoDirectoryInfo parent, YearInfo year)
			: base(parent)
		{
			this.Info = year;
		}
		
		protected override IEnumerable<SixteenFiles> GetEntries ()
		{
			var packs = JsonConvert.DeserializeObject<IList<PackInfo>>(LoadJsonString());
			foreach (var pack in packs)
			{
				yield return new SixteenFiles(this, pack);
			}
		}

	}
}

