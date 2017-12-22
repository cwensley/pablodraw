using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Pablo.BGI;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class Paste : RipTool
	{
		Point? point;
		
		bool applied;

		public override string Description {
			get { return "Pastes the contents of the clipboard (S)"; }
		}
		
		public override Key Accelerator {
			get {
				return Key.S;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Paste.png"); }
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			var updates = new List<Rectangle> ();
			RemoveDrawing (updates);
			point = Point.Max (Point.Empty, (Point)e.Location);
			ApplyDrawing (updates);
			Handler.FlushCommands (updates);
			e.Handled = true;
		}
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			switch (e.Buttons) {
			case MouseButtons.Primary:
				var updates = new List<Rectangle> ();
				ApplyInvertedDrawing (updates);
				point = null;
				if (Document.RipImage != null) {
					var command = Document.Create<Commands.PutImage>();
					command.Point = Point.Max (Point.Empty, (Point)e.Location);
					Handler.AddCommand (command, updates);
				}
				Handler.FlushCommands (updates);
				
				e.Handled = true;
				break;
			default:
				base.OnMouseDown (e);
				break;
			}
		}
		
		public override void Unselected ()
		{
			base.Unselected ();
			RemoveDrawing ();
			point = null;
		}
		
		protected void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			if (point == null) return;
			var pasteImage = Document.RipImage;
			if (pasteImage == null) return;
			var rect = new Rectangle (point.Value, pasteImage.Size);
			var oldmode = this.BGI.GetWriteMode ();
			var oldcol = this.BGI.GetColor ();
			var oldlinestyle = this.BGI.GetLineStyle ();
			this.BGI.SetColor (15);
			this.BGI.SetWriteMode (BGICanvas.WriteMode.Xor);
			this.BGI.SetLineStyle (BGICanvas.LineStyle.Dashed);
			this.BGI.Rectangle (rect, updates);
			
			this.BGI.SetColor (oldcol);
			this.BGI.SetLineStyle (oldlinestyle);
			this.BGI.SetWriteMode (oldmode);
		}
		
		public override void RemoveDrawing (IList<Rectangle> updates = null)
		{
			if (applied) {
				ApplyInvertedDrawing (updates);
				applied = false;
			}
		}
		
		public override void ApplyDrawing (IList<Rectangle> updates = null)
		{
			if (!applied) {
				ApplyInvertedDrawing (updates);
				applied = true;
			}
		}
	}
}

