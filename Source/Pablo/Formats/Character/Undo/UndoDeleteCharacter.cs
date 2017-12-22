using System;
using Eto.Drawing;
using Pablo.Network;

namespace Pablo.Formats.Character.Undo
{
	public class UndoDeleteCharacter : UndoBuffer
	{
		public Point Position { get; set; }
		
		public Point? NextPosition { get; set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoDeleteCharacter; }
		}
		
		public UndoDeleteCharacter()
		{
		}
		
		public UndoDeleteCharacter (CharacterHandler handler, Point position)
		{
			this.Position = position;
			Add (new UndoRect (handler, new Rectangle (position.X, position.Y, 1, 1)));
		}

		public override IUndoItem Reciprocal (CharacterHandler handler)
		{
			return new UndoInsertCharacter (handler, this.Position){ NextPosition = this.CursorLocation, CursorLocation = this.NextPosition };
		}
		
		public override void Apply (CharacterHandler handler)
		{
			var canvas = handler.CurrentPage.Canvas;
			canvas.InsertCharacter (Position.X, Position.Y, CanvasElement.Default);
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

