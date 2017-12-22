using Eto.Forms;
using Eto.Drawing;
using System;

namespace Pablo.Formats.Character.Actions
{

	public class DrawCharacterSetChar : DrawCharacter
	{
		protected override int Character {
			get {
				var handler = Handler as CharacterHandler;
				return handler.Info.GetCharacter(handler.CharacterSet, base.Character);
			}
			set {
				base.Character = value;
			}
		}
		
		public new CharacterHandler Handler {
			get { return base.Handler as CharacterHandler; }
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}
		
		public DrawCharacterSetChar(CharacterHandler handler, int character, Keys accelerator)
			: base(handler)
		{
			base.Character = character;
			ID = "character_drawChar" + character;
			MenuText = "Draw Character";
			ToolTip = "Draws a character";
			Shortcut = accelerator;
		}

	}
}
