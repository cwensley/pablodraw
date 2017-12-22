using System;
using Eto.Forms;
using Eto;

namespace Pablo.Interface.Actions
{
	public class FocusChatView : Command
	{
		Main main;
		public const string ActionID = "focusChatView";
		
		public FocusChatView (Main main)
		{
			this.main = main;
			base.ID = ActionID;
			if (main.Platform.IsMac)
				this.Shortcut = Keys.Control | Keys.C;
			else
				this.Shortcut = Keys.Alt | Keys.C;
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);

			if (main.ChatPanel != null)
			{
				if (!main.ChatPanel.SetChatFocus ())
					main.Content.Focus ();
			}
		}
	}
}

