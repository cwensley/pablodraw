using System;
using Eto.IO;
using Eto.Forms;

namespace Pablo.Mobile
{
	public class PabloApplication : Application
	{
		public PabloApplication (Eto.Platform platform = null)
			: base(platform ?? Eto.Platform.Detect)
		{
			this.Style = "pablo";
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".rar");
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".zip");
		}

		protected override void OnInitialized (EventArgs e)
		{
			base.OnInitialized (e);
			
			MainForm = new Main();
			MainForm.Show();
		}
	}
}

