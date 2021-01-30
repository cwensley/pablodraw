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
			MenuText = "Set Canvas &Size...";
			ToolBarText = "Set Size";
			ToolTip = "Sets the canvas size";
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
			dialog.CanvasSize = size;
			var result = dialog.ShowModal (handler.Viewer as Control);
			if (result == DialogResult.Ok) {
				DoResize (dialog.CanvasSize);
			}
		}
		
		void DoResize(Size canvasSize)
		{
			CharacterHandler handler = base.Handler as CharacterHandler;
			var size = handler.CurrentPage.Canvas.Size;
			size = canvasSize;
			
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
			dialog.CanvasSize = size;
			var result = dialog.ShowModal(handler.Viewer as Control);
			if (result == DialogResult.Ok) {
				args.Message.Write (dialog.CanvasSize);
				return true;
			}
			else
				return false;
		}
		
		public override void Receive (ReceiveCommandArgs args)
		{
			base.Receive (args);
			var size = args.Message.ReadSize ();
			DoResize (size);
		}
	}
}

