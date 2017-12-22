using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Under : Command
	{
		Selection tool;
		public const string ActionID = "character_block_Under";

		public Under(Selection tool)
		{
			this.tool = tool;
			ID = ActionID;
			MenuText = "&Under";
			ToolBarText = "Under";
			ToolTip = "Paste the block under current drawing";
			Shortcut = Keys.U; // TODO: = new [] { Keys.U, Keys.O };
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Paste; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			if (tool.PasteMode == PasteMode.Under)
				tool.PasteMode = PasteMode.Normal;
			else
				tool.PasteMode = PasteMode.Under;
		}
	}
}

