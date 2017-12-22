using System;
using Eto.Drawing;
using Pablo.Network;
using Pablo.Formats.Character.Undo;
using Eto;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions
{
	public class DrawCharacter : Command
	{
		protected virtual int Character { get; set; }
		
		public Point? Location { get; set; }
		
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		public DrawCharacter (CharacterHandler handler)
			: base(handler)
		{
		}
		
		public override int CommandID {
			get {
				return (int)NetCommands.DrawCharacter;
			}
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		public void Activate (int character)
		{
			this.Character = character;
			Activate ();
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = Handler as CharacterHandler;
			var pos = Location ?? handler.CursorPosition;
			var ce = new CanvasElement (this.Character, handler.DrawAttribute);
			var insmode = handler.InsertMode;

			var nextPos = pos;
			if (nextPos.X < handler.CurrentPage.Canvas.Width - 1)
				nextPos.X++;
			
			Do (handler, pos, ce, insmode, false, true, pos, nextPos);
			MoveCursor();
		}
		
		public static void Do (CharacterHandler handler, Point pos, CanvasElement ce, bool insmode, bool inval, bool saveUndo, Point? location, Point? nextPosition)
		{
			var canvas = handler.CurrentPage.Canvas;
			if (insmode) {
				if (saveUndo) handler.Undo.Save (new UndoInsertCharacter (handler, pos) { CursorLocation = location, NextPosition = nextPosition });
				canvas.InsertCharacter (pos.X, pos.Y, ce);
				handler.InvalidateCharacterRegion (new Rectangle (pos, new Point (canvas.Width - 1, pos.Y)), true);
			} else {
				if (saveUndo) handler.Undo.Save (location, nextPosition, new Rectangle (pos, pos));
				var oldce = canvas[pos];
				canvas [pos] = ce;
				if (oldce != ce) {
					if (inval)
						handler.UpdateRegion(new Rectangle(pos, new Size(1, 1)));
					else
						handler.InvalidateCharacterRegion(new Rectangle(pos, new Size(1, 1)), true, true);
				}
			}
		}
		
		void MoveCursor()
		{
			if (Location == null) {
				var handler = Handler as CharacterHandler;
				var canvas = handler.CurrentPage.Canvas;
				var cursor = handler.CursorPosition;
				if (cursor.X < canvas.Width - 1) {
					cursor.X++;
					handler.CursorPosition = cursor;
				} else
					handler.UpdateRegion (new Rectangle (cursor, new Size (1, 1)));
			}
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			var handler = Handler as CharacterHandler;
			var attr = handler.DrawAttribute;
			//var canvas = handler.CurrentPage.Canvas;
			//canvas [handler.CursorPosition] = new CanvasElement (this.Character, attr);
			args.Message.Write (Location ?? handler.CursorPosition);
			args.Message.Write (Location == null ? handler.InsertMode : false);
			args.Message.WriteVariableInt32 (attr.Foreground);
			args.Message.WriteVariableInt32 (attr.Background);
			args.Message.WriteVariableInt32 (this.Character);
			MoveCursor ();
			return true;
		}

		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var pos = args.Message.ReadPoint ();
			var insmod = args.Message.ReadBoolean ();
			var fore = args.Message.ReadVariableInt32 ();
			var back = args.Message.ReadVariableInt32 ();
			var character = args.Message.ReadVariableInt32 ();
			var ce = new CanvasElement(character, new Attribute(fore, back));
			var nextPos = pos;
			if (nextPos.X < Handler.CurrentPage.Canvas.Width - 1)
				nextPos.X++;
			args.Invoke (delegate {
				Do (Handler as CharacterHandler, pos, ce, insmod, true, true, args.IsMe ? (Point?)pos : null, args.IsMe ? (Point?)nextPos : null);
			});
		}
	}
}