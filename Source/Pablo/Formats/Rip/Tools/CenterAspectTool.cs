using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.BGI;

namespace Pablo.Formats.Rip.Tools
{
	public abstract class CenterAspectTool<T> : TwoPointTool<T>
		where T: RipCommand
	{
		sealed protected override void SetStartLocation (Point start, Key modifiers, Point location)
		{
		}
		
		protected bool IsSquare { get; private set; }
		protected bool IsCentered { get; private set; }

		protected abstract void SetPosition(Rectangle rect);
		
		sealed protected override void SetEndLocation (Point end, Key modifiers, Point location)
		{
			var start = Start;
			var rect = new Rectangle ();
			var aspect = BGICanvas.ASPECT;
			var size = Size.Abs (new Size(end - start));
			if (modifiers.HasFlag (Key.Shift)) {
				IsSquare = true;
				int diameter = Math.Max (size.Width, (int)Math.Round (size.Height / aspect));
				if (modifiers.HasFlag (Key.Alt)) {
					IsCentered = true;
					rect.Location = new Point(start - new Point (diameter, (int)Math.Round (diameter * aspect)));
					diameter *= 2;
				} else {
					IsCentered = false;
					rect.Location = new Point (
						(end.X < start.X) ? start.X - diameter : start.X,
						(end.Y < start.Y) ? start.Y - (int)Math.Round (diameter * aspect) : start.Y
						);
				}
				rect.Size = new Size (
					diameter + 1,
					(int)Math.Round (diameter * aspect) + 1
				);
				
			} else {
				IsSquare = false;
				if (modifiers.HasFlag (Key.Alt)) {
					IsCentered = true;
					rect.Location = start - size;
					size *= new Size (2, 2);
					rect.Size = size + 1;
				} else {
					IsCentered = false;
					rect.Size = size + 1;
					rect.Location = new Point (
						(end.X < start.X) ? start.X - size.Width : start.X,
						(end.Y < start.Y) ? start.Y - size.Height : start.Y
						);
				}
			}
			SetPosition(rect);
		}
	}
}

