using System;
using Eto.Drawing;

namespace Pablo.Formats.Character.Undo
{
	public class UndoDeleteLine : UndoBuffer
	{
		public int Line { get; set; }
		
		public override int UndoID {
			get { return (int)UndoType.UndoDeleteLine; }
		}
		
		public UndoDeleteLine()
		{
		}
		
		public UndoDeleteLine(CharacterHandler handler, int line)
		{
			this.Line = line;
			var canvas = handler.CurrentPage.Canvas;
			Add (new UndoRect(handler, new Rectangle(0, Line, canvas.Width, 1)));
		}

		public override IUndoItem Reciprocal (CharacterHandler handler)
		{
			return new UndoInsertLine(handler, this.Line){ CursorLocation = this.CursorLocation };
		}
		
		public override void Apply (CharacterHandler handler)
		{
			var canvas = handler.CurrentPage.Canvas;
			canvas.InsertRow (Line);
			base.Apply (handler);
			handler.InvalidateCharacterRegion (new Rectangle(0, Line, canvas.Width, canvas.Height - Line), true);
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

