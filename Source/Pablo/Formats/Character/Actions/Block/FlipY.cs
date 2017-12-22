using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class FlipY : Command
	{
		Selection tool;
		public FlipY(Selection tool)
		{
			this.tool = tool;
			ID = "character_FlipY";
			MenuText = "Flip &Y";
			ToolBarText = "Flip Y";
			ToolTip = "Flip the block vertically";
			Shortcut = Keys.Y;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Paste; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			Canvas canvas = tool.PasteCanvas;
			
			canvas.FlipY(tool.Handler.Info.FlipY);
			tool.Handler.InvalidateCharacterRegion(new Rectangle(tool.Handler.CursorPosition, canvas.Size), false);
		}
	}
}

