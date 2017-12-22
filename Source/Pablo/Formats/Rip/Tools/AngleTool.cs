using System;
using Eto.Forms;
using System.Collections;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.BGI;
using System.Reflection;

namespace Pablo.Formats.Rip.Tools
{
	public abstract class AngleTool<T> : CenterAspectTool<T>
		where T: RipCommand
	{
		bool adjustingArc;
		Point? startPoint;
		Point? endPoint;
		Point middlePoint;
		Size radius;
		bool adjustingStart = true;
		
		protected abstract void SetCenter (Point point, Rectangle rect);
		
		sealed protected override void SetPosition (Rectangle rect)
		{
			middlePoint = Point.Max (Point.Empty, rect.Center);
			radius = rect.Size / 2;
			SetCenter (middlePoint, rect);
		}
		
		protected abstract void SetStartAngle (int angle);
		
		protected abstract void SetEndAngle (int angle);
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			if (adjustingArc) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					UpdateArc ((Point)e.Location, e.Modifiers);
					adjustingStart = false;
					e.Handled = true;
					break;
				}
			} else
				base.OnMouseDown (e);
		}
		
		int GetAngle (Point start, Point end)
		{
			var offset = new Point(end - start);

			var angle = BGICanvas.Round (Math.Atan ((double)offset.Y * radius.Width / radius.Height / offset.X) * (180.0 / Math.PI));

			if (offset.Y < 0 && offset.X < 0)
				angle = 180 - angle;
			else if (offset.Y > 0 && offset.X < 0)
				angle = 180 - angle;
			else if (offset.Y > 0 && offset.X > 0)
				angle = 360 - angle;
			else
				angle = -angle;
			return angle;
		}
		
		void UpdateArc (Point location, Key modifiers)
		{
			if (Command == null)
				return;
			var updates = new List<Rectangle> ();
			RemoveDrawing (updates);
			var angle = GetAngle (middlePoint, location);
			if (adjustingStart) {
				startPoint = location;
				SetStartAngle (angle);
			} else {
				endPoint = location;
				SetEndAngle (angle);
			}

			ApplyDrawing (updates);
			Handler.FlushCommands (updates);
			
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			if (adjustingArc) {
				UpdateArc ((Point)e.Location, e.Modifiers);
			} else
				base.OnMouseMove (e);
		}
		
		public override void OnMouseUp (MouseEventArgs e)
		{
			if (adjustingArc) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					adjustingStart = false;
					if (startPoint != null && startPoint.Value != e.Location) {
						UpdateArc ((Point)e.Location, e.Modifiers);
						DoFinish (e.Modifiers);
					} else 
						UpdateArc ((Point)e.Location, e.Modifiers);
					break;
				}
			} else
				base.OnMouseUp (e);
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates)
		{
			base.ApplyInvertedDrawing (updates);
			ApplyHandles(updates);
		}
		
		protected void ApplyHandles(IList<Rectangle> updates)
		{
			var oldcol = this.BGI.GetColor ();
			var oldlinestyle = this.BGI.GetLineStyle ();
			
			this.BGI.SetColor (1);
			this.BGI.SetLineStyle (BGICanvas.LineStyle.Dashed);
			if (startPoint != null)
				this.BGI.Line (middlePoint, startPoint.Value, updates);
			if (endPoint != null)
				this.BGI.Line (middlePoint, endPoint.Value, updates);
			this.BGI.SetLineStyle (oldlinestyle);
			this.BGI.SetColor (oldcol);
		}
		
		sealed protected override void FinishCommand (Key modifiers, IList<Eto.Drawing.Rectangle> updates)
		{
			RemoveDrawing (updates);
			adjustingArc = true;
			ApplyDrawing (updates);
		}
		
		void DoFinish(Key modifiers)
		{
			var updates = new List<Rectangle>();
			RemoveDrawing (updates);
			Finish (modifiers, updates);
			Handler.FlushCommands(updates);
			Reset ();
		}
		
		protected virtual void Finish (Key modifiers, IList<Rectangle> updates = null)
		{
			base.FinishCommand (modifiers, updates);
		}

		void Reset ()
		{
			adjustingStart = true;
			startPoint = null;
			endPoint = null;
			adjustingArc = false;
		}
		
		public override void Unselected ()
		{
			base.Unselected ();
			Reset ();
		}
	}
}

