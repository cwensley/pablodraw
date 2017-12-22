using System;
using Eto.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pablo.Sixteencolors
{
	public class SixteenFiles : GenericDirectory<SixteenFiles, SixteenFile>
	{
		public PackInfo Info { get; set; }
		
		public override string FullName {
			get { return Info.FileName; }
		}
		
		public override string Name {
			get { return Info.Name; }
		}
		
		public SixteenFiles (EtoDirectoryInfo parent, PackInfo packInfo)
			: base(parent)
		{
			this.Info = packInfo;
		}

		#region implemented abstract members of Pablo.Sixteencolors.GenericDirectory[SixteenFiles]
		
		public override string ApiPath {
			get { return SixteenBrowser.API_PATH + "/pack/" + Info.Name + "?rows=0"; }
		}

		protected override IEnumerable<SixteenFiles> GetEntries ()
		{
			yield break;
		}
		
		public override IEnumerable<SixteenFile> GetFileEntries ()
		{
			var details = JsonConvert.DeserializeObject<PackDetails>(LoadJsonString());
			foreach (var file in details.Files)
			{
				yield return new SixteenFile(this, file);
			}
		}
		#endregion
	}
}

