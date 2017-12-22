using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Pablo.Gallery
{
	[JsonObject(MemberSerialization.OptIn)]
	public class PackDetails
	{
		[JsonProperty("pack_file_location")]
		public string DownloadUrl { get; set; }
		
		[JsonProperty("files")]
		public List<FileInfo> Files { get; set; }
	}
}

