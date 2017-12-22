using System;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{
	public class SwitchForegroundBackground : ButtonAction
	{
		public const string ActionID = "character_switchFgBg";
		
		CharacterHandler handler;

		public SwitchForegroundBackground (CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			Text = "Switch Foreground / Background|Switch FG/BG|Switch the currently selected foreground and background colours";
			this.Accelerator = Command.CommonModifier | Key.Shift | Key.X;
		}
        
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
            
			var attr = handler.DrawAttribute;
			var fg = attr.Foreground;
			attr.Foreground = attr.Background;
			attr.Background = fg;
			
			handler.DrawAttribute = attr;
		}
	}
}

