using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Pablo.BGI;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class DrawRectangle : CenterAspectTool<Commands.DrawRectangle>
	{
		public override string Description {
			get { return "Draws a rectangle or square (R)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.R;
			}
		}
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Rectangle.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.LineStyle>();
			}
		}
		
		protected override void SetPosition (Rectangle rect)
		{
			rect.TopLeft = Point.Max (Point.Empty, rect.TopLeft);
			rect.InnerBottomRight = Point.Max (Point.Empty, rect.InnerBottomRight);
			this.Command.Rectangle = rect;
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.Add (Separator ());
			layout.Add (new Controls.LineStylePad (Handler, true));
			return layout;
		}
	}
}

