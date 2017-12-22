using System;
using Eto.Drawing;
using System.Reflection;
using Eto.Forms;
using Pablo.BGI;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class Oval : CenterAspectTool<Commands.Oval>
	{
		public override string Description {
			get { return "Draws an ellipse or circle (E)"; }
		}
		
		public override Keys Accelerator {
			get {
				return Keys.E;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Oval.png"); }
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
			this.Command.Point = Point.Max (Point.Empty, rect.Center);
			this.Command.Radius = rect.Size / 2;
			this.Command.StartAngle = 0;
			this.Command.EndAngle = 360;
		}
		
		protected override void FinishCommand (Keys modifiers, IList<Rectangle> updates = null)
		{
			if (IsSquare) {
				var command = RipCommands.Create<Commands.Circle> (Document);
				command.Point = this.Command.Point;
				command.Radius = this.Command.Radius.Width;
				Handler.AddCommand (command, updates);
				Command = null;
			} else
				base.FinishCommand (modifiers, updates);
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };

			layout.Add(Separator());
			layout.Add (new Controls.LineStylePad (Handler, false));
			return layout;
		}
		
	}
}

