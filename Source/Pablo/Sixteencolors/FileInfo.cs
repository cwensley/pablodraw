using System;
using Newtonsoft.Json;

namespace Pablo.Sixteencolors
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FileInfo
	{
		[JsonProperty("filename")]
		public string FileName { get; set; }
		
		[JsonProperty("file_location")]
		public string DownloadUrl { get; set; }
		
	}
}

