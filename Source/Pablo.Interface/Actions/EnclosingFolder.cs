using System;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class EnclosingFolder : ButtonAction
	{
		Main main;
		public const string ActionID = "EnclosingFolder";

		public EnclosingFolder (Main main)
		{
			this.main = main;
			this.ID = ActionID;
			this.MenuText = "Enclosing Folder";
			this.ToolBarText = "Up";
			this.TooltipText = "Opens the parent folder";
			this.Accelerator = Command.CommonModifier | Key.Shift | Key.Up;
		}

		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);

			main.FileList.GoToParent ();
		}
	}
}

