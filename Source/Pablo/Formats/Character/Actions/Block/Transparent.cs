using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Transparent : Command
	{
		Selection tool;
		public const string ActionID = "character_block_transparent";

		public Transparent(Selection tool)
		{
			this.tool = tool;
			ID = ActionID;
			MenuText = "&Transparent";
			ToolBarText = "Transparent";
			ToolTip = "Make the block transparent when pasting";
			Shortcut = Keys.T;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Paste; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			// Stamp the selected region
			
			if (tool.PasteMode == PasteMode.Transparent)
				tool.PasteMode = PasteMode.Normal;
			else
				tool.PasteMode = PasteMode.Transparent;
		}
	}
}

