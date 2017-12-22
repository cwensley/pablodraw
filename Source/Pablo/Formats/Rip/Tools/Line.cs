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
		
		public override Keys Accelerator {
			get {
				return Keys.L;
			}
		}

		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Line.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.LineStyle>();
			}
		}
		
		protected override void SetStartLocation (Point start, Keys modifiers, Point location)
		{
			this.Command.Start = Point.Max (Point.Empty, start);
		}
		
		protected override void SetEndLocation (Point end, Keys modifiers, Point location)
		{
			
			this.Command.End = Point.Max (Point.Empty, end);
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };

			layout.Add(Separator());
			layout.Add (new Controls.LineStylePad (Handler, true));
			return layout;
		}
		
	}
}

