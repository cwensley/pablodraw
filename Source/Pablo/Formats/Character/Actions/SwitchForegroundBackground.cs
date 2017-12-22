using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{
	public class SwitchForegroundBackground : Command
	{
		public const string ActionID = "character_switchFgBg";
		CharacterHandler handler;

		public SwitchForegroundBackground(CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			MenuText = "Switch Foreground / Background";
			ToolBarText = "Switch FG/BG";
			ToolTip = "Switch the currently selected foreground and background colours";
			Shortcut = PabloCommand.CommonModifier | Keys.Shift | Keys.X;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			var attr = handler.DrawAttribute;
			var fg = attr.Foreground;
			attr.Foreground = attr.Background;
			attr.Background = fg;
			
			handler.DrawAttribute = attr;
		}
	}
}

