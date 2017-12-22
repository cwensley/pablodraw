using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Rotate : Command
	{
		Selection tool;
		public Rotate(Selection tool)
		{
			this.tool = tool;
			ID = "character_Rotate";
			MenuText = "&Rotate";
			ToolBarText = "Rotate";
			ToolTip = "Rotate the region";
			Shortcut = Keys.R;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Paste; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			Canvas canvas = tool.PasteCanvas;
			
			tool.PasteCanvas = canvas.Rotate(tool.Handler.Info.FlipRotate);
		}
	}
}

