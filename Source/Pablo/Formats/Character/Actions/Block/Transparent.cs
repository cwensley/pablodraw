using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Transparent : ButtonAction
	{
		Selection tool;
		public const string ActionID = "character_block_transparent";
		
		public Transparent(Selection tool)
		{
			this.tool = tool;
			ID = ActionID;
			Text = "&Transparent|Transparent|Toggle transparent mode|Make the block transparent when pasting";
			Accelerator = Key.T;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			// Stamp the selected region
			
			if (tool.PasteMode == PasteMode.Transparent)
				tool.PasteMode = PasteMode.Normal;
			else 
				tool.PasteMode = PasteMode.Transparent;
		}
	}
}

