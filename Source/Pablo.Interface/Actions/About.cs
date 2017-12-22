using System;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class About : Command
	{
		public Main Main { get; private set; }
		
		public const string ActionID = "about";
		
		public About (Main main)
		{
			this.Main = main;
			base.ID = ActionID;
			this.MenuText = "&About PabloDraw";
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			new Dialogs.About ().ShowModal(Main);
		}
	}
}

