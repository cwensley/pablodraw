using System;
using Eto.IO;
using Eto.Forms;

namespace Pablo.Mobile
{
	public class PabloApplication : Application
	{
		public PabloApplication ()
		{
			this.Style = "pablo";
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".rar");
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".zip");
		}

		public override void OnInitialized (EventArgs e)
		{
			base.OnInitialized (e);
			
			MainForm = new Main();
			MainForm.Show();
		}
	}
}

