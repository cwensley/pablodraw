using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;

namespace Pablo.Formats.Character.Actions
{
	public class ToggleInsertMode : CheckAction
	{
		public const string ActionID = "character_toggleInsert";

		public ToggleInsertMode(CharacterHandler handler)
		{
			this.Handler = handler;
			ID = ActionID;
			Text = "Toggle &Insert Mode|Insert|Toggle Insert Mode|Toggles insert mode on/off";
			Accelerator = Key.Insert;
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

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			Handler.InsertMode = !Handler.InsertMode;
			this.Checked = Handler.InsertMode;
		}
	}
}
