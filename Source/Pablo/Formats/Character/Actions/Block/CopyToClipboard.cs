using System;
using Eto.Forms;
using System.IO;
using Eto.Drawing;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class CopyToClipboard : Command
	{
		public const string ActionID = "character_copyToClipboard";
		
		Selection tool;
		
		public CopyToClipboard (Selection tool)
		{
			this.tool = tool;
			this.ID = ActionID;
			MenuText = "&Copy";
			ToolBarText = "Copy";
			ToolTip = "Copy the selected region to the clipboard";
			Shortcut = PabloCommand.CommonModifier | Keys.C;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Selecting; }
			set { base.Enabled = value; }
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			if (tool.SelectedRegion == null)
				return;
			var handler = tool.Handler;
			var rect = tool.SelectedRegion.Value;
			// copy the selected region
			
			var location = rect.TopLeft;
			rect.Normalize();
			Canvas canvas = new MemoryCanvas(rect.Size);
			canvas.Set(Point.Empty, rect, handler.CurrentPage.Canvas);
			
			
			// save to clipboard
			var cb = new Clipboard();
			cb.Clear (); // clear out contents of clipboard
			
			// save as text
			var ascii = new Types.Ascii(handler.Info);
			cb.Text = ascii.SaveToText(canvas, handler.CurrentPage.Font.Encoding);
			
			
			// save as ansi
			using (var ansistream = new MemoryStream()) {
				var writer = new BinaryWriter(ansistream);
				writer.Write (canvas.Width);
				writer.Write (canvas.Height);
				writer.Flush ();
				Types.Pablo.Save (ansistream, canvas, handler.CurrentPage.Palette);
				ansistream.Flush ();
				
				cb.SetDataStream(ansistream, "pablo");
			}
			
			tool.DrawMode = DrawMode.Normal;
			handler.CursorPosition = location; // go to starting position for the block
		}
	}
}

