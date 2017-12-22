using System;
using Eto.Forms;
namespace Pablo.Interface.Actions
{
	public class CloseWindow : Command
	{
		Main main;
		public const string ActionID = "closeWindow";
		
		public CloseWindow (Main main)
		{
			this.main = main;
			this.ID = ActionID;
			this.MenuText = "Close";
			this.ToolTip = "Closes this window";
			this.Shortcut = PabloCommand.CommonModifier | Keys.W;
			
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			main.Close();
		}
	}
}

