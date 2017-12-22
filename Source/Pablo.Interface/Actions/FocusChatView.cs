using System;
using Eto.Forms;
using Eto;

namespace Pablo.Interface.Actions
{
	public class FocusChatView : ButtonAction
	{
		Main main;
		public const string ActionID = "focusChatView";
		
		public FocusChatView (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			if (main.Generator.IsMac)
				this.Accelerator = Key.Control | Key.C;
			else
				this.Accelerator = Key.Alt | Key.C;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			
			if (main.ChatPanel != null)
			{
				if (!main.ChatPanel.SetChatFocus ())
					main.Content.Focus ();
			}
		}
	}
}

