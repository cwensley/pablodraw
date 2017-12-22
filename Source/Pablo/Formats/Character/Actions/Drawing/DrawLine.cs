using System;
using System.Linq;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Drawing;
using Pablo.Network;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions.Drawing
{
	public class DrawLine : PabloCommand
	{
		public Point Start { get; set; }

		public Point End { get; set; }
		
		public CanvasElement Element { get; set; }

		public bool ApplyColour { get; set; }
		
		public bool ApplyCharacter { get; set; }
		
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		public DrawLine (CharacterHandler handler)
			: base(handler)
		{
		}
		
		public override int CommandID {
			get { return (int)NetCommands.DrawLine; }
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			Do (Handler.CursorPosition, Start, End, Element, ApplyColour, ApplyCharacter);
		}
		
		public void Do (Point? cursorPosition, Point start, Point end, CanvasElement element, bool applyColour, bool applyCharacter)
		{
			var lines = new ScanLines ();
			lines.AddLine (start, end);
			var canvas = Handler.CurrentPage.Canvas;
			Handler.Undo.Save (cursorPosition, cursorPosition, new Rectangle(start, end));
			foreach (var row in lines.Values) {
				var minx = row.Min ();
				var width = row.Max () - minx + 1;
				var rect = new Rectangle(minx, row.Row, width, 1);
				if (applyColour && applyCharacter)
					canvas.Fill (rect, element);
				else if (applyColour)
					canvas.Fill (rect, element.Attribute);
				else if (applyCharacter)
					canvas.Fill (rect, element.Character);
			}
			Handler.InvalidateCharacterRegion(new Rectangle(start, end), true, false);
			Handler.Document.IsModified = true;
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			args.Message.Write (Start);
			args.Message.Write (End);
			args.Message.Write (ApplyColour);
			args.Message.Write (ApplyCharacter);
			args.Message.Write (Element);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var start = args.Message.ReadPoint ();
			var end = args.Message.ReadPoint ();
			var applyColour = args.Message.ReadBoolean ();
			var applyCharacter = args.Message.ReadBoolean ();
			var element = args.Message.ReadCanvasElement();
			args.Invoke (delegate {
				Do (null, start, end, element, applyColour, applyCharacter);
			});
			
		}
	}
}

