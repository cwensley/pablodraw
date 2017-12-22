using System;
using Pablo.Network;
using Eto.Drawing;
using Eto;
using Eto.Forms;

namespace Pablo.Formats.Character.Actions.Drawing
{
	public class Brush : PabloCommand
	{
		public Point Location { get; set; }
		public BrushInfo DrawBrush { get; set; }
		public int Size { get; set; }
		public bool Inverse { get; set; }
		public bool ApplyColour { get; set; }
		
		
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }
		
		public Brush (CharacterHandler handler)
			: base(handler)
		{
			Size = 3;
		}
		
		public override int CommandID {
			get { return (int)NetCommands.DrawBrush; }
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			if (DrawBrush == null) return;
			var gradient = DrawBrush.GetCharacters (Handler.CurrentPage.Font.Encoding);
			Do (Location, Handler.CursorPosition, Handler.DrawAttribute, gradient, Size, Inverse, ApplyColour);
		}
		
		void DrawCharacter (Canvas canvas, Point location, Attribute attribute, Character[] gradient, bool inverse, bool applyColour)
		{
			var cur = canvas [location];
			var index = Array.LastIndexOf (gradient, cur.Character);
			Character character;
			if (inverse) {
				if (index > 0 && index <= gradient.Length - 1) {
					index --;
					character = gradient [index];
				}
				else character = CanvasElement.Default.Character;
			}
			else {
				if (index >= 0 && index < gradient.Length - 1) {
					index ++;
				} else if (index == -1)
					index = 0;
				character = gradient[index];
			}

			canvas[location] = new CanvasElement(character, applyColour ? attribute : cur.Attribute);
		}
		
		void Do (Point location, Point? cursorPosition, Attribute attribute, Character[] gradient, int size, bool inverse, bool applyColour)
		{
			var canvas = Handler.CurrentPage.Canvas;
			var rect = new Rectangle (location, new Size(size, size));
			rect.Restrict (canvas.Size);
			if (!rect.IsEmpty) {
				Handler.Undo.Save (cursorPosition, cursorPosition, rect);
				var pt = new Point ();
				for (pt.Y = rect.Top; pt.Y <= rect.InnerBottom; pt.Y ++)
					for (pt.X = rect.Left; pt.X <= rect.InnerRight; pt.X ++) {
						DrawCharacter (canvas, pt, attribute, gradient, inverse, applyColour);
					}
				Handler.InvalidateCharacterRegion(rect, true, false);
				Handler.CharacterDocument.IsModified = true;
			}
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			var attr = Handler.DrawAttribute;
			var gradient = DrawBrush.GetCharacters (Handler.CurrentPage.Font.Encoding);
			args.Message.Write (gradient);
			args.Message.Write (Location);
			args.Message.Write (attr);
			args.Message.Write (Inverse);
			args.Message.Write (ApplyColour);
			args.Message.WriteVariableInt32 (this.Size);
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var gradient = args.Message.ReadArray<Character>();
			var location = args.Message.ReadPoint ();
			var attr = args.Message.ReadAttribute ();
			var inverse = args.Message.ReadBoolean ();
			var applyColour = args.Message.ReadBoolean ();
			var size = args.Message.ReadVariableInt32 ();
			args.Invoke (delegate {
				Do (location, null, attr, gradient, size, inverse, applyColour);
			});
		}
	}
}

