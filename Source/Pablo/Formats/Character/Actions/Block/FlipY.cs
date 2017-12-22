using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class FlipY : ButtonAction
	{
		Selection tool;
		public FlipY(Selection tool)
		{
			this.tool = tool;
			ID = "character_FlipY";
			Text = "Flip &Y|Flip Y|Flip the block vertically";
			Accelerator = Key.Y;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			
			Canvas canvas = tool.PasteCanvas;
			
			canvas.FlipY(tool.Handler.Info.FlipY);
			tool.Handler.InvalidateCharacterRegion(new Rectangle(tool.Handler.CursorPosition, canvas.Size), false);
		}
	}
}

