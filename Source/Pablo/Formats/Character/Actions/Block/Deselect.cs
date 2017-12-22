using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Deselect : ButtonAction
	{
		Tools.Selection tool;
		public Deselect(Tools.Selection tool)
		{
			this.tool = tool;
			ID = "character_deselect";
			Text = "&Deselect|Deselect|Clears the selection|Clears the current selection";
			Accelerator = Key.Escape;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			tool.PasteCanvas = null;
			tool.ClearSelected = false;
			tool.DrawMode = DrawMode.Normal;
		}
		
	}
}

