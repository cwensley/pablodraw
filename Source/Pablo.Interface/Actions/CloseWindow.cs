using System;
using Eto.Forms;
namespace Pablo.Interface.Actions
{
	public class CloseWindow : ButtonAction
	{
		Main main;
		public const string ActionID = "closeWindow";
		
		public CloseWindow (Main main)
		{
			this.main = main;
			this.ID = ActionID;
			this.Text = "Close|Close|Closes this window";
			this.Accelerator = Command.CommonModifier | Key.W;
			
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			main.Close();
		}
	}
}

