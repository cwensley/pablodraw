using System;
using Eto.Drawing;
using System.Reflection;
using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class Bezier : TwoPointTool<Commands.Bezier>
	{
		int segments = 100;
		int? movingPoint;
		bool moving;
		Point? connectingPoint;

		public override string Description {
			get { return "Draws a curved line with four control points (Z)"; }
		}
		
		public override Keys Accelerator {
			get {
				return Keys.Z;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get {
				return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Bezier.png");
			}
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
			start = Point.Max (Point.Empty, start);
			this.Command.Points = new Point[4];
			this.Command.Points [0] = start;
			this.Command.Segments = segments;
		}
		
		protected override void SetEndLocation (Point end, Keys modifiers, Point location)
		{
			end = Point.Max (Point.Empty, end);
			var start = this.Command.Points [0];
			var inc = (end - start) / 3;
			this.Command.Points [1] = connectingPoint ?? start + inc;
			this.Command.Points [2] = start + inc * 2;
			this.Command.Points [3] = end;
		}
		
		void FindClosestPoint (Point location)
		{
			double distance = double.MaxValue;
			for (int i = 0; i<this.Command.Points.Length; i++) {
				var point = this.Command.Points [i];
			
				var dist = Point.Distance (point, location);
				if (dist < distance) {
					distance = dist;
					movingPoint = i;
				}
			}
		}
		
		public override void Unselected ()
		{
			base.Unselected ();
			moving = false;
			movingPoint = null;
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			base.ApplyInvertedDrawing (updates);
			
			var points = this.Command.Points;
			var oldcol = this.BGI.GetColor ();
			var oldlinestyle = this.BGI.GetLineStyle ();
			var oldthickness = this.BGI.GetLineThickness ();
			this.BGI.SetColor (1);
			this.BGI.SetLineStyle (Pablo.BGI.BGICanvas.LineStyle.Dotted);
			this.BGI.SetLineThickness (1);
			this.BGI.DrawPoly (new Point[] {
				points [0],
				points [1],
				points [3],
				points [2]
			}, updates);
			this.BGI.SetColor (2);
			this.BGI.SetLineStyle (Pablo.BGI.BGICanvas.LineStyle.Solid);
			this.BGI.SetLineThickness (oldthickness);
			foreach (var point in points) {
				this.BGI.Ellipse (new Rectangle (point - 3, new Size (6, 6)), updates);
			}
			
			this.BGI.SetColor (oldcol);
			this.BGI.SetLineStyle (oldlinestyle);
			
			
		}
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			if (moving) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					FindClosestPoint ((Point)e.Location);
					break;
				}
			} else 
				base.OnMouseDown (e);
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			if (moving) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					if (movingPoint != null) {
						var updates = new List<Rectangle> ();
						RemoveDrawing (updates);
						this.Command.Points [movingPoint.Value] = Point.Max (Point.Empty, (Point)e.Location);
						ApplyDrawing (updates);
						Handler.FlushCommands (updates);
						e.Handled = true;
					}
					break;
				}
			} else
				base.OnMouseMove (e);
		}
		
		public override void OnMouseUp (MouseEventArgs e)
		{
			if (moving) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					if (movingPoint != null) {
						var updates = new List<Rectangle> ();
						
						RemoveDrawing (updates);
						this.Command.Points [movingPoint.Value] = Point.Max (Point.Empty, (Point)e.Location);
						ApplyDrawing (updates);
						Handler.FlushCommands (updates);
						movingPoint = null;
					}
					break;
				case MouseButtons.Alternate:
					Finish (e.Modifiers, (Point)e.Location);
					break;
				}
			} else
				base.OnMouseUp (e);
		}
		
		void Finish (Keys modifiers, Point location)
		{
			Start = this.Command.Points [3];
			connectingPoint = Start + (Start - this.Command.Points[2]);
			var updates = new List<Rectangle> ();
			moving = false;
			movingPoint = null;
			RemoveDrawing (updates);
			base.FinishCommand (modifiers, updates);
			CreateCommand ();
			SetStartLocation (Start, Keys.None, Start);
			SetEndLocation (location, Keys.None, location);
			ApplyDrawing (updates);
			
			Handler.FlushCommands (updates);
			
		}
		
		protected override void FinishCommand (Keys modifiers, IList<Rectangle> updates = null)
		{
			moving = true;
			connectingPoint = null;
			ApplyDrawing (updates);
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };
			//layout.Container.MinimumSize = new Size (100, 20);

			layout.Add(TopSeparator());
			layout.Add (new Controls.LineStylePad (Handler, true));
			
			var font = new Font (SystemFont.Default, 7);
			layout.Add (new Label{ Text = "Segments", Font = font, TextAlignment = TextAlignment.Center });
			var segmentBox = new NumericStepper{
				Font = new Font (SystemFont.Default, 8),
				Value = segments,
				MinValue = 1,
				MaxValue = 999,
				Size = new Size(20, -1)
			};
			segmentBox.ValueChanged += delegate {
				segments = Math.Min (999, Math.Max (1, (int)segmentBox.Value));
				if (Command != null) {
					var updates = new List<Rectangle> ();
					RemoveDrawing (updates);
					Command.Segments = segments;
					ApplyDrawing (updates);
					Handler.FlushCommands (updates);
				}
			};
			layout.Add (segmentBox);

			layout.Add (null);
			
			return layout;
		}
		
	}
}

