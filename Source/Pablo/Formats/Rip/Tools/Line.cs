using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Eto;

namespace Pablo.Formats.Rip.Tools
{
	public class Line : TwoPointTool<Commands.Line>
	{
		
		public override string Description {
			get { return "Draws a line between two points (L)"; }
		}
		
		public override Key Accelerator {
			get {
				return Key.L;
			}
		}

		public override Eto.Drawing.Image Image {
			get { return Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Line.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.LineStyle>();
			}
		}
		
		protected override void SetStartLocation (Point start, Key modifiers, Point location)
		{
			this.Command.Start = Point.Max (Point.Empty, start);
		}
		
		protected override void SetEndLocation (Point end, Key modifiers, Point location)
		{
			
			this.Command.End = Point.Max (Point.Empty, end);
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);

			layout.Add(Separator());
			layout.Add (new Controls.LineStylePad (Handler, true));
			return layout;
		}
		
	}
}

