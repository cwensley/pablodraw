using System;
using Eto.Drawing;
using System.Reflection;
using Eto.Forms;

namespace Pablo.Formats.Rip.Tools
{
	public class InkDropper : RipTool
	{
		RipTool oldTool;
		
		public override string Description {
			get { return "Selects the foreground (left click) or background (right click) color from under the mouse (I)"; }
		}

		public override Key Accelerator {
			get {
				return Key.I;
			}
		}

		public override Eto.Drawing.Image Image { get { return Bitmap.FromResource ("Pablo.Formats.Rip.Icons.InkDropper.png"); } }
		
		public override void Selecting ()
		{
			base.Selecting ();
			oldTool = Handler.SelectedTool;
		}
		
		void SetOldTool (Key modifiers)
		{
			if (oldTool != null && !modifiers.HasFlag (Key.Shift)) {
				Handler.SelectedTool = oldTool;
				oldTool = null;
			}
		}
		
		bool SetColor (Eto.Forms.MouseEventArgs e)
		{
			switch (e.Buttons) {
			case MouseButtons.Primary:
				Handler.Foreground = this.BGI.GetPixel ((Point)e.Location);
				e.Handled = true;
				return true;
			case MouseButtons.Alternate:
				Handler.Background = this.BGI.GetPixel ((Point)e.Location);
				e.Handled = true;
				return true;
			}
			return false;
		}
		
		public override void OnMouseDown (Eto.Forms.MouseEventArgs e)
		{
			if (!SetColor (e))
				base.OnMouseDown (e);
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			if (!SetColor (e))
				base.OnMouseMove (e);
		}
		
		public override void OnMouseUp (MouseEventArgs e)
		{
			if (SetColor (e))
				SetOldTool (e.Modifiers);
			else
				base.OnMouseUp (e);
		}
		
		public override void ApplyDrawing (System.Collections.Generic.IList<Rectangle> updates)
		{
			
		}
		
		public override void RemoveDrawing (System.Collections.Generic.IList<Rectangle> updates)
		{
			
		}
	}
}

