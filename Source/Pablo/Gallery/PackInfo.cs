using System;
using Newtonsoft.Json;

namespace Pablo.Gallery
{
	[JsonObject(MemberSerialization.OptIn)]
	public class PackInfo
	{
		[JsonProperty("filename")]
		public string FileName { get; set; }
		
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}

