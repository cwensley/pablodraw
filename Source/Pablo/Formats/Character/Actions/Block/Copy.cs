using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Copy : Command
	{
		Selection tool;

		public Copy(Selection tool)
		{
			this.tool = tool;
			ID = "character_Copy";
			MenuText = "&Copy Block";
			ToolBarText = "Copy";
			ToolTip = "Copies the selected region";
			Shortcut = Keys.C;
			Enabled = tool.Handler.Client == null || tool.Handler.Client.CurrentUser.Level >= UserLevel.Editor;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Selecting; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			Rectangle rect = tool.SelectedRegion.Value;
			// copy the selected region
			
			Point location = rect.TopLeft;
			rect.Normalize();
			Canvas canvas = new MemoryCanvas(rect.Size);
			canvas.Set(new Point(0, 0), rect, tool.Handler.CurrentPage.Canvas);
			
			tool.DrawMode = DrawMode.Paste;
			tool.SelectedRegion = null;
			tool.Handler.CursorPosition = location; // go to starting position for the block
			
			// go into paste mode
			tool.PasteCanvas = canvas;
		}
	}
}

