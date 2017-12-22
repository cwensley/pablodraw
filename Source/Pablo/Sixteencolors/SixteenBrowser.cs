using System;
using Eto.IO;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Pablo.Sixteencolors
{
	public class SixteenBrowser : GenericDirectory<SixteenPacks>
	{
		public const string MAIN_PATH = "http://sixteencolors.net";
		public const string API_PATH = "http://api.sixteencolors.net/v0";
		
		public override string ApiPath {
			get { return API_PATH + "/year?rows=0"; }
		}
		
		public override string FullName {
			get { return "SixteenColors"; }
		}
		
		public override string Name {
			get { return "SixteenColors"; }
		}
		
		
		public SixteenBrowser(EtoDirectoryInfo parent)
			: base(parent)
		{
		}
		
		protected override IEnumerable<SixteenPacks> GetEntries ()
		{
			var json = LoadJsonString();
			var years = JsonConvert.DeserializeObject<List<YearInfo>>(json);
			foreach (var year in years)
			{
				yield return new SixteenPacks(this, year);
			}
		}
		
		public override IEnumerable<EtoFileInfo> GetFiles ()
		{
			yield break;
		}
		
	}
}

