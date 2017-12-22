using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class PolyLine : MultiPointTool<Commands.PolyLine>
	{
		public override string Description {
			get { return "Draws a connected set of lines (L)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.L;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.PolyLine.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.LineStyle>();
			}
		}

		protected override void ApplyPoints (List<Point> points)
		{
			this.Command.Points = points.ToArray ();
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.Add (base.GeneratePad ());
			layout.Add(Separator());
			layout.Add (new Controls.LineStylePad (Handler, true));
			return layout;
		}
		
	}
}

