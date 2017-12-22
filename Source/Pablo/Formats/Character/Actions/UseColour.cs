using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{

	public class UseColour : ButtonAction
	{
		public UseColour(CharacterHandler handler)
		{
			this.Handler = handler;
			ID = "character_useColour";
			Text = "Use colour under cursor";
			Description = "Use colour under cursor";
			Accelerator = Key.Alt | Key.U;
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
			Canvas canvas = Handler.CurrentPage.Canvas;
			Handler.DrawAttribute = canvas[Handler.CursorPosition].Attribute;
		}
		
	}
}
