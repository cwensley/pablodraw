using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Copy : ButtonAction
	{
		Selection tool;

		public Copy (Selection tool)
		{
			this.tool = tool;
			ID = "character_Copy";
			Text = "&Copy Block|Copy|Copy selected region|Copies the selected region";
			Accelerator = Key.C;
			Enabled = tool.Handler.Client == null || tool.Handler.Client.CurrentUser.Level >= UserLevel.Editor;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			Rectangle rect = tool.SelectedRegion.Value;
			// copy the selected region
			
			Point location = rect.TopLeft;
			rect.Normalize ();
			Canvas canvas = new MemoryCanvas (rect.Size);
			canvas.Set (new Point (0, 0), rect, tool.Handler.CurrentPage.Canvas);
			
			tool.DrawMode = DrawMode.Paste;
			tool.SelectedRegion = null;
			tool.Handler.CursorPosition = location; // go to starting position for the block
			
			// go into paste mode
			tool.PasteCanvas = canvas;
		}
		
	}
}

