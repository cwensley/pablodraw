using System;
using System.Linq;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Drawing;
using Pablo.Network;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions.Drawing
{
	public class DrawRect : PabloCommand
	{
		public Rectangle Rectangle { get; set; }
		
		public CanvasElement Element { get; set; }

		public bool ApplyColour { get; set; }
		
		public bool ApplyCharacter { get; set; }
		
		public bool Filled { get; set; }
		
		public bool HalfMode { get; set; }

		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		public DrawRect (CharacterHandler handler)
			: base(handler)
		{
		}
		
		public override int CommandID {
			get { return (int)NetCommands.DrawRect; }
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			Do (Handler.CursorPosition, Rectangle, Element, ApplyColour, ApplyCharacter, Filled, HalfMode);
		}

		public void Do(Point? cursorPosition, Rectangle rect, CanvasElement element, bool applyColour, bool applyCharacter, bool filled, bool halfMode)
		{
			var canvas = Handler.CurrentPage.Canvas;
			Handler.Undo.Save(cursorPosition, cursorPosition, halfMode, rect);

			var lines = new ScanLines();
			lines.AddRect(rect);

			void draw(Rectangle linerect)
			{
				if (halfMode)
					canvas.FillHalfBlocks(linerect, element.Foreground);
				else if (applyColour && applyCharacter)
					canvas.Fill(linerect, element);
				else if (applyColour)
					canvas.Fill(linerect, element.Attribute);
				else if (applyCharacter)
					canvas.Fill(linerect, element.Character);
			};

			if (filled)
				lines.Fill(draw);
			else
				lines.Outline(draw);

			Handler.InvalidateCharacterRegion(rect, true, false, halfMode);
			Handler.Document.IsModified = true;
		}

		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			args.Message.Write (Rectangle);
			args.Message.Write (ApplyColour);
			args.Message.Write (ApplyCharacter);
			args.Message.Write (HalfMode);
			args.Message.Write (Filled);
			args.Message.Write (Element);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var rect = args.Message.ReadRectangle ();
			var applyColour = args.Message.ReadBoolean ();
			var applyCharacter = args.Message.ReadBoolean ();
			var halfMode = args.Message.ReadBoolean ();
			var filled = args.Message.ReadBoolean ();
			var element = args.Message.ReadCanvasElement ();
			args.Invoke (delegate {
				Do (null, rect, element, applyColour, applyCharacter, filled, halfMode);
			});
			
		}
	}
}

