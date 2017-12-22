using Eto.Forms;
using System;

namespace Pablo.Formats.Character.Actions
{
	public class DefaultColour : Command
	{
		CharacterHandler handler;
		public const string ActionID = "character_defaultColour";
		
		public DefaultColour(CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			MenuText = ToolBarText = "Default Colour";
			ToolTip = "Set the foreground and background to the default colours";
			Shortcut = PabloCommand.CommonModifier | Keys.D;
		}

		protected override void OnExecuted(EventArgs e)
		{
			handler.DrawAttribute = new Attribute(7, 0);
		}
	}
}
