using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Undo;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public class NewLine : PabloCommand
	{
		public NewLine (Handler handler) : base(handler)
		{
			ID = "character_newline";
			MenuText = "New Line";
			Name = "New Line";
			ToolTip = "Adds a new line to the document";
			Shortcut = Keys.Enter;
		}

		public new CharacterHandler Handler {
			get { return base.Handler as CharacterHandler; }
		}

		public override bool Enabled {
			get { return Handler.AllowKeyboardEditing; }
			set { base.Enabled = value; }
		}

		public override bool AllowKeyboardFallback => true;

		public override int CommandID {
			get {
				return (int)NetCommands.NewLine;
			}
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			CharacterHandler handler = this.Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;

			Point cursor = handler.CursorPosition;

			if (cursor.Y < canvas.Height - 1) {
				var pos = cursor;
				pos.Y++;
				pos.X = 0;
				if (handler.InsertMode) {
					InsertLine (pos, cursor);
				}

				handler.CursorPosition = pos;
				handler.EnsureVisible (handler.CursorPosition);
			}
		}
		
		void InsertLine (Point pos, Point cursor)
		{
			CharacterHandler handler = this.Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;
			var rect = new Rectangle (cursor.X, cursor.Y, canvas.Width - cursor.X, 1);
					
			var undo = new UndoBuffer () { CursorLocation = handler.CursorPosition };
			undo.Add (new UndoRect (handler, rect));
			undo.Add (new UndoInsertLine (handler, pos.Y));
			handler.Undo.Save (undo);
					
			var buffer = canvas.Copy (rect);
			canvas.Fill (rect, CanvasElement.Default);
			canvas.InsertRow (pos.Y);
			canvas.Set (pos, buffer);
			handler.InvalidateCharacterRegion (rect, true);
			handler.InvalidateCharacterRegion (new Rectangle (0, pos.Y, canvas.Width, canvas.Height - pos.Y), true);
			
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			CharacterHandler handler = this.Handler as CharacterHandler;
			Canvas canvas = handler.CurrentPage.Canvas;
			var cursor = handler.CursorPosition;
			if (cursor.Y < canvas.Height - 1) {
				var pos = cursor;
				pos.Y++;
				pos.X = 0;
				handler.CursorPosition = pos;
				handler.EnsureVisible (pos);
				
				if (handler.InsertMode) {
					args.Message.Write (cursor);
					args.Message.Write (pos);
					return true;
				}
			}
			return false;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var cursor = args.Message.ReadPoint ();
			var pos = args.Message.ReadPoint ();
			InsertLine (pos, cursor);			
		}
		
	}
}
