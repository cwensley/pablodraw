using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class FlipX : Command
	{
		Selection tool;
		public FlipX(Selection tool)
		{
			this.tool = tool;
			ID = "character_FlipX";
			MenuText = "Flip &X";
			ToolBarText = "Flip X";
			ToolTip = "Flip the block horizontally";
			Shortcut = Keys.X;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Paste; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			Canvas canvas = tool.PasteCanvas;
			
			canvas.FlipX(tool.Handler.Info.FlipX);
			tool.Handler.InvalidateCharacterRegion(new Rectangle(tool.Handler.CursorPosition, canvas.Size), false);
		}
	}
}

