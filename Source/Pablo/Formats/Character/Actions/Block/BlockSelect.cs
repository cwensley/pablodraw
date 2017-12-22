using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class BlockSelect : Command
	{
		Tools.Selection tool;
		public BlockSelect(Tools.Selection tool)
		{
			this.tool = tool;
			ID = "character_blockSelect";
			MenuText = "Block Select";
			ToolBarText = "Select";
			ToolTip = "Starts selecting a block of text";
			Shortcut = Keys.B | Keys.Alt;
		}

		protected override void OnExecuted(EventArgs e)
		{
			Point pos = tool.Handler.CursorPosition;
			tool.DrawMode = DrawMode.Selecting;
			tool.SelectedRegion = new Rectangle(pos, pos);
		}
		
	}
}

