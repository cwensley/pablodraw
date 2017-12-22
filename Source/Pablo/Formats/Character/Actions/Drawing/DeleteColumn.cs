using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Undo;
using Eto;

namespace Pablo.Formats.Character.Actions
{
	public class DeleteColumn : PabloCommand
	{
		public DeleteColumn (CharacterHandler handler)
			: base(handler)
		{
			ID = "character_deleteColumn";
			MenuText = "Remove Column";
			ToolTip = "Removes the column at the cursor position";
			Shortcut = Keys.Left | Keys.Alt;
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
				return (int)NetCommands.DeleteColumn;
			}
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = this.Handler as CharacterHandler;
			var canvas = handler.CurrentPage.Canvas;

			Point cursor = handler.CursorPosition;

			handler.Undo.Save (new UndoDeleteColumn (handler, cursor.X){ CursorLocation = cursor});
			
			canvas.DeleteColumn (cursor.X);
			handler.InvalidateCharacterRegion (new Rectangle (cursor.X, 0, canvas.Width - cursor.X, canvas.Height), true);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			var handler = this.Handler as CharacterHandler;
			args.Message.WriteVariableInt32 (handler.CursorPosition.X);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var handler = this.Handler as CharacterHandler;
			var canvas = handler.CurrentPage.Canvas;
			var x = args.Message.ReadVariableInt32 ();
			args.Invoke (delegate {
				handler.Undo.Save (new UndoDeleteColumn (handler, x){ CursorLocation = handler.CursorPosition });
				canvas.DeleteColumn (x);
				handler.InvalidateCharacterRegion (new Rectangle (x, 0, canvas.Width - x, canvas.Height), true);
			});
		}
	}
}
