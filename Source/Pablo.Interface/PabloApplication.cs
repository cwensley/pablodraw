#if DEBUG

//#define TWO_WINDOWS

#endif

using System;
using Eto;
using Eto.Forms;
using System.Reflection;
using Eto.IO;
using System.ComponentModel;
using System.Diagnostics;
using Eto.Drawing;
using System.Text;


namespace Pablo.Interface
{
	public class PabloApplication : Eto.Forms.Application
	{
		public Main Main { get; private set; }
#if TWO_WINDOWS
		static Main main2;
#endif

		public PabloApplication()
			: this(Platform.Detect)
		{
		}

		public PabloApplication(Platform platform)
			: base(platform)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			
			this.Name = "PabloDraw";
			this.Style = "application";
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".rar");
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".7z");
			//EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".tar.gz");
			//EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".tgz");
			//EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".bz2");
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".zip");
		}

		protected override void OnInitialized(EventArgs e)
		{
			this.Main = new Main();
			base.OnInitialized(e);
			this.MainForm = Main;
			this.MainForm.Show();
#if TWO_WINDOWS
			main2 = new Main();
			main2.Location = this.MainForm.Location + new Size(50, 50);
			main2.Show ();
#endif
		}

		protected override void OnTerminating(CancelEventArgs e)
		{
			base.OnTerminating(e);

			if (Dialogs.FileModifiedDialog.Show(this.Main) == DialogResult.Cancel)
				e.Cancel = true;
			if (!e.Cancel)
			{
				this.Main.WriteXml();
				this.Main.SetDocument(null);
			}
		}

	}
}
