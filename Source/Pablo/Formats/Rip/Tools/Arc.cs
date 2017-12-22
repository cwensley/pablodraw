using System;
using Eto.Forms;
using System.Collections;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.BGI;
using System.Reflection;

namespace Pablo.Formats.Rip.Tools
{
	public class Arc : AngleTool<Commands.OvalArc>
	{
		public override string Description {
			get { return "Draws an elliptical or circular arc (A)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.A;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Arc.png"); }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.LineStyle>();
			}
		}
		
		protected override void SetCenter (Point point, Rectangle rect)
		{
			this.Command.Point = point;
			this.Command.Radius = rect.Size / 2;
			this.Command.StartAngle = 0;
			this.Command.EndAngle = 360;
		}
		
		protected override void SetStartAngle (int angle)
		{
			this.Command.StartAngle = angle;
		}
		
		protected override void SetEndAngle (int angle)
		{
			this.Command.EndAngle = angle;
		}
		
		protected override void Finish (Keys modifiers, IList<Rectangle> updates)
		{
			if (IsSquare) {
				ApplyStyles ();
				var command = RipCommands.Create<Commands.Arc> (Document);
				command.Point = this.Command.Point;
				command.Radius = this.Command.Radius.Width;
				command.StartAngle = this.Command.StartAngle;
				command.EndAngle = this.Command.EndAngle;
				Handler.AddCommand (command, updates);
				Command = null;
			} else
				base.Finish (modifiers, updates);
			
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			layout.Add (Separator ());
			layout.Add (new Controls.LineStylePad(Handler, false));
			return layout;
		}
		
	}
}