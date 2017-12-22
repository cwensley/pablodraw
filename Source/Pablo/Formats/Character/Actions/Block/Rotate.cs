using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Rotate : ButtonAction
	{
		Selection tool;
		public Rotate(Selection tool)
		{
			this.tool = tool;
			ID = "character_Rotate";
			Text = "&Rotate|Rotate|Rotate the region";
			Accelerator = Key.R;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			Canvas canvas = tool.PasteCanvas;
			
			tool.PasteCanvas = canvas.Rotate(tool.Handler.Info.FlipRotate);
		}
	}
}

