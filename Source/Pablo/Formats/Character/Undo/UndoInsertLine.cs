using System;
using Eto.Drawing;

namespace Pablo.Formats.Character.Undo
{
	public class UndoInsertLine : UndoBuffer
	{
		public int Line { get; set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoInsertLine; }
		}
		
		public UndoInsertLine ()
		{
		}
		
		public UndoInsertLine (CharacterHandler handler, int line)
		{
			this.Line = line;
			var canvas = handler.CurrentPage.Canvas;
			Add (new UndoRect (handler, new Rectangle (0, canvas.Height - 1, canvas.Width, 1)));
		}

		public override IUndoItem Reciprocal (CharacterHandler handler)
		{
			return new UndoDeleteLine (handler, Line){ CursorLocation = this.CursorLocation };
		}
		
		public override void Apply (CharacterHandler handler)
		{
			var canvas = handler.CurrentPage.Canvas;
			canvas.DeleteRow (Line);
			base.Apply (handler);
			handler.InvalidateCharacterRegion (new Rectangle (0, Line, canvas.Width, canvas.Height - Line), true);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			args.Message.WriteVariableInt32 (Line);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			Line = args.Message.ReadVariableInt32 ();
		}
	}
}

