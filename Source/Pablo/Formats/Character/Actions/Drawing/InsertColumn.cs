using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Undo;
using Eto;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public class InsertColumn : Command
	{
		public InsertColumn (CharacterHandler handler)
			: base(handler)
		{
			ID = "character_insertColumn";
			Text = "Insert Column";
			Description = "Inserts a new column at the cursor position";
			Accelerators = new Key[] { Key.Right | Key.Alt };
		}
		
		public new CharacterHandler Handler {
			get { return base.Handler as CharacterHandler; }
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		public override int CommandID {
			get { return (int)NetCommands.InsertColumn; }
		}
		
		public override Pablo.Network.UserLevel Level {
			get { return UserLevel.Operator; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;

			Point cursor = handler.CursorPosition;
			
			handler.Undo.Save (new UndoInsertColumn (handler, cursor.X){ CursorLocation = cursor});
			
			canvas.InsertColumn (cursor.X);
			handler.InvalidateCharacterRegion (new Rectangle (cursor.X, 0, canvas.Width - cursor.X, canvas.Height), true);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			var handler = Handler as CharacterHandler;
			args.Message.WriteVariableInt32 (handler.CursorPosition.X);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var handler = Handler as CharacterHandler;
			var canvas = handler.CurrentPage.Canvas;
			var x = args.Message.ReadVariableInt32 ();

			args.Invoke (delegate { 
				handler.Undo.Save (new UndoInsertColumn (handler, x){ CursorLocation = handler.CursorPosition });
				canvas.InsertColumn (x);
				handler.InvalidateCharacterRegion (new Rectangle (x, 0, canvas.Width - x, canvas.Height), true);
			});
		}
	}
}
