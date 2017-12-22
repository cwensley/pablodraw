using System;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo.Formats.Character.Controls
{
	public class ColourSelection : Drawable
	{
		#region Members

		Rectangle[,] positions;
		Palette pal;
		Attribute attribute;
		bool iceColours;
		Size offset;
		bool showForeground = true;
		bool showBackground = true;
		string foreText = string.Empty;
		string backText = string.Empty;
		bool foreSelected = true;
		Font fnum;
		Font font;
		int fontHeight;

		#endregion

		#region Properties

		public bool ShowForeground {
			get { return showForeground; }
			set {
				showForeground = value;
				Invalidate ();
			}
		}

		public bool ShowBackground {
			get { return showBackground; }
			set {
				showBackground = value;
				if (!showBackground)
					foreSelected = true;
				Invalidate ();
			}
		}

		public Attribute Attribute {
			get { return attribute; }
			set {
				attribute = value;
				Invalidate ();
			}
		}
		
		public bool iCEColours {
			get { return iceColours; }
			set {
				iceColours = value;
				Invalidate ();
			}
		}

		#endregion

		public event EventHandler<EventArgs> Selected;
		
		protected virtual void OnSelected (EventArgs e)
		{
			if (Selected != null)
				Selected (this, e);
		}
		
		public event EventHandler<EventArgs> Changed;
		
		protected virtual void OnChanged (EventArgs e)
		{
			if (Changed != null)
				Changed (this, e);
		}
		
		public ColourSelection (Palette pal, Attribute attribute)
		{
			this.CanFocus = true;
			this.pal = pal;
			this.attribute = attribute;
		}
		
		public override void OnSizeChanged (EventArgs e)
		{
			base.OnSizeChanged (e);
			Invalidate ();
		}
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
			Focus ();
		}
		
		public override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus (e);
			Invalidate ();
		}
		
		public override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
			Invalidate ();
		}
		
		public override void OnKeyDown (KeyEventArgs e)
		{
			if (!e.Handled && ShowBackground) {
				switch (e.KeyData) {
				case Key.Right:
					if (attribute.Background < pal.Count - 1)
						attribute.Background++;
					e.Handled = true;
					break;
				case Key.Left:
					if (attribute.Background > 0)
						attribute.Background--;
					e.Handled = true;
					break;
				}
				if (e.Handled)
					backText = attribute.Background.ToString ();
			}
			if (!e.Handled && ShowForeground) {
				switch (e.KeyData) {
				case Key.Up:
					if (attribute.Foreground < pal.Count - 1)
						attribute.Foreground++;
					e.Handled = true;
					break;
				case Key.Down:
					if (attribute.Foreground > 0)
						attribute.Foreground --;
					e.Handled = true;
					break;
				}
				if (e.Handled)
					foreText = attribute.Foreground.ToString ();
			}

			if (!e.Handled) {
				if (foreSelected && ShowForeground) {
					ParseValue (ref foreText, e);
					if (e.Handled && foreText.Length > 0)
						attribute.Foreground = Convert.ToByte (foreText);
					if (e.KeyData == Key.Enter) {
						e.Handled = true;
						if (ShowBackground)
							foreSelected = false;
						else
							OnSelected (EventArgs.Empty);
					}
				} else if (ShowBackground) {
					ParseValue (ref backText, e);
					if (e.Handled && backText.Length > 0)
						attribute.Background = Convert.ToByte (backText);
					if (!e.Handled && e.KeyData == Key.Backspace && backText.Length == 0) {
						foreSelected = true;
						e.Handled = true;
					}
					if (e.KeyData == Key.Enter) {
						e.Handled = true;
						OnSelected (EventArgs.Empty);
					}
				}
			}

			if (e.Handled) {
				OnChanged (EventArgs.Empty);
				Invalidate ();
			}
			
			base.OnKeyDown (e);
		}

		private void ParseValue(ref string val, KeyEventArgs e)
		{
			if (e.KeyData == Key.Backspace) {
				if (val.Length > 0) {
					val = val.Remove (val.Length - 1);
					e.Handled = true;
				}
			} else if (e.KeyData >= Key.D0 && e.KeyData <= Key.D9) {
				string newval = val + Convert.ToString ((int)(e.KeyData - Key.D0));
				int num = Convert.ToInt32 (newval);
				if (num >= 0 && num < pal.Count) {
					val = newval;
					e.Handled = true;
				}
			}
		}

		private Attribute CalculatePositions (int fontHeight)
		{
			positions = new Rectangle[pal.Count, pal.Count];
			Size clientSize = Size;
			clientSize.Height -= fontHeight;
			
			float ratio = 16F / (float)pal.Count;
			ratio *= ratio;

			float divx = 1300F / clientSize.Width / (16F / (float)pal.Count);
			float divy = 1300F / clientSize.Height / (16F / (float)pal.Count);
			
			Attribute attr = attribute;
			if (!ShowBackground)
				attr.Background = (byte)(pal.Count / 2);
			if (!ShowForeground)
				attr.Foreground = (byte)(pal.Count / 2);
			
			Point pt = new Point (0, 0);
			
			for (int fg=positions.GetLength(0)-1; fg>=0; fg--) {
				int fgofs = pal.Count - 1 - Math.Abs (attr.Foreground - fg);
				int fgwidth = (int)(fgofs * fgofs / divx * ratio);
				int fgheight = (int)(fgofs * fgofs / divy * ratio);
				
				Point ptbg = pt;
				for (int bg=0; bg<positions.GetLength(1); bg++) {
					int bgofs = pal.Count - 1 - Math.Abs (attr.Background - bg);
					int curheight = (bg == attr.Background) ? fgheight : (int)(bgofs * bgofs / divy * ratio);
					int curwidth = (fg == attr.Foreground) ? (int)(bgofs * bgofs / divx * ratio) : fgwidth;
					var rect = new Rectangle (ptbg.X, ptbg.Y, curwidth, curheight);
					rect.Normalize ();
					positions [fg, bg] = rect;
					ptbg.X += curwidth;
				}

				pt.Y += fgheight;
			}
			
			int maxx = positions [attr.Foreground, pal.Count - 1].InnerRight;
			int maxy = positions [0, attr.Background].InnerBottom;
			offset = new Size ((clientSize.Width - maxx) / 2, (clientSize.Height - maxy) / 2);
			
			return attr;
		}
		
		public override void OnPaint (PaintEventArgs pe)
		{
			base.OnPaint (pe);
			Graphics graphics = pe.Graphics;
			Rectangle rect;
			Size clientSize = Size;
			
			float fontSize = 10F * clientSize.Width / 220F;
			fontSize = Math.Max (1.0F, fontSize);
			if (font == null || font.Size != fontSize) {
				font = Fonts.Sans (fontSize);

				fnum = Fonts.Sans (fontSize, FontStyle.Bold);
				fontHeight = CalculateFontHeight (graphics, font);
			}

			graphics.FillRectangle (Colors.Black, 0, 0, clientSize.Width, clientSize.Height);

			Attribute attr = CalculatePositions (fontHeight);
			Rectangle current = positions [attr.Foreground, attr.Background];

			if (ShowForeground) {
				for (int fg = 0; fg < pal.Count; fg++) {
					if (fg != attr.Foreground) {
						rect = positions [fg, attr.Background];
						rect.X = current.X + (current.Width - rect.Width) / 2;
						rect.Offset (offset);
						graphics.FillRectangle (pal [fg], rect);
					}
				}
			}

			if (ShowBackground) {
				for (int bg = 0; bg < pal.Count; bg++) {
					var curbg = bg;
					if (!iceColours) curbg %= 8;
					if (bg != attr.Background) {
						rect = positions [attr.Foreground, bg];
						rect.Y = current.Y + (current.Height - rect.Height) / 2;
						rect.Offset (offset);
						graphics.FillRectangle (pal [curbg], rect);
					}
				}
			}

			rect = current;
			rect.Offset (offset);
			
			if (ShowBackground) {
				var curbg = attr.Background;
				if (!iceColours) curbg %= 8;
				graphics.FillRectangle (pal [curbg], rect);
				rect.Inflate (-rect.Width / 5, -rect.Height / 5);
			}
			
			if (ShowForeground) {
				graphics.FillRectangle (pal [attr.Foreground], rect);
			}
			if (HasFocus) {
				rect = current;
				rect.Offset (offset);
				graphics.DrawRectangle (Colors.DarkGray, rect);
				
			}
			var numrect = new Rectangle (0, clientSize.Height - fontHeight, clientSize.Width, fontHeight);
			numrect.Inflate (-4, 0);
			DrawNumericColours (graphics, numrect, font);
		}

		int CalculateFontHeight (Graphics graphics, Font font)
		{
			return Size.Round (graphics.MeasureString (font, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")).Height + 4;
		}

		void DrawNumericColours (Graphics graphics, Rectangle rect, Font font)
		{
			string text;
			Point pos = rect.TopLeft;

			graphics.FillRectangle (Colors.Black, 0, pos.Y, rect.Width, rect.Height + 2);

			if (this.ShowForeground) {
				text = "Foreground: ";
				DrawString (graphics, font, Colors.White, text, ref pos);
				DrawString (graphics, fnum, Colors.DarkGray, foreText, ref pos);
				if (HasFocus && this.foreSelected)
					DrawString (graphics, fnum, Colors.DarkGray, "_", ref pos);
			}

			if (this.ShowBackground) {
				pos.X = rect.Width / 2;
				text = "Background: ";
				DrawString (graphics, font, Colors.White, text, ref pos);
				text = this.Attribute.Background.ToString ();
				DrawString (graphics, fnum, Colors.DarkGray, backText, ref pos);
				if (HasFocus && (!this.foreSelected || !this.ShowForeground))
					DrawString (graphics, fnum, Colors.DarkGray, "_", ref pos);
			}
		}

		void DrawString (Graphics graphics, Font font, Color color, string text, ref Point point)
		{
			SizeF size = graphics.MeasureString (font, text);
			graphics.DrawText (font, color, point.X, point.Y, text);
			point.X += (int)size.Width;
		}
	}
}
