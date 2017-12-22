using System;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class About : ButtonAction
	{
		public Main Main { get; private set; }
		
		public const string ActionID = "about";
		
		public About (Main main)
		{
			this.Main = main;
			base.ID = ActionID;
			this.Text = "&About PabloDraw|About|About PabloDraw.NET";
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			new Dialogs.About ().ShowDialog (Main);
		}
	}
}

