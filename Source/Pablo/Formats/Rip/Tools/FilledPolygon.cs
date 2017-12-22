using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class FilledPolygon : MultiPointTool<Commands.FilledPolygon>
	{
		public override string Description {
			get { return "Draws an enclosed, filled polygon (O)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.O;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.FilledPolygon.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.LineStyle>();
				yield return Document.Create<Commands.FillStyle>();
				yield return Document.Create<Commands.FillPattern>();
			}
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			if (this.Command.Points.Length == 2)
				this.BGI.Line (this.Command.Points [0], this.Command.Points [1], updates);
			else
				this.BGI.DrawPoly (this.Command.Points, updates);
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
			layout.Add (new Controls.LineStylePad(Handler, true));
			layout.Add(Separator());
			layout.Add (new Controls.FillStylePad(Handler));
			return layout;
		}
		
	}
}

