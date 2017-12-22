using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public class SetWidth : PabloCommand
	{
		public const string ActionID = "setWidth";

		public SetWidth (CharacterHandler handler) : base(handler)
		{
			ID = ActionID;
			MenuText = "Set Canvas &Width...";
			ToolBarText = "Set Width";
			ToolTip = "Sets the canvas width";
			Image = ImageCache.IconFromResource("Pablo.Icons.setwidth.ico");
		}
		
		public override int CommandID {
			get {
				return (int)NetCommands.SetWidth;
			}
		}
        
		protected override void Execute (CommandExecuteArgs args)
		{
			CharacterHandler handler = base.Handler as CharacterHandler;
			var size = handler.CurrentPage.Canvas.Size;
			var dialog = new WidthDialog ();
			dialog.Width = size.Width;
			var result = dialog.ShowModal (handler.Viewer as Control);
			if (result == DialogResult.Ok) {
				DoResize (dialog.Width);
			}
		}
		
		void DoResize(int width)
		{
			CharacterHandler handler = base.Handler as CharacterHandler;
			var size = handler.CurrentPage.Canvas.Size;
			size.Width = width;
			
			handler.CurrentPage.Canvas.ResizeCanvas (size, true, true);
			var pos = handler.CursorPosition;
			pos.Restrict (new Rectangle (handler.CurrentPage.Canvas.Size));
			handler.CursorPosition = pos;
			handler.InvalidateCharacterRegion (new Rectangle (size), true);
			handler.TriggerSizeChanged ();
		}
		
		public override bool Send (SendCommandArgs args)
		{
			base.Send (args);
			CharacterHandler handler = base.Handler as CharacterHandler;
			var size = handler.CurrentPage.Canvas.Size;
			var dialog = new WidthDialog ();
			dialog.Width = size.Width;
			var result = dialog.ShowModal(handler.Viewer as Control);
			if (result == DialogResult.Ok) {
				args.Message.WriteVariableInt32 (dialog.Width);
				return true;
			}
			else
				return false;
		}
		
		public override void Receive (ReceiveCommandArgs args)
		{
			base.Receive (args);
			int width = args.Message.ReadVariableInt32 ();
			DoResize (width);
		}
	}
}

