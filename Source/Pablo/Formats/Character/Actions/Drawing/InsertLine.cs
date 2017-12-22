using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Undo;
using Eto;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public class InsertLine : PabloCommand
	{
		
		public InsertLine (Handler handler)
			: base(handler)
		{
			ID = "character_insertline";
			MenuText = "Insert Line";
			Name = "Insert Line";
			ToolTip = "Inserts a new line at the cursor position";
			Shortcut = Keys.Down | Keys.Alt; // TODO: Shortcut, Keys.Alt | Keys.I };
		}
		
		public new CharacterHandler Handler {
			get { return base.Handler as CharacterHandler; }
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}
		
		public override int CommandID {
			get { return (int)NetCommands.InsertLine; }
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			CharacterHandler handler = this.Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;

			Point cursor = handler.CursorPosition;
			
			
			handler.Undo.Save (new UndoInsertLine (handler, cursor.Y){ CursorLocation = cursor});
			
			canvas.InsertRow (cursor.Y);
			var rect = new Rectangle (0, cursor.Y, canvas.Width, canvas.Height - cursor.Y);
			handler.InvalidateCharacterRegion (rect, true);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			CharacterHandler handler = this.Handler as CharacterHandler;
			args.Message.WriteVariableInt32 (handler.CursorPosition.Y);
			
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			CharacterHandler handler = this.Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;
			
			int y = args.Message.ReadVariableInt32 ();
			
			var rect = new Rectangle (0, y, canvas.Width, canvas.Height - y);
			
			if (!args.IsMe) {
				args.Invoke (delegate {
					handler.Undo.Save (new UndoInsertLine (handler, y){ CursorLocation = handler.CursorPosition });
					canvas.InsertRow (y);
					if (y <= handler.CursorPosition.Y) {
						var pos = handler.CursorPosition;
						pos.Y++;
						handler.CursorPosition = pos;
						var scrollpos = handler.Viewer.ScrollPosition;
						scrollpos.Y += handler.CurrentPage.Font.Height;
						handler.Viewer.ScrollPosition = scrollpos;
					}
					if (handler.SelectedTool != null)
						handler.SelectedTool.InsertLine (y);
					handler.InvalidateCharacterRegion (rect, true);
				});
			} else {
				args.Invoke (delegate {
					handler.Undo.Save (new UndoInsertLine (handler, y){ CursorLocation = handler.CursorPosition });
					canvas.InsertRow (y);
					handler.InvalidateCharacterRegion (rect, true);
				});
			}
		}
	}
}
