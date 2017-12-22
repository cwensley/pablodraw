using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Deselect : Command
	{
		Tools.Selection tool;
		public Deselect(Tools.Selection tool)
		{
			this.tool = tool;
			ID = "character_deselect";
			MenuText = "&Deselect";
			ToolBarText = "Deselect";
			ToolTip = "Clears the current selection";
			Shortcut = Keys.Escape;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode != DrawMode.Normal; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			tool.PasteCanvas = null;
			tool.ClearSelected = false;
			tool.DrawMode = DrawMode.Normal;
		}
		
	}
}

