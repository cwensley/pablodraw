using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{

	public class SwitchCharacterSet : ButtonAction
	{
		int characterSet;
		
		public SwitchCharacterSet(CharacterHandler handler, int characterSet, Key accelerator)
		{
			this.Handler = handler;
			this.characterSet = characterSet;
			ID = "character_switchCharSet" + characterSet;
			Text = "Select Character Set";
			Description = "Select Character Set";
			Accelerator = accelerator;
		}

		public CharacterHandler Handler {
			get; private set;
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		protected override void OnActivated (EventArgs e)
		{
			Handler.CharacterSet = characterSet;
		}
	}
}
