using Eto.Forms;
using System;

namespace Pablo.Formats.Character.Actions
{
	public class DefaultColour : ButtonAction
	{
		CharacterHandler handler;
		public const string ActionID = "character_defaultColour";
		
		public DefaultColour(CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			Text = "Default Colour|Default Colour|Set the foreground and background to the default colours";
			Accelerator = Command.CommonModifier | Key.D;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			handler.DrawAttribute = new Attribute(7, 0);
		}
	}
}
