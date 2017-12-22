using System;
using Eto.IO;

namespace Pablo.WebDav
{
	public class WebDavBrowser : WebDavDirectory
	{
		string name;
		
		public override string Name {
			get {
				return name;
			}
		}
		
		public WebDavBrowser (EtoDirectoryInfo parent, string name, string url)
			: base(parent, url)
		{
			this.name = name;
		}
	}
}

