using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;

namespace Pablo.Drawing
{
	public static class DrawingHelpers
	{
		public static void DrawButtonOutline(this Graphics graphics, Rectangle rect, bool hover = false)
		{
			rect.Size -= 1;
			var col = hover 
				? SystemColors.Highlight // new Color(Colors.Gray, 0.4f)
				: new Color(Colors.Gray, 0.8f);
			graphics.DrawRectangle(col, rect);
			// graphics.DrawInsetRectangle(Colors.Gray, Colors.White, rect);
		}
		
		public static void DrawButton(this Graphics graphics, Rectangle rect, bool enabled = true, bool hover = false, bool selected = false)
		{
			if (!enabled)
				return;
			if (selected)
			{
				var col = Color.FromGrayscale(0.6f, 0.8f);
				graphics.FillRectangle(col, rect);
				graphics.DrawButtonOutline(rect, hover);
			}
			else if (hover)
			{
				var col = Color.FromGrayscale(0.6f, 0.4f);
				graphics.FillRectangle(col, rect);
				graphics.DrawButtonOutline(rect, hover);
			}
			
		}
	}
}