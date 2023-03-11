using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo.Drawing
{
	public abstract class ColourBox : Drawable
	{
		public abstract Color Color { get; set; }

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
			var rect = new Rectangle(Size);
			pe.Graphics.FillRectangle(Color, rect);
			pe.Graphics.DrawButtonOutline(rect);
		}
	}
}