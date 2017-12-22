using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Move : ButtonAction
	{
		Selection tool;
		public Move(Selection tool)
		{
			this.tool = tool;
			ID = "character_Move";
			Text = "&Move Block|Move|Move selected region|Moves the selected region";
			Accelerator = Key.M;
			Enabled = tool.Handler.Client == null || tool.Handler.Client.CurrentUser.Level >= UserLevel.Editor;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			if (tool.SelectedRegion == null)
				return;
			Rectangle rect = tool.SelectedRegion.Value;
			// copy the selected region
			
			Point location = rect.TopLeft;
			rect.Normalize();
			Canvas canvas = new MemoryCanvas(rect.Size);
			canvas.Set(new Point(0,0), rect, tool.Handler.CurrentPage.Canvas);
			
			tool.DrawMode = DrawMode.Paste;
			tool.ClearSelected = true;
			tool.Handler.CursorPosition = location; // go to starting position for the block
			
			// go into paste mode
			tool.PasteCanvas = canvas;
		}
		
	}
}

