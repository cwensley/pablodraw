using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public class Delete : Command
	{
		public Delete (CharacterHandler handler)
			: base(handler)
		{
			ID = "character_delete";
			Text = "Delete|Delete|Delete Character|Deletes the character under the cursor";
			Accelerator = Key.Delete;
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
				return (int)NetCommands.Delete;
			}
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}

		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = Handler as CharacterHandler;
			DoDelete (handler.CursorPosition);
			handler.EnsureVisible (handler.CursorPosition);
		}
		
		void DoDelete (Point pos)
		{
			var handler = Handler as CharacterHandler;
			handler.CurrentPage.Canvas.DeleteCharacter (pos.X, pos.Y, CanvasElement.Default);
			handler.UpdateRegion (new Rectangle (pos, new Size (handler.CurrentPage.Canvas.Width - pos.X, 1)));
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var pos = args.Message.ReadPoint ();
			DoDelete (pos);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			var handler = Handler as CharacterHandler;
			args.Message.Write (handler.CursorPosition);
			return true;
		}
	}
}
