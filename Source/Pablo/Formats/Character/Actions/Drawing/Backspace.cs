using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public class Backspace : PabloCommand
	{
		public Backspace (CharacterHandler handler)
			: base(handler)
		{
			ID = "character_backspace";
			MenuText = "Backspace";
			ToolTip = "Deletes the previous character";
			Shortcut = Keys.Backspace;
		}
		
		public new CharacterHandler Handler {
			get { return base.Handler as CharacterHandler; }
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		public override int CommandID {
			get {
				return (int)NetCommands.Backspace;
			}
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}

		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;

			if (handler.CursorPosition.X > 0) {
				Point newPoint = handler.CursorPosition;
				newPoint.X--;
				if (handler.InsertMode) {
					canvas.DeleteCharacter (newPoint.X, newPoint.Y, CanvasElement.Default);
					handler.InvalidateCharacterRegion (new Rectangle (newPoint, new Size (canvas.Width - newPoint.X, 1)), true);
				} else {
					canvas [newPoint] = CanvasElement.Default;
				}
				handler.CursorPosition = newPoint;
			} else if (handler.InsertMode && handler.CursorPosition.Y > 0) {
				// move line up!
				MoveLineUp (handler.CursorPosition, handler.WrapMode);
			}
		}
		
		void MoveLineUp (Point point, bool wrapmode)
		{
			var handler = Handler as CharacterHandler;
			var canvas = handler.CurrentPage.Canvas;
			var cursorpos = point;
			point.Y--;
			point.X = canvas.FindEndX (point.Y, CanvasElement.Default) + 1;
				
			int width = canvas.Width - point.X;
			if (width > 0) {
				var curLineRect = new Rectangle (0, cursorpos.Y, width, 1);
				Canvas buffer = canvas.Copy (curLineRect);
				canvas.Set (point, buffer);

				if (wrapmode) {
					// shift rest of line to beginning
					var copyRect = new Rectangle (width, cursorpos.Y, canvas.Width - width, 1);
					buffer = canvas.Copy (copyRect);
					canvas.Set (new Point (0, cursorpos.Y), buffer);
					copyRect = new Rectangle (canvas.Width - width, cursorpos.Y, width, 1);
					canvas.Fill (copyRect, CanvasElement.Default);
				}
					
				int lastx = canvas.FindEndX (cursorpos.Y, CanvasElement.Default);
				if (lastx == -1) {
					canvas.DeleteRow (cursorpos.Y);
					handler.InvalidateCharacterRegion (new Rectangle (0, point.Y, canvas.Width, canvas.Height - point.Y), true);
				} else
					handler.InvalidateCharacterRegion (new Rectangle (0, point.Y, canvas.Width, 2), true);
			}
		}
		
		public override void Receive (ReceiveCommandArgs args)
		{
			base.Receive (args);
			var handler = Handler as CharacterHandler;
			var canvas = handler.CurrentPage.Canvas;

			bool inline = args.Message.ReadBoolean ();
			var point = args.Message.ReadPoint ();
			if (inline) {
				point.X--;
				var insmode = args.Message.ReadBoolean ();
				if (insmode) {
					canvas.DeleteCharacter (point.X, point.Y, CanvasElement.Default);
					handler.InvalidateCharacterRegion (new Rectangle (point, new Size (canvas.Width - point.X, 1)), true);
				} else {
					canvas [point] = CanvasElement.Default;
				}
			} else {
				var wrapmode = args.Message.ReadBoolean ();
				MoveLineUp (point, wrapmode);
			}
		}
		
		public override bool Send (SendCommandArgs args)
		{
			base.Send (args);
			var handler = Handler as CharacterHandler;
			var canvas = handler.CurrentPage.Canvas;

			if (handler.CursorPosition.X > 0) {
				var point = handler.CursorPosition;
				args.Message.Write (true);
				args.Message.Write (point);
				args.Message.Write (handler.InsertMode);
				
				point.X--;
				handler.CursorPosition = point;
			} else if (handler.InsertMode && handler.CursorPosition.Y > 0) {
				args.Message.Write (false);
				// move line up!
				var point = handler.CursorPosition;
				args.Message.Write (point);
				args.Message.Write (handler.WrapMode);
				point.Y--;
				point.X = canvas.FindEndX (point.Y, CanvasElement.Default) + 1;
				
				handler.CursorPosition = point;
			} else
				return false;
			
			return true;
		}
	}
}
