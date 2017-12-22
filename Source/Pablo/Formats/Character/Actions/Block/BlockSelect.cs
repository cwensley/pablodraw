using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class BlockSelect : ButtonAction
	{
		Tools.Selection tool;
		public BlockSelect(Tools.Selection tool)
		{
			this.tool = tool;
			ID = "character_blockSelect";
			Text = "Block Select|Select|Start block selection|Starts selecting a block of text";
			Accelerator = Key.B | Key.Alt;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			Point pos = tool.Handler.CursorPosition;
			tool.DrawMode = DrawMode.Selecting;
			tool.SelectedRegion = new Rectangle(pos, pos);
		}
		
	}
}

