using System;
using Eto.Drawing;
using System.Reflection;
using Eto.Forms;
using System.Collections.Generic;
using Pablo.BGI;

namespace Pablo.Formats.Rip.Tools
{
	public class Text : SimpleTool<Commands.OutTextXY>
	{
		Point? currentPoint;
		BGICanvas.BGIImage saved;

		public override string Description {
			get { return "Draws text (T)"; }
		}
		
		public override Eto.Drawing.Image Image {
			get { return Bitmap.FromResource("Pablo.Formats.Rip.Icons.Text.png"); }
		}
		
		public override Key Accelerator {
			get { return Key.T; }
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
				yield return Document.Create<Commands.FontStyle>();
			}
		}

		public override bool AllowToolShortcuts
		{
			get { return Command == null; }
		}
		
		protected override bool ShouldApplyDrawing {
			get { return true; }
		}
		
		protected override bool InvertDrawing {
			get { return false; }
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates)
		{
			Point? point = currentPoint;
			var font = this.BGI.LoadFont (this.BGI.GetFontType ());
			if (Command != null) {
				base.ApplyInvertedDrawing (updates);
				
				if (!string.IsNullOrEmpty (this.Command.Text)) {
					var fontSize = font.GetTextSize (this.Command.Text, this.BGI.GetTextDirection (), this.BGI.GetFontSize ());
					point = this.BGI.GetTextDirection () == BGICanvas.Direction.Horizontal 
						? new Point (Command.Point.X + fontSize.Width, Command.Point.Y)
						: Command.Point;
				} else
					point = this.Command.Point;
			}
			if (point != null) {
				var oldwm = this.BGI.GetWriteMode ();
				var oldthickness = this.BGI.GetLineThickness ();
				var oldcol = this.BGI.GetColor ();
				
				this.BGI.SetLineThickness (1);
				this.BGI.SetColor ((byte)(this.Command != null ? 8 : 14));
				this.BGI.SetWriteMode (BGICanvas.WriteMode.Xor);
				var charSize = font.GetMaxCharacterSize (this.BGI.GetTextDirection (), this.BGI.GetFontSize ());
				this.BGI.Rectangle (new Rectangle (point.Value, charSize), updates);
				
				this.BGI.SetColor (oldcol);
				this.BGI.SetLineThickness (oldthickness);
				this.BGI.SetWriteMode (oldwm);
			}
		}
		
		public override void ApplyDrawing (IList<Rectangle> updates)
		{
			if (ShouldApplyDrawing && !Applied) {
				PushStyles ();
				
				if (Command != null && !string.IsNullOrEmpty (this.Command.Text)) {
					// save region
					var font = this.BGI.LoadFont (this.BGI.GetFontType ());
					var fontSize = font.GetTextSize (this.Command.Text, this.BGI.GetTextDirection (), this.BGI.GetFontSize ()) + 1;
					saved = this.BGI.GetImage (new Rectangle (this.Command.Point, fontSize));
				}
				ApplyInvertedDrawing (updates);
				Applied = true;
			}
		}
		
		public override void RemoveDrawing (IList<Rectangle> updates)
		{
			if (ShouldApplyDrawing && Applied) {
				ApplyInvertedDrawing (updates);
				if (saved != null) {
					this.BGI.PutImage (saved.Origin, saved, BGICanvas.WriteMode.Copy, updates);
					saved = null;
				}
				PopStyles ();
				Applied = false;
			}
		}

		public override void OnMouseMove (MouseEventArgs e)
		{
			if (Command == null) {
				var updates = new List<Rectangle> ();
				RemoveDrawing (updates);
				currentPoint = (Point)e.Location;
				ApplyDrawing (updates);
				this.BGI.UpdateRegion (updates);
				e.Handled = true;
			} else
				base.OnMouseMove (e);
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			var updates = new List<Rectangle> ();
			if (e.KeyData == Key.Enter) {
				if (Command != null) {
					var font = this.BGI.LoadFont (this.BGI.GetFontType ());
					var fontSize = font.GetMaxCharacterSize (this.BGI.GetTextDirection (), this.BGI.GetFontSize ());
					var oldPoint = this.Command.Point;
					RemoveDrawing (updates);
					if (!string.IsNullOrEmpty (this.Command.Text))
						FinishCommand (e.Modifiers, updates);
					
					CreateCommand ();
					this.Command.Point = Handler.TextDirection == BGICanvas.Direction.Horizontal 
						? new Point (oldPoint.X, oldPoint.Y + fontSize.Height)
						: new Point (oldPoint.X + fontSize.Width, oldPoint.Y);
					ApplyDrawing (updates);
					e.Handled = true;
				}
			} else {
				base.OnKeyDown (e);
				if (!e.Handled && Command != null) {
					
					if (e.KeyData == Key.Backspace) {
						RemoveDrawing (updates);
						if (!string.IsNullOrEmpty (this.Command.Text))
							this.Command.Text = this.Command.Text.Substring (0, this.Command.Text.Length - 1);
						ApplyDrawing (updates);
						e.Handled = true;
					}
					else if (e.IsChar) {
						RemoveDrawing (updates);
						if (this.Command.Text == null)
							this.Command.Text = e.KeyChar.ToString ();
						else
							this.Command.Text += e.KeyChar;
						ApplyDrawing (updates);
						e.Handled = true;
					}
				}
			}
			Handler.FlushCommands (updates);
		}
		
		public override void OnMouseUp (Eto.Forms.MouseEventArgs e)
		{
			switch (e.Buttons) {
			case MouseButtons.Primary:
				var updates = new List<Rectangle> ();
				RemoveDrawing (updates);
				CreateCommand ();
				this.currentPoint = null;
				this.Command.Point = (Point)e.Location;
				ApplyDrawing (updates);
				Handler.FlushCommands (updates);
				e.Handled = true;
				break;
			default:
				base.OnMouseUp (e);
				break;
			}
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout(Padding.Empty);
			layout.Add(Separator());
			layout.Add (new Controls.FontStylePad (this.Handler));
			return layout;
		}
	}
}

