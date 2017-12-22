using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Eto.Forms;
using Pablo.Interface;

namespace PabloDraw
{
	[MonoMac.Foundation.Register("AppDelegate")]
	public partial class AppDelegate : Eto.Platform.Mac.AppDelegate
	{
		string fileName;
		
		public override void DidFinishLaunching (NSNotification notification)
		{
			base.DidFinishLaunching (notification);
			if (fileName != null)
			{
				var form = (Main)Eto.Forms.Application.Instance.MainForm;
				form.LoadFile(fileName, true);
				form.Show();
				fileName = null;
			}
		}

		public override bool ApplicationShouldOpenUntitledFile (NSApplication sender)
		{
			return true;
		}
		
		public override bool ApplicationOpenUntitledFile (NSApplication sender)
		{
			return true;
		}
		
		public override bool OpenFile (NSApplication sender, string filename)
		{
			var form = Eto.Forms.Application.Instance.MainForm as Main;
			if (form != null) {
				form.Show();
				return form.LoadFile(filename, true);
			}
			else this.fileName = filename;
			return true;
		}
		
		public override bool OpenFileWithoutUI (NSObject sender, string filename)
		{
			this.fileName = filename;
			return true;
		}
	}
}

