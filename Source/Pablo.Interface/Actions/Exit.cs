using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Interface.Actions
{
	public class Exit : ButtonAction
	{
		public Main Main { get; private set; }
		
		public const string ActionID = "exit";
		
		public Exit (Main main)
		{
			this.Main = main;
			this.ID = ActionID;
			this.Text = "E&xit|Exit|Exit the application";
			this.Image = Icon.FromResource ("Pablo.Interface.Icons.close.ico");
			this.Accelerator = Key.X | Key.Alt;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			
			Main.ExitApplication ();
		}
	}
}

