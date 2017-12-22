using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Pablo.BGI;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class Bar : CenterAspectTool<Commands.Bar>
	{
		public override string Description {
			get { return "Draws filled rectangle with no border (R)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.R;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.FilledRectangle.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.FillStyle>();
				yield return Document.Create<Commands.FillPattern>();
			}
		}
		
		protected override void SetPosition (Rectangle rect)
		{
			rect.TopLeft = Point.Max (Point.Empty, rect.TopLeft);
			rect.InnerBottomRight = Point.Max (Point.Empty, rect.InnerBottomRight);
			this.Command.Rectangle = rect;
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			var rect = this.Command.Rectangle;
			this.BGI.Rectangle (rect.Left, rect.Top, rect.InnerRight, rect.InnerBottom, updates);
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.Add (Separator ());
			layout.Add (new Controls.FillStylePad(Handler));
			return layout;
		}
		
	}
}

