using System;
using Eto.Forms;
using System.IO;
using Eto.Drawing;
using System.Linq;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class PasteFromClipboard : PabloCommand
	{
		public const string ActionID = "character_pasteFromClipboard";
		readonly Selection tool;

		public PasteFromClipboard(Selection tool)
			: base(tool.Handler)
		{
			this.tool = tool;
			this.ID = ActionID;
			this.MenuText = "&Paste";
			this.ToolTip = "Paste at the current position from the clipboard";
			this.Shortcut = CommonModifier | Keys.V;
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Normal; }
			set { base.Enabled = value; }
		}

		public override int CommandID
		{
			get
			{
				return (int)NetCommands.PasteFromClipboard;
			}
		}

		public override UserLevel Level
		{
			get { return UserLevel.Editor; }
		}

		Canvas GetCanvasFromClipboard()
		{
			var handler = (CharacterHandler)Handler;

			Canvas canvas = null;
			var cb = new Clipboard();
			var stream = cb.GetDataStream("pablo");
			if (stream != null)
			{
				// get width/height
				var reader = new BinaryReader(stream);
				var width = reader.ReadInt32();
				var height = reader.ReadInt32();
				canvas = new MemoryCanvas(new Size(width, height));
				Types.Pablo.Load(stream, canvas, handler.CurrentPage.Palette);
			}
			else
			{
				var text = cb.Text;
				if (!string.IsNullOrEmpty(text))
				{
					// calculate height/width of incoming text
					var lines = text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
					var width = lines.Max(r => r.Length);
					var height = lines.Length;

					// encode to memory stream
					var textstream = new MemoryStream(handler.CurrentPage.Font.Encoding.GetBytes(text), false);
					
					// load it up
					canvas = new MemoryCanvas(new Size(width, height));
					var ascii = new Types.Ascii(handler.Info);
					ascii.Load(textstream, canvas, null, false);
					//pasteMode = PasteMode.Transparent;
				}
			}
			return canvas;
			
		}

		protected override void Execute(CommandExecuteArgs args)
		{
			var handler = Handler as CharacterHandler;
			
			var pasteMode = tool.PasteMode;
			var canvas = GetCanvasFromClipboard();
			
			if (canvas != null)
			{
				handler.Undo.Save(handler.CursorPosition, handler.CursorPosition, new Rectangle(handler.CursorPosition, canvas.Size));
				handler.CurrentPage.Canvas.Set(handler.CursorPosition, new Rectangle(canvas.Size), canvas, pasteMode);
				handler.InvalidateCharacterRegion(new Rectangle(handler.CursorPosition, canvas.Size), true);
			}
		}

		public override bool Send(SendCommandArgs args)
		{
			base.Send(args);
			var handler = (CharacterHandler)Handler;
			
			var canvas = GetCanvasFromClipboard();
			if (canvas != null)
			{
				args.Message.Write(handler.CursorPosition);
				args.Message.WriteEnum(tool.PasteMode);
				args.Message.Write(canvas, handler.CurrentPage.Palette);
				return true;
			}
			return false;
		}

		public override void Receive(ReceiveCommandArgs args)
		{
			base.Receive(args);
			var handler = Handler as CharacterHandler;
			
			var position = args.Message.ReadPoint();
			var pasteMode = args.Message.ReadEnum<PasteMode>();
			var canvas = args.Message.ReadCanvas(handler.CurrentPage.Palette);
			var rect = new Rectangle(position, canvas.Size);
			var cursor = args.IsMe ? (Point?)position : null;
			handler.Undo.Save(cursor, cursor, rect);
			handler.CurrentPage.Canvas.Set(position, new Rectangle(canvas.Size), canvas, pasteMode);
			handler.InvalidateCharacterRegion(rect, true);
		}
	}
}

