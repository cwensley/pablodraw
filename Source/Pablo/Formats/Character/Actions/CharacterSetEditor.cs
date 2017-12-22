using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;

namespace Pablo.Formats.Character.Actions
{
	public class CharacterSetEditor : ButtonAction
	{
		public const string ActionID = "character_charsetEditor";
		
		CharacterHandler handler;

		public CharacterSetEditor (CharacterHandler handler)
		{
			this.handler = handler;
			ID = ActionID;
			Text = "Edit Character Set...|CharSet|Edit the character sets";
		}
        
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
            
			var dlg = new Controls.CharacterSetEditor(handler);
			
			dlg.ShowDialog (handler.Viewer as Control);
		}
	}
}

