using System;
using System.Linq;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Drawing
{
	public class ScanRow : List<int>
	{
		public int Row { get; set; }
			
	}
		
	public delegate void ScanLinesDrawDelegate(Rectangle rect);

	
	public class ScanLines : Dictionary<int, ScanRow>
	{
		const double rad2deg = 180.0 / Math.PI;
		const double deg2rad = Math.PI / 180.0;

		public int LineThickness { get; set; }
		
		public ScanLines ()
		{
			LineThickness = 1;
		}
		
		public void Add (params Point[] points)
		{
			foreach (var point in points) {
				Add (point);
			}
		}
		
		public bool PointIsInside (Point point)
		{
			ScanRow row;
			if (this.TryGetValue (point.Y, out row)) {
				return point.X >= row.Min() && point.X <= row.Max();
			}
			return false;
		}

		public bool PointIsDrawn (Point point)
		{
			ScanRow row;
			if (this.TryGetValue (point.Y, out row)) {
				return row.Contains (point.X);
			}
			return false;
		}
		
		public void Add (Point point)
		{
			Add (point.X, point.Y);
		}
			
		public void Add (int x, int y)
		{
			ScanRow row;
			if (!this.TryGetValue (y, out row)) {
				row = new ScanRow { Row = y };
				base.Add (y, row);
			}
			row.Add (x);
		}
		
		public void Add (int x1, int y1, int x2, int y2)
		{
			Add (x1, y1);
			Add (x2, y2);
		}
		
		public void ScanLine (Point start, Point end, bool full)
		{
			int ydelta = Math.Abs (end.Y - start.Y);

			if (full || start.Y < end.Y) {
				this.Add (start);
			}
			if (ydelta > 0) {
				int xdelta = (start.Y > end.Y) ? (start.X - end.X) : (end.X - start.X);
				int minX = (start.Y > end.Y) ? end.X : start.X;
				int posY = Math.Min (start.Y, end.Y);
				int posX;

				posY++;
				for (int count = 1; count < ydelta; count++) {
					posX = (xdelta * count / ydelta) + minX;
					this.Add (posX, posY);
					posY++;
				}
			}
			if (full || end.Y < start.Y) {
				this.Add (end);
			}
		}
		
		public void AddLine (Point start, Point end)
		{
			int lAdjUp, lAdjDown, lError, lAdvance;
			int lXDelta, lYDelta;
			int lWholeStep, lStartLength, lEndLength, lCount;
			int lRunLength;
			int lStep;
			Point pos;
			lYDelta = Math.Abs (end.Y - start.Y);
			lXDelta = Math.Abs (end.X - start.X);
			
			if (lXDelta == 0) {
				//FillY (x1, Math.Min (y1, y2), lYDelta + 1, ref offset, drawUpdates);
				AddVertical (start.X, Math.Min (start.Y, end.Y), lYDelta + 1);
			} else if (lYDelta == 0) {
				//FillX (y1, Math.Min (x1, x2), lXDelta + 1, ref offset, drawUpdates);
				AddHorizontal (Math.Min (start.X, end.X), start.Y, lXDelta + 1);
			} else if (lXDelta >= lYDelta) {
				lAdvance = 1;
				if (start.Y < end.Y) {
					pos = start;
					lStep = (start.X > end.X) ? -1 : 1;
				} else {
					pos = end;
					lStep = (end.X > start.X) ? -1 : 1;
				}

				lWholeStep = (lXDelta / lYDelta) * lStep;
				lAdjUp = (lXDelta % lYDelta);
				lAdjDown = lYDelta * 2;
				lError = lAdjUp - lAdjDown;
				lAdjUp *= 2;

				lStartLength = (lWholeStep / 2) + lStep;
				lEndLength = lStartLength;
				if ((lAdjUp == 0) && ((lWholeStep & 0x01) == 0)) {
					lStartLength -= lStep;
				}

				if ((lWholeStep & 0x01) != 0)
					lError += lYDelta;

				//FillX (pos.Y, pos.X, lStartLength, ref offset, drawUpdates);
				AddHorizontal (pos.X, pos.Y, lStartLength);
				pos.X += lStartLength;
				pos.Y += lAdvance;

				for (lCount = 0; lCount < (lYDelta-1); lCount++) {
					lRunLength = lWholeStep;
					if ((lError += lAdjUp) > 0) {
						lRunLength += lStep;
						lError -= lAdjDown;
					}
					AddHorizontal (pos.X, pos.Y, lRunLength);
					//FillX (pos.Y, pos.X, lRunLength, ref offset, drawUpdates);
					pos.X += lRunLength;
					pos.Y += lAdvance;
				}
				AddHorizontal (pos.X, pos.Y, lEndLength);
				//FillX (pos.Y, pos.X, lEndLength, ref offset, drawUpdates);
			} else if (lXDelta < lYDelta) {
				if (start.Y < end.Y) {
					pos = start;
					lAdvance = (start.X > end.X) ? -1 : 1;
				} else {
					pos = end;
					lAdvance = (end.X > start.X) ? -1 : 1;
				}

				lWholeStep = lYDelta / lXDelta;
				lAdjUp = (lYDelta % lXDelta);
				lAdjDown = lXDelta * 2;
				lError = lAdjUp - lAdjDown;
				lAdjUp *= 2;
				lStartLength = (lWholeStep / 2) + 1;
				lEndLength = lStartLength;
				if ((lAdjUp == 0) && ((lWholeStep & 0x01) == 0)) {
					lStartLength--;
				}
				if ((lWholeStep & 0x01) != 0) {
					lError += lXDelta;
				}
        
				//FillY (pos.X, pos.Y, lStartLength, ref offset, drawUpdates);
				AddVertical (pos.X, pos.Y, lStartLength);
				pos.Y += lStartLength;
				pos.X += lAdvance;

				for (lCount = 0; lCount < (lXDelta-1); lCount++) {
					lRunLength = lWholeStep;
					if ((lError += lAdjUp) > 0) {
						lRunLength++;
						lError -= lAdjDown;
					}

					AddVertical (pos.X, pos.Y, lRunLength);
					//FillY (pos.X, pos.Y, lRunLength, ref offset, drawUpdates);
					pos.Y += lRunLength;
					pos.X += lAdvance;
				}
				AddVertical (pos.X, pos.Y, lEndLength);
				//FillY (pos.X, pos.Y, lEndLength, ref offset, drawUpdates);
			}
		}
		
		bool InAngle (int angle, int start_angle, int end_angle)
		{
			return (angle >= start_angle && angle <= end_angle);
		}
		
		void SymmetryScan (int x, int y, int start_angle, int end_angle, int xoffset, int yoffset, int radiusx, int radiusy, int angle, bool horizontal)
		{
			if (LineThickness == 1) {
				if (InAngle (angle, start_angle, end_angle))
					this.Add (x + xoffset, y - yoffset);
				if (InAngle (180 - angle, start_angle, end_angle))
					this.Add (x - xoffset, y - yoffset);
				if (InAngle (180 + angle, start_angle, end_angle))
					this.Add (x - xoffset, y + yoffset);
				if (InAngle (360 - angle, start_angle, end_angle))
					this.Add (x + xoffset, y + yoffset);
			} else {
				int offset = LineThickness / 2;
				if (horizontal) {
					if (InAngle (angle, start_angle, end_angle))
						AddHorizontal (x + xoffset - offset, y - yoffset, LineThickness);
					if (InAngle (180 - angle, start_angle, end_angle))
						AddHorizontal (x - xoffset - offset, y - yoffset, LineThickness);
					if (InAngle (180 + angle, start_angle, end_angle))
						AddHorizontal (x - xoffset - offset, y + yoffset, LineThickness);
					if (InAngle (360 - angle, start_angle, end_angle))
						AddHorizontal (x + xoffset - offset, y + yoffset, LineThickness);
				} else {
					if (InAngle (angle, start_angle, end_angle))
						AddVertical (x + xoffset, y - yoffset - offset, LineThickness);
					if (InAngle (180 - angle, start_angle, end_angle))
						AddVertical (x - xoffset, y - yoffset - offset, LineThickness);
					if (InAngle (180 + angle, start_angle, end_angle))
						AddVertical (x - xoffset, y + yoffset - offset, LineThickness);
					if (InAngle (360 - angle, start_angle, end_angle))
						AddVertical (x + xoffset, y + yoffset - offset, LineThickness);
				}
				
			}
		}
		
		public void AddVertical (int x, int y, int count)
		{
			if (count > 0) {
				for (int i = 0; i < count; i++) {
					Add (x, y + i);
				}
			} else {
				for (int i = 0; i > count; i--) {
					Add (x, y + i);
				}
			}
		}

		public void AddHorizontal (int x, int y, int count)
		{
			if (count > 0) {
				for (int i = 0; i < count; i++) {
					Add (x + i, y);
				}
			} else {
				for (int i = 0; i > count; i--) {
					Add (x + i, y);
				}
			}
		}
		
		public void AddRect (Rectangle rectangle)
		{
			for (int y = rectangle.Top; y <= rectangle.InnerBottom; y++) {
				AddHorizontal (rectangle.X, y, rectangle.Width);
			}
		}
		
		public Rectangle AddEllipse (Rectangle rectangle, int start_angle = 0, int end_angle = 360)
		{
			if (rectangle.Width < 2) rectangle.Width = 2;
			if (rectangle.Height < 2) rectangle.Height = 2;
			if ((rectangle.Width % 2) == 0) rectangle.Width ++;
			if ((rectangle.Height % 2) == 0) rectangle.Height ++;
			AddEllipse(rectangle.MiddleX, rectangle.MiddleY, rectangle.Width / 2, rectangle.Height / 2, start_angle, end_angle);
			return rectangle;
		}
		
		public void AddEllipse (int x, int y, int radiusx, int radiusy, int start_angle, int end_angle)
		{
			// check if valid angles
			if (start_angle > end_angle) {
				var tt = start_angle;
				start_angle = end_angle;
				end_angle = tt;
			}

			radiusx = Math.Max (1, radiusx);
			radiusy = Math.Max (1, radiusy);
			
			int diameterx = radiusx * 2;
			int diametery = radiusy * 2;
			int b1 = diametery & 1; 
			long stopx = 4 * (1 - diameterx) * diametery * diametery;
			long stopy = 4 * (b1 + 1) * diameterx * diameterx; // error increment 
			long err = stopx + stopy + b1 * diameterx * diameterx; // error of 1 step 

			int xoffset = radiusx;
			int yoffset = 0;
			int incx = 8 * diameterx * diameterx;
			int incy = 8 * diametery * diametery;
			
			var aspect = (double)radiusx / (double)radiusy;
			
			// calculate horizontal fill angle
			var horizontal_angle = (radiusx < radiusy) ? 90.0 - (45.0 * aspect) : (45.0 / aspect);
			
			do {
				long e2 = 2 * err;
				var angle = Math.Atan ((double)yoffset * aspect / (double)xoffset) * rad2deg;
				
				SymmetryScan (x, y, start_angle, end_angle, xoffset, yoffset, radiusx, radiusy, (int)Math.Round (angle), angle <= horizontal_angle);
				if (Math.Abs (angle - horizontal_angle) < 1) {
					SymmetryScan (x, y, start_angle, end_angle, xoffset, yoffset, radiusx, radiusy, (int)Math.Round (angle), !(angle <= horizontal_angle));
				}
				
				if (e2 <= stopy) {
					yoffset++;
					err += stopy += incx;
				}
				if (e2 >= stopx) {
					xoffset--;
					err += stopx += incy;
				}
			} while (xoffset >= 0);
   
			xoffset++;
			while (yoffset < radiusy) {
				var angle = Math.Atan ((double)yoffset * aspect / (double)xoffset) * rad2deg;
				SymmetryScan (x, y, start_angle, end_angle, xoffset, yoffset, radiusx, radiusy, (int)Math.Round (angle), angle <= horizontal_angle);
				if (angle == horizontal_angle) {
					SymmetryScan (x, y, start_angle, end_angle, xoffset, yoffset, radiusx, radiusy, (int)Math.Round (angle), !(angle <= horizontal_angle));
				}
				yoffset++;
			}			
		}

		public void Fill (ScanLinesDrawDelegate draw)
		{
			foreach (var row in this.Values) {
				var minx = row.Min ();
				var width = row.Max () - minx + 1;
				var linerect = new Rectangle(minx, row.Row, width, 1);
				draw(linerect);
			}
		}
		
		
		public void Outline (ScanLinesDrawDelegate draw)
		{
			int lastx = 0;
			ScanRow row;
			var rows = new List<ScanRow>(this.Values);
			rows.Sort ((x, y) => x.Row.CompareTo (y.Row));
			// remove all innards
			foreach (var childrow in rows) {
				childrow.Sort ();
				if (childrow.Count > 2)
					childrow.RemoveRange (1, childrow.Count - 2);
			}
			
			//int maxy = rows.Select (r => r.Row).Max ();
			//int miny = rows.Select (r => r.Row).Min ();
			// trace min edge
			for (int i = 0; i < rows.Count; i++) {
				row = rows [i];
				var cury = row.Row;
				int curx = row.Min ();
				bool last = i == rows.Count - 1;
				bool first = i == 0;
				
				if (!first && !last) {
					var nextx = rows[i+1].Min ();
					if (curx < lastx) {
						draw (new Rectangle(new Point(curx, cury), new Point(lastx - 1, cury)));
					}
					else if (curx > lastx + 1) {
						draw (new Rectangle(new Point(lastx+1, cury-1), new Point(curx-1, cury-1)));
						draw (new Rectangle(new Point(curx, cury), new Point(curx, cury)));
					}
					else {
						draw (new Rectangle(new Point(curx, cury), new Point(curx, cury)));
					}
					if (nextx > curx)
						draw (new Rectangle(new Point(curx, cury), new Point(nextx - 1, cury)));
				}
				lastx = curx;
								
			}
			// trace max edge
			for (int i = 0; i < rows.Count; i++) {
				row = rows [i];
				var cury = row.Row;
				int curx = row.Max ();
				bool last = i == rows.Count - 1;
				bool first = i == 0;
				
				if (!first && !last) {
					var nextx = rows[i+1].Max ();
					if (curx > lastx) {
						draw (new Rectangle(new Point(lastx + 1, cury), new Point(curx, cury)));
					}
					else if (curx < lastx - 1) {
						draw (new Rectangle(new Point(curx+1, cury-1), new Point(lastx-1, cury-1)));
						draw (new Rectangle(new Point(curx, cury), new Point(curx, cury)));
					}
					else {
						draw (new Rectangle(new Point(curx, cury), new Point(curx, cury)));
					}
					if (nextx < curx)
						draw (new Rectangle(new Point(nextx + 1, cury), new Point(curx, cury)));
				}
				lastx = curx;
								
			}
			// fill top/bottom
			row = rows[0];
			draw(new Rectangle(new Point(row.Min(), row.Row), new Point(row.Max(), row.Row)));
			row = rows[rows.Count-1];
			draw(new Rectangle(new Point(row.Min(), row.Row), new Point(row.Max(), row.Row)));
		}
		
	}
}

