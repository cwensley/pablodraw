#if DEBUG

//#define TWO_WINDOWS

#endif

using System;
using Eto;
using Eto.Forms;
using Eto.Misc;
using System.Reflection;
using Eto.IO;
using System.ComponentModel;
using System.Diagnostics;
using Eto.Drawing;


namespace Pablo.Interface
{
	public class Application : Eto.Forms.Application
	{
		public Main Main { get; private set; }
#if TWO_WINDOWS
		static Main main2;
#endif
		
#if DEBUG
		static Application ()
		{
			Debug.Listeners.Add (new TextWriterTraceListener (System.Console.Out));
		}
#endif
		
		public Application ()
			: this (Generator.Detect)
		{
		}
		
		public Application(Generator g)
			: base(g)
		{
			this.Name = "PabloDraw";
			this.Style = "application";
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".rar");
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".7z");
			//EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".tar.gz");
			//EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".tgz");
			//EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".bz2");
			EtoDirectoryInfo.AddVirtualDirectoryType<SharpCompressDirectoryInfo>(".zip");
		}
	
		public override void OnInitialized (EventArgs e)
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
		
		public override void OnTerminating (CancelEventArgs e)
		{
			base.OnTerminating (e);
			
			if (Dialogs.FileModifiedDialog.Show (this.Main) == DialogResult.Cancel)
				e.Cancel = true;
			if (!e.Cancel) {
				this.Main.WriteXml ();
				this.Main.SetDocument (null);
			}
		}
		
	}
}
