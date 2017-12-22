using System;
using Eto.Forms;
using System.IO;
using Eto.Drawing;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class CutToClipboard : PabloCommand
	{
		public const string ActionID = "character_cutToClipboard";
		
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		Selection tool;
		public CutToClipboard (Selection tool)
			: base (tool.Handler)
		{
			this.tool = tool;
			this.ID = ActionID;
			this.MenuText = "&Cut";
			this.ToolTip = "Cut the selected region to the clipboard";
			this.Shortcut = CommonModifier | Keys.X;
			this.Enabled = Handler.Client == null;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Selecting; }
			set { base.Enabled = value; }
		}

		public override int CommandID { get { return (int)NetCommands.CutToClipboard; } }
		
		public override UserLevel Level { get { return UserLevel.Editor; } }
		
		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = tool.Handler;
			if (tool.SelectedRegion == null)
				return;
			var rect = tool.SelectedRegion.Value;
			// copy the selected region
			
			rect.Normalize();
			var location = rect.TopLeft;
			var canvas = new MemoryCanvas(rect.Size);
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

			handler.Undo.Save (handler.CursorPosition, location, rect);
			handler.CurrentPage.Canvas.Fill(rect, handler.DrawElement);
			handler.InvalidateCharacterRegion(rect, true, true);
			
			tool.DrawMode = DrawMode.Normal;
			handler.CursorPosition = location; // go to starting position for the block
			
		}
	}
}

