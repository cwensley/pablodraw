using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections;

namespace Pablo.Controls
{
	public class ToggleGrid : Drawable
	{
		BitArray bits;
		
		public Size GridSize { get; private set; }
		
		public bool Invert { get; set; }
		
		public BitArray Bits
		{
			get { return bits; }
			set {
				bits = value;
				OnBitsChanged (EventArgs.Empty);
				Invalidate ();
			}
		}
		
		public event EventHandler<EventArgs> BitsChanged;
		
		protected virtual void OnBitsChanged (EventArgs e)
		{
			if (BitsChanged != null)
				BitsChanged (this, e);
		}
		
		public ToggleGrid (Size size)
		{
			this.GridSize = size;
			this.Size = this.GridSize * 16;
		}
		
		protected override void OnPaint (PaintEventArgs pe)
		{
			base.OnPaint (pe);
			pe.Graphics.FillRectangle (Color.FromGrayscale (0.8f), pe.ClipRectangle);
			if (Bits == null)
				return;
			var size = this.Size / this.GridSize;
			for (int y=0; y< GridSize.Height; y++) {
				for (int x = 0; x < GridSize.Width; x++) {
					var xofs = Invert ? x / 8 * 8 + (7 - x % 8) : x;
					if (Bits [y * GridSize.Width + xofs]) {
						pe.Graphics.FillRectangle (Colors.Black, new Rectangle (new Point (x, y) * size, size));
					}
				}
			}
		}
		
		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
		}
		
		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary) {
				var item = (Point)e.Location / (this.Size / this.GridSize);
				var xofs = Invert ? item.X / 8 * 8 + (7 - item.X % 8) : item.X;
				var index = item.Y * GridSize.Width + xofs;
				if (Bits != null && index < Bits.Length) {
					Bits [index] = !Bits [index];
					OnBitsChanged (EventArgs.Empty);
					Invalidate ();
					e.Handled = true;
				}
			} else 
				base.OnMouseUp (e);
		}
	}
}

