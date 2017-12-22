using System;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class EnclosingFolder : Command
	{
		Main main;
		public const string ActionID = "EnclosingFolder";

		public EnclosingFolder (Main main)
		{
			this.main = main;
			this.ID = ActionID;
			this.MenuText = "Enclosing Folder";
			this.ToolBarText = "Up";
			this.ToolTip = "Opens the parent folder";
			this.Shortcut = PabloCommand.CommonModifier | Keys.Shift | Keys.Up;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);

			main.FileList.GoToParent ();
		}
	}
}

