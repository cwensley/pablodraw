using System;
using Eto.Forms;
using System.IO;
using Eto.Drawing;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class CopyToClipboard : ButtonAction
	{
		public const string ActionID = "character_copyToClipboard";
		
		Selection tool;
		
		public CopyToClipboard (Selection tool)
		{
			this.tool = tool;
			this.ID = ActionID;
			this.Text = "&Copy|Copy|Copy the selected region to the clipboard";
			this.Accelerator = Command.CommonModifier | Key.C;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			
			
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

