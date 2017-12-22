using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Eto.Forms;

namespace Pablo.Gallery
{
	[JsonObject(MemberSerialization.OptIn)]
	public class YearInfo
	{
		[JsonProperty("packs")]
		public int Packs { get; set; }
		
		[JsonProperty("year")]
		public int Year { get; set; }
	}
}

