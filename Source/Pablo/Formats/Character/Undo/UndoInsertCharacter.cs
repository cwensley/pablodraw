using System;
using Eto.Drawing;
using Pablo.Network;

namespace Pablo.Formats.Character.Undo
{
	public class UndoInsertCharacter : UndoBuffer
	{
		public Point Position { get; set; }
		
		public Point? NextPosition { get; set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoInsertCharacter; }
		}
		
		public UndoInsertCharacter ()
		{
		}
		
		public UndoInsertCharacter (CharacterHandler handler, Point position)
		{
			this.Position = position;
			var canvas = handler.CurrentPage.Canvas;
			Add (new UndoRect (handler, new Rectangle (canvas.Width - 1, position.Y, 1, 1)));
		}

		public override IUndoItem Reciprocal (CharacterHandler handler)
		{
			return new UndoDeleteCharacter (handler, Position){ NextPosition = this.CursorLocation, CursorLocation = NextPosition };
		}
		
		public override void Apply (CharacterHandler handler)
		{
			var canvas = handler.CurrentPage.Canvas;
			handler.CurrentPage.Canvas.DeleteCharacter (Position.X, Position.Y, CanvasElement.Default);
			base.Apply (handler);
			handler.InvalidateCharacterRegion (new Rectangle (Position.X, Position.Y, canvas.Width - Position.X, 1), true);
		}

		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			args.Message.Write (Position);
			args.Message.Write (NextPosition != null);
			if (NextPosition != null) args.Message.Write (NextPosition.Value);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			Position = args.Message.ReadPoint ();
			NextPosition = args.Message.ReadBoolean () ? (Point?)args.Message.ReadPoint () : null;
		}
	}
}

