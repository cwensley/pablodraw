using System;
using System.Reflection;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public class InkDropper : CharacterTool
	{
		CharacterTool oldTool;

		public override string Description {
			get { return "Ink Dropper - Selects the foreground (left click) or background (right click) color from under the mouse"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.I | (Handler.Generator.IsMac ? Keys.Control : Keys.Alt);
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.InkDropper.png"); }
		}

		public override void Selecting ()
		{
			base.Selecting ();
			oldTool = Handler.SelectedTool;
		}
		
		public override Cursor MouseCursor {
			get { return new Cursor (CursorType.Crosshair); }
		}
		
		
		void SetOldTool (Keys modifiers)
		{
			if (oldTool != null && !modifiers.HasFlag (Keys.Shift)) {
				Handler.SelectedTool = oldTool;
				oldTool = null;
			}
		}

		bool SetColor (Eto.Forms.MouseEventArgs e)
		{
			var location = GetLocation ((Point)e.Location);
			var newAttr = Handler.CurrentPage.Canvas[location].Attribute;
			var curAttr = Handler.DrawAttribute;
			switch (e.Buttons) {
			case MouseButtons.Primary:
				newAttr.Background = curAttr.Background;
				Handler.DrawAttribute = newAttr;
				e.Handled = true;
				return true;
			case MouseButtons.Alternate:
				newAttr.Foreground = curAttr.Foreground;
				Handler.DrawAttribute = newAttr;
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
	}
}

