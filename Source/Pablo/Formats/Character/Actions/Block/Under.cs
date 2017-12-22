using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Under : ButtonAction
	{
		Selection tool;
		public const string ActionID = "character_block_Under";
		
		public Under(Selection tool)
		{
			this.tool = tool;
			ID = ActionID;
			Text = "&Under|Under|Toggle under mode|Paste the block under current drawing";
			Accelerators = new Key[] { Key.U, Key.O };
		}
		
		protected override void OnActivated (EventArgs e)
		{
			if (tool.PasteMode == PasteMode.Under)
				tool.PasteMode = PasteMode.Normal;
			else 
				tool.PasteMode = PasteMode.Under;
		}
	}
}

