using Eto.Forms;
using Eto.Drawing;
using System;
using Pablo.Network;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Stamp : Command
	{
		Selection tool;
		public Stamp (Selection tool) : base(tool.Handler)
		{
			this.tool = tool;
			ID = "character_Stamp";
			Text = "&Stamp|Stamp|Stamp selected region|Copies the selected region";
			Name = "Stamp";
			Accelerators = new Key[] { Key.S, Key.Space };
		}
		
		public override int CommandID {
			get {
				return (int)NetCommands.BlockStamp;
			}
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			CharacterHandler handler = this.Handler as CharacterHandler;
			// Stamp the selected region
			
			Canvas canvas = tool.PasteCanvas;

			Rectangle? clearRect = null;
			if (tool.ClearSelected) {
				clearRect = tool.SelectedRegion.Value;
				tool.ClearSelected = false;
			}
			
			Do (handler.CursorPosition, tool.PasteMode, canvas, clearRect, handler.CursorPosition);
			
			tool.SelectedRegion = null;
		}
		
		void Do (Point position, PasteMode pasteMode, Canvas canvas, Rectangle? clearRect, Point? cursorPosition)
		{
			var handler = this.Handler as CharacterHandler;
			if (clearRect != null) {
				var cr = clearRect.Value;
				cr.Normalize ();
				handler.Undo.Save (cursorPosition, position, cr, new Rectangle (position, canvas.Size));
				handler.CurrentPage.Canvas.Fill (cr, CanvasElement.Default);
				handler.InvalidateCharacterRegion (cr, true);
			} else
				handler.Undo.Save (cursorPosition, position, new Rectangle (position, canvas.Size));
		
			handler.CurrentPage.Canvas.Set (position, new Rectangle (canvas.Size), canvas, pasteMode);
			handler.InvalidateCharacterRegion (new Rectangle (position, canvas.Size), true);
		}
		
		public override void Receive (ReceiveCommandArgs args)
		{
			base.Receive (args);
			var handler = this.Handler as CharacterHandler;

			var clearSelected = args.Message.ReadBoolean ();
			var clearRect = clearSelected ? (Rectangle?)args.Message.ReadRectangle () : null;
			var position = args.Message.ReadPoint ();
			var pasteMode = args.Message.ReadEnum<PasteMode> ();
			var canvas = args.Message.ReadCanvas (handler.CurrentPage.Palette);

			Do (position, pasteMode, canvas, clearRect, args.IsMe ? (Point?)position : null);
		}
		
		public override bool Send (SendCommandArgs args)
		{
			base.Send (args);
			var handler = this.Handler as CharacterHandler;
			
			args.Message.Write (tool.ClearSelected);
			if (tool.ClearSelected) {
				var clearRect = tool.SelectedRegion.Value;
				args.Message.Write (clearRect);
				tool.ClearSelected = false;
			}
			args.Message.Write (handler.CursorPosition);
			args.Message.WriteEnum (tool.PasteMode);
			args.Message.Write (tool.PasteCanvas, handler.CurrentPage.Palette);
			
			tool.SelectedRegion = null;
			return true;
		}
		
	}
}

