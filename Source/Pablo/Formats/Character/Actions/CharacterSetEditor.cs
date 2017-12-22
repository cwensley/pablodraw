using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;

namespace Pablo.Formats.Character.Actions
{
	public class CharacterSetEditor : Command
	{
		public const string ActionID = "character_charsetEditor";
		CharacterHandler handler;

		public CharacterSetEditor(CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			MenuText = "Edit Character Set...";
			ToolBarText = "CharSet";
			ToolTip = "Edit the character sets";
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			var dlg = new Controls.CharacterSetEditor(handler);
			
			dlg.ShowModal(handler.Viewer as Control);
		}
	}
}

