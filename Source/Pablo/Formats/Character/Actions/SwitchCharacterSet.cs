using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{

	public class SwitchCharacterSet : Command
	{
		readonly int characterSet;
		
		public SwitchCharacterSet(CharacterHandler handler, int characterSet, Keys accelerator)
		{
			Handler = handler;
			this.characterSet = characterSet;
			ID = "character_switchCharSet" + characterSet;
			MenuText = ToolBarText = "Select Character Set";
			ToolTip = "Select Character Set";
			Shortcut = accelerator;
		}

		public CharacterHandler Handler {
			get; private set;
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			Handler.CharacterSet = characterSet;
		}
	}
}
