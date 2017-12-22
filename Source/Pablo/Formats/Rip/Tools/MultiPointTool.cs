using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Rip.Tools
{
	public abstract class MultiPointTool<T> : TwoPointTool<T>
		where T: RipCommand
	{
		List<Point> points;
		bool lastPoint;
		bool enableMulti;
		
		bool EnableMulti (Keys modifiers)
		{
			return enableMulti ^ (modifiers == Keys.Shift);
		}
		
		protected abstract void ApplyPoints (List<Point> points);

		protected override void SetStartLocation (Point start, Keys modifiers, Point location)
		{
			points = new List<Point> ();
			points.Add (Point.Max (Point.Empty, start));
			ApplyPoints (points);
			lastPoint = false;
		}
		
		public override void Unselected ()
		{
			base.Unselected ();
			lastPoint = false;
		}
		
		protected override void SetEndLocation (Point end, Keys modifiers, Point location)
		{
			if (!lastPoint) {
				AddPoint (end);
				lastPoint = true;
			} else {
				points [points.Count - 1] = Point.Max (Point.Empty, end);
				ApplyPoints (points);
			}
		}
		
		void AddPoint (Point point)
		{
			points.Add (Point.Max (Point.Empty, point));
			ApplyPoints (points);
		}
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			if (Command != null) {
				if (e.Buttons == MouseButtons.Primary)
					lastPoint = false;
			}
			else
				base.OnMouseDown (e);
		}
		
		public override void OnMouseUp (MouseEventArgs e)
		{
			if (Command != null) {
				var updates = new List<Rectangle> ();
				switch (e.Buttons) {
				case MouseButtons.Alternate:
					RemoveDrawing (updates);
					if (lastPoint) {
						points.RemoveAt (points.Count - 1);
						ApplyPoints (points);
					}
					FinishCommand (e.Modifiers, updates);
					Handler.FlushCommands (updates);
					e.Handled = true;
					break;
				/*case MouseButtons.Primary:
					if (AllowSameEndPoint || e.Location != Start) {
						RemoveDrawing (updates);
						AddPoint (e.Location);
						ApplyDrawing (updates);
						BGI.UpdateRegion (updates);
					}
					e.Handled = true;
					break;*/
				}
				
			} else
				base.OnMouseUp (e);
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			switch (e.Buttons) {
			case MouseButtons.Primary:
				if (Command != null && EnableMulti (e.Modifiers)) {
					var updates = new List<Rectangle> ();
					RemoveDrawing (updates);
					AddPoint ((Point)e.Location);
					ApplyDrawing (updates);
					Handler.FlushCommands (updates);
					e.Handled = true;
				} else 
					base.OnMouseMove (e);
				break;
			default:
				base.OnMouseMove (e);
				break;
			}
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };

			layout.Add(TopSeparator());
			layout.BeginVertical (Padding.Empty);
			layout.BeginHorizontal ();
			
			var b = new ImageButton{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.MultiPoint-Multi.png"),
				Toggle = true,
				Pressed = enableMulti
			};
			b.Click += delegate {
				enableMulti = b.Pressed;
			};
			
			layout.Add (b);
			layout.Add (null);
			layout.EndHorizontal ();
			layout.EndVertical();
			
			return layout;
		}
		
	}
}

