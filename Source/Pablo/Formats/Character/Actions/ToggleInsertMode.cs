using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;

namespace Pablo.Formats.Character.Actions
{
	public class ToggleInsertMode : CheckCommand
	{
		public const string ActionID = "character_toggleInsert";

		public ToggleInsertMode(CharacterHandler handler)
		{
			this.Handler = handler;
			ID = ActionID;
			MenuText = "Toggle &Insert Mode";
			ToolBarText = "Insert";
			ToolTip = "Toggles insert mode on/off";
			Shortcut = Keys.Insert;
		}

		public CharacterHandler Handler {
			get; private set;
		}

		public override bool Enabled {
			get { 
				return Handler.AllowKeyboardEditing; 
			}
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);

			Handler.InsertMode = !Handler.InsertMode;
			this.Checked = Handler.InsertMode;
		}
	}
}
