using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class FlipX : ButtonAction
	{
		Selection tool;
		public FlipX(Selection tool)
		{
			this.tool = tool;
			ID = "character_FlipX";
			Text = "Flip &X|Flip X|Flip the block horizontally";
			Accelerator = Key.X;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			
			Canvas canvas = tool.PasteCanvas;
			
			canvas.FlipX(tool.Handler.Info.FlipX);
			tool.Handler.InvalidateCharacterRegion(new Rectangle(tool.Handler.CursorPosition, canvas.Size), false);
		}
	}
}

