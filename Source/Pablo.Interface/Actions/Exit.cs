using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Interface.Actions
{
	public class Exit : Command
	{
		public Main Main { get; private set; }
		
		public const string ActionID = "exit";
		
		public Exit (Main main)
		{
			this.Main = main;
			this.ID = ActionID;
			this.MenuText = "E&xit";
			this.ToolTip = "Exit the application";
			this.Image = ImageCache.IconFromResource("Pablo.Interface.Icons.close.ico");
			this.Shortcut = Keys.X | Keys.Alt;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);

			Main.ExitApplication ();
		}
	}
}

