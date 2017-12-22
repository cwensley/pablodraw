using System;
using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public abstract class TwoPointTool<T> : SimpleTool<T>
		where T: RipCommand
	{
		protected Point Start { get; set; }
		
		protected virtual bool AllowSameEndPoint {
			get { return false; }
		}

		protected virtual RipCommand DuplicateCommand ()
		{
			var clonable = Command as ICloneable;
			if (clonable != null)
				return clonable.Clone () as RipCommand;
			return null;
		}
		
		protected bool ApplyDuplicate ()
		{
			var dupe = DuplicateCommand ();
			if (dupe != null) {
				var updates = new List<Rectangle> ();
				RemoveDrawing (updates);

				ApplyStyles ();
				Handler.AddCommand (dupe, updates);
				Handler.FlushCommands (updates);
				return true;
			}
			return false;
		}
		
		
		
		protected abstract void SetStartLocation (Point start, Keys modifiers, Point location);

		protected abstract void SetEndLocation (Point end, Keys modifiers, Point location);
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			if (Command == null) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					CreateCommand ();
					Start = (Point)e.Location;
					SetStartLocation (Start, e.Modifiers, (Point)e.Location);
					e.Handled = true;
					break;
				}
			} /*else if (Applied) {
				FinishCommand (e);
				e.Handled = true;
			}*/
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			if (Command != null) {
				if (e.Key == Keys.S || e.Key == Keys.Space || e.Key == Keys.Enter) {
					if (ApplyDuplicate ())
						e.Handled = true;
				}
			}
			if (!e.Handled)
				base.OnKeyDown (e);
		}
		
		public override void OnMouseUp (MouseEventArgs e)
		{
			if (Command != null) {
				switch (e.Buttons) {
				case MouseButtons.Primary:
					var updates = new List<Rectangle> ();
					RemoveDrawing (updates);
					SetEndLocation ((Point)e.Location, e.Modifiers, (Point)e.Location);
					
					if (AllowSameEndPoint || e.Location != Start)
						FinishCommand (e.Modifiers, updates);
					Handler.FlushCommands (updates);
					e.Handled = true;
					break;
				case MouseButtons.Alternate:
					RemoveDrawing ();
					Command = null;
					break;
				}
			}
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			if (Command != null) {
				var updates = new List<Rectangle> ();
				RemoveDrawing (updates);
				
				SetEndLocation ((Point)e.Location, e.Modifiers, (Point)e.Location);
				
				ApplyDrawing (updates);
				Handler.FlushCommands (updates);
				e.Handled = true;
			}
		}
		
	}
}

