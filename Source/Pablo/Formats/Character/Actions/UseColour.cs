using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{
	public class UseColour : Command
	{
		public UseColour(CharacterHandler handler)
		{
			this.Handler = handler;
			ID = "character_useColour";
			MenuText = ToolBarText = ToolTip = "Use colour under cursor";
			Shortcut = Keys.Alt | Keys.U;
		}

		public CharacterHandler Handler
		{
			get;
			private set;
		}

		public override bool Enabled
		{
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			Canvas canvas = Handler.CurrentPage.Canvas;
			Handler.DrawAttribute = canvas[Handler.CursorPosition].Attribute;
		}
	}
}
