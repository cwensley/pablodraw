using System;
using Eto.Drawing;
using System.Reflection;
using Eto.Forms;
using Pablo.BGI;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class FilledOval : CenterAspectTool<Commands.FilledOval>
	{
		public override string Description {
			get { return "Draws a filled ellipse or circle (E)"; }
		}
		
		public override Keys Accelerator {
			get {
				return Keys.E;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.FilledOval.png"); }
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
		
		
		protected override void SetPosition (Rectangle rect)
		{
			Command.Point = Point.Max (Point.Empty, rect.Center);
			Command.Radius = rect.Size / 2;
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			this.BGI.Ellipse(Command.Point.X, Command.Point.Y, Command.Radius.Width, Command.Radius.Height, updates);
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.Add(Separator());
			layout.Add (new Controls.LineStylePad(Handler, false));
			layout.Add(Separator());
			layout.Add (new Controls.FillStylePad(Handler));
			return layout;
		}
		
	}
}

