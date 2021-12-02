using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pablo.BGI
{
	public delegate void BGIUpdateEventHandler(object sender, Rectangle rect);

	unsafe public class BGICanvas : IDisposable
	{
		const double rad2deg = 180.0 / Math.PI;
		const double deg2rad = Math.PI / 180.0;
		public static Encoding Encoding = Encoding.GetEncoding(437);

		public enum Color
		{
			Black = 0,
			Blue,
			Green,
			Cyan,
			Red,
			Magenta,
			Brown,
			LightGray,
			DarkGray,
			LightBlue,
			LightGreen,
			LightCyan,
			LightRed,
			LightMagenta,
			Yellow,
			White
		}

		public enum WriteMode
		{
			Copy = 0,
			Xor,
			Or,
			And,
			Not
		}

		public enum LineStyle
		{
			Solid = 0,
			Dotted,
			Center,
			Dashed,
			User
		}

		public enum FillStyle
		{
			Empty = 0,
			Solid,
			Line,
			LtSlash,
			Slash,
			BkSlash,
			LtBkSlash,
			Hatch,
			XHatch,
			Interleave,
			WideDot,
			CloseDot,
			User
		}

		public enum Direction
		{
			Horizontal = 0,
			Vertical
		}

		public enum FontType
		{
			Default = 0,
			Triplex,
			Small,
			SansSerif,
			Gothic,
			Script,
			Simplex,
			TriplexScript,
			Complex,
			European,
			BoldOutline,
			User
		}

		public const double ASPECT = 350.0 / 480.0 * 1.06; //0.772; //7.0/9.0; //350.0 / 480.0 * 1.066666;
		const int BITS_PER_PIXEL = 8;
		IndexedBitmap bmp;
		BitmapData bd;
		byte* bits;
		readonly Palette palEga = Palette.GetEgaPalette();
		readonly Palette pal = Palette.GetDosPalette();
		byte color = 7;
		byte bkcolor;
		byte fillcolor;
		readonly int[] colors = new int[16];
		Graphics graphics;
		int totalrectcomplexity;
		FillStyle fillStyle;
		LineStyle lineStyle = LineStyle.Solid;
		Direction textDirection = Direction.Horizontal;
		int lineThickness = 1;
		FontType font = FontType.Small;
		int characterSize = 4;
		WriteMode writeMode = WriteMode.Copy;
		Point currentPos;
		Rectangle viewPort = new Rectangle(0, 0, 640, 350);
		readonly Size windowSize = new Size(640, 350);
		int scanWidth = 640;
		readonly string[] fontTypes = {
			string.Empty,
			"TRIP.CHR",
			"LITT.CHR",
			"SANS.CHR",
			"GOTH.CHR",
			"SCRI.CHR",
			"SIMP.CHR",
			"TSCR.CHR",
			"LCOM.CHR",
			"EURO.CHR",
			"BOLD.CHR"
		};
		readonly Dictionary<FontType, IBGIFont> fontCache = new Dictionary<FontType, IBGIFont>();
		Drawable control;
		readonly byte[] DefaultUserPattern = { 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55 };
		readonly byte[][] fillStyles = {
			new byte[] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00},
			new byte[] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF},
			new byte[] { 0xFF,  0xFF,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00},
			new byte[] { 0x01,  0x02,  0x04,  0x08,  0x10,  0x20,  0x40,  0x80},
			new byte[] { 0xE0,  0xC1,  0x83,  0x07,  0x0E,  0x1C,  0x38,  0x70},
			new byte[] { 0xF0,  0x78,  0x3C,  0x1E,  0x0F,  0x87,  0xC3,  0xE1},
			new byte[] { 0xA5,  0xD2,  0x69,  0xB4,  0x5A,  0x2D,  0x96,  0x4B},
			new byte[] { 0xFF,  0x88,  0x88,  0x88,  0xFF,  0x88,  0x88,  0x88},
			new byte[] { 0x81,  0x42,  0x24,  0x18,  0x18,  0x24,  0x42,  0x81},
			new byte[] { 0xCC,  0x33,  0xCC,  0x33,  0xCC,  0x33,  0xCC,  0x33},
			new byte[] { 0x80,  0x00,  0x08,  0x00,  0x80,  0x00,  0x08,  0x00},
			new byte[] { 0x88,  0x00,  0x22,  0x00,  0x88,  0x00,  0x22,  0x00},
			new byte[] { 0xAA,  0x55,  0xAA,  0x55,  0xAA,  0x55,  0xAA,  0x55}
		};
		readonly uint[] line_style_bits = {
				(uint)0xFFFF,
				(uint)0xCCCC,
				(uint)0xF878,
				(uint)0xF8F8,
				(uint)0x0000
			};
		BitArray lineStyleBits;

		public bool DelayDraw
		{
			get;
			set;
		}

		public Palette Palette
		{
			get { return pal; }
		}

		public Drawable Control
		{
			get { return control; }
			set
			{
				control = value;
				graphics = null;
			}
		}

		public Size WindowSize
		{
			get { return windowSize; }
		}

		public Palette EGAPalette
		{
			get { return palEga; }
		}

		SizeF scale = new SizeF(1, 1);
		public SizeF Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				IsDefaultScale = scale == new SizeF(1, 1);
			}
		}

		bool IsDefaultScale
		{
			get;
			set;
		}


		/*
		 */
		public IndexedBitmap GetBitmap()
		{
			lock (bitmapLock)
			{

				var newbmp = new IndexedBitmap(bmp.Size.Width, bmp.Size.Height, 8);
				newbmp.Palette = bmp.Palette.Clone();
				using (var newbd = newbmp.Lock())
				{
					var psrc = (byte*)bd.Data;
					var pdest = (byte*)newbd.Data;
					for (int i = 0; i < bmp.Size.Width * bmp.Size.Height; i++)
					{
						*
							pdest = *psrc;
						pdest++;
						psrc++;
					}
				}

				return newbmp;


				/*
				bmp.Unlock (bd); 
				Bitmap bb = new Bitmap (generator, bmp.Size.Width, bmp.Size.Height, PixelFormat.Format32bppRgb);
				using (var g = new Graphics(generator, bb)) {
					g.DrawImage (bmp, 0, 0);
					g.Flush ();
				}
				bd = bmp.Lock ();
				bits = (byte*)bd.Data;
				return bb;
				*/
			}
		}
		/*
		 */

		public void GraphDefaults(IList<Rectangle> updates = null)
		{
			viewPort = new Rectangle(windowSize);
			Palette.EGAColors.CopyTo(colors, 0);
			SetColor(7);
			SetBkColor(0);
			SetFillPattern(DefaultUserPattern);
			SetLineStyle(LineStyle.Solid);
			SetFillStyle(FillStyle.Solid, 0);
			ClearDevice(updates);
		}

		public void DrawRegion(Graphics g, Rectangle rect)
		{
			DrawRegion(g, rect, rect);
		}

		Bitmap wpfbm;

		public void DrawRegion(Graphics g, Rectangle rectSource, Rectangle rectDest)
		{
			lock (bitmapLock)
			{

				bd.Dispose();
				Image b = bmp;
				if (g.Platform.IsWpf)
				{
					if (wpfbm == null)
						wpfbm = new Bitmap(b, interpolation: ImageInterpolation.None);
					else
					{
						using (var g2 = new Graphics(wpfbm))
						{
							g2.ImageInterpolation = ImageInterpolation.None;
							g2.DrawImage(bmp, rectSource, rectSource);
						}
					}
					b = wpfbm;
				}

				g.DrawImage(b, rectSource, rectDest);

				bd = bmp.Lock();
				bits = (byte*)bd.Data;
			}
		}

		public BGICanvas(Control control, Size? size = null)
		{
			this.control = control as Drawable;

			if (size != null)
				this.windowSize = size.Value;
			// generator.ThreadInit();
			bmp = new IndexedBitmap(windowSize.Width, windowSize.Height, 8);
			//generator.ThreadDone();
			Palette currentPal = bmp.Palette;
			for (int i = 0; i < pal.Count; i++)
			{
				currentPal[i] = pal[i];
			}
			bmp.Palette = currentPal;
			bd = bmp.Lock();
			this.scanWidth = bd.ScanWidth;
			bits = (byte*)bd.Data;
			Palette.EGAColors.CopyTo(colors, 0);


			GraphDefaults();
		}

		public byte SetColor(byte c)
		{
			var old = color;
			color = (byte)(c % 16);
			return old;
		}

		public void SetBkColor(byte c)
		{
			bkcolor = (byte)(c % 16);
		}

		public byte GetBkColor()
		{
			return bkcolor;
		}

		public byte GetColor()
		{
			return color;
		}

		static class BezierHandler
		{
			static readonly double[] StArr = { 1, 3, 3, 1 };

			static double First(int n, double v)
			{
				switch (n)
				{
					default:
						return 1;
					case 1:
						return v;
					case 2:
						return v * v;
					case 3:
						return v * v * v;
				}
			}

			static double Second(int n, double v)
			{
				switch (n)
				{
					default:
						return 1;
					case 2:
						return Math.Exp(Math.Log(1.0 - v));
					case 1:
						return Math.Exp((double)2 * Math.Log(1.0 - v));
					case 0:
						return Math.Exp((double)3 * Math.Log(1.0 - v));
				}
			}

			public static double Bezier(double v, int n)
			{
				return StArr[n] * First(n, v) * Second(n, v);
			}
		}

		public void DrawBezier(int count, Point[] points, int segments, IList<Rectangle> updates = null)
		{
			int x1 = points[0].X;
			int y1 = points[0].Y;
			int v = 1;
			var drawUpdates = updates ?? new List<Rectangle>();
			do
			{
				double x3 = 0;
				double y3 = 0;
				double br = (double)v / (double)segments;
				for (int i = 0; i < 4; i++)
				{
					double ar = BezierHandler.Bezier(br, i);
					x3 += (double)points[i].X * ar;
					y3 += (double)points[i].Y * ar;
				}
				int x2 = Round(x3);
				int y2 = Round(y3);
				Line(x1, y1, x2, y2, drawUpdates);
				x1 = x2;
				y1 = y2;
				v++;
			} while (v < segments);
			Line(x1, y1, points[count - 1].X, points[count - 1].Y, drawUpdates);
			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		/*
		public void DrawBezier2(int count, Point[] points, int segments)
		{
			int i,i2,i3,xx,yy;
			double t,tm3,t2,t2m3,t3,t3m3,nc1,nc2,nc3,nc4,step;
			//d:curveDataRec absolute d0;
			MoveTo(points[0].X, points[0].Y);
			step=1/segments;
			for (i2=0; i2<=(count-1)/4; i2++)
			{
				i=i2*4;
				t=0.0;
				for (i3=(segments-1); i3>=0; i3--)
				{
					t=t+step;
					tm3=t*3.0; t2=t*t; t2m3=t2*3.0; t3=t2*t; t3m3=t3*3.0;
					nc1=1-tm3+t2m3-t3;
					nc2=t3m3-2.0*t2m3+tm3;
					nc3=t2m3-t3m3;
					nc4=t3;

					xx=(int)(nc1*points[i].X+nc2*points[i+1].X+nc3*points[i+2].X+nc4*points[i+3].X);
					yy=(int)(nc1*points[i].Y+nc2*points[i+1].Y+nc3*points[i+2].Y+nc4*points[i+3].Y);
					LineTo(xx, yy);
				}
			}
			LineTo(points[count-1].X, points[count-1].Y);
		}


		public void DrawBezier3(int count, Point[] points, int segments)
		{
			const double nsa=1/6; const double nsb=2/3;
			int i,i2,xx,yy;
			double t,ta,t2,t2a,t3,t3a,nc1,nc2,nc3,nc4,step;
			step=(double)1/segments;
			int lastx = points[0].X;
			int lasty = points[0].Y;
			for (i=0; i<=count-4; i++)
			{
				t=0.0;
				for (i2=segments-1; i2>=0; i2--)
				{
					t=t+step;
					ta=t*0.5; t2=t*t; t2a=t2*0.5; t3=t2*t; t3a=t3*0.5;
					nc1=-nsa*t3+t2a-ta+nsa;
					nc2=t3a-t2+nsb;
					nc3=-t3a+t2a+ta+nsa;
					nc4=nsa*t3;
					xx=Round(nc1*points[i].X+nc2*points[i+1].X+nc3*points[i+2].X+nc4*points[i+3].X);
					yy=Round(nc1*points[i].Y+nc2*points[i+1].Y+nc3*points[i+2].Y+nc4*points[i+3].Y);
					Line(lastx, lasty, xx, yy);
					lastx = xx;
					lasty = yy;
				}
			}
			Line(lastx, lasty, points[count-1].X, points[count-1].Y);
		}

		public void DrawBezier4(int count, Point[] points, int segments)
		{
			double resolution = (double)1/(double)segments;

			int lastx = points[0].X;
			int lasty = points[0].Y;
			
			double t = 0;
			while (t < 1)
			{
				int curx = (int)(Math.Pow(1 - t, 3) * points[0].X + 3 * t * Math.Pow(1 - t, 2) * points[1].X +
					3 * t * t * (1 - t) * points[2].X + Math.Pow(t, 3) * points[3].X);
				int cury = (int)(Math.Pow(1 - t, 3) * points[0].Y + 3 * t * Math.Pow(1 - t, 2) * points[1].Y +
					3 * t * t * (1 - t) * points[2].Y + Math.Pow(t, 3) * points[3].Y);

				Line(lastx, lasty, curx, cury);
				lastx = curx;
				lasty = cury;
				t += resolution;
			}
			Line(lastx, lasty, points[3].X, points[3].Y);
		}
		 */

		public static int Trunc(double val)
		{
			return (int)val;
		}

		public static int Trunc(float val)
		{
			return (int)val;
		}

		public static int Round(float val)
		{
			if (val > 0)
			{
				if ((val % 1) >= 0.5)
				{
					return (int)val + 1;
				}
			}
			else
			{
				if ((val % 1) <= -0.5)
				{
					return (int)val - 1;
				}
			}
			return (int)val;
		}

		public static int Round(double val)
		{
			if (val > 0)
			{
				if ((val % 1) >= 0.5)
				{
					return (int)val + 1;
				}
			}
			else
			{
				if ((val % 1) <= -0.5)
				{
					return (int)val - 1;
				}
			}
			return (int)val;
		}

		public void DrawPoly(Point[] points, IList<Rectangle> updates = null)
		{
			var drawUpdates = updates ?? new List<Rectangle>();
			Point lastPoint = points[0];
			for (int i = 1; i < points.Length; i++)
			{
				Point point = points[i];
				Line(lastPoint.X, lastPoint.Y, point.X, point.Y, drawUpdates);
				lastPoint = point;
			}
			Line(lastPoint.X, lastPoint.Y, points[0].X, points[0].Y, drawUpdates);
			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		void ScanLine(Point start, Point end, List<int>[] rows, bool full)
		{
			int ydelta = Math.Abs(end.Y - start.Y);


			if (full || start.Y < end.Y)
			{
				AddScanRow(rows, start);
			}
			if (ydelta > 0)
			{
				int xdelta = (start.Y > end.Y) ? (start.X - end.X) : (end.X - start.X);
				int minX = (start.Y > end.Y) ? end.X : start.X;
				int posY = Math.Min(start.Y, end.Y);
				int posX;

				posY++;
				for (int count = 1; count < ydelta; count++)
				{
					posX = (xdelta * count / ydelta) + minX;

					if (posY >= 0 && posY < rows.Length)
						AddScanRow(rows, posX, posY);
					posY++;
				}
			}
			if (full || end.Y < start.Y)
			{
				AddScanRow(rows, end);
			}
		}

		void ScanLines(int startIndex, int endIndex, List<int>[] rows, IList<Point> points, bool full)
		{
			ScanLine(points[startIndex], points[endIndex], rows, full);
		}

		public void FillPoly(Point[] points, IList<Rectangle> updates = null)
		{
			if (points.Length <= 1)
				return;

			var rows = CreateScanRows();

			if (!viewPort.Contains(points[0]))
				return;
			for (int i = 1; i < points.Length; i++)
			{
				/*if (!viewPort.Contains (points [i]))
					return;*/
				ScanLines(i - 1, i, rows, points, false);
			}
			ScanLines(points.Length - 1, 0, rows, points, false);
			var drawUpdates = updates ?? new List<Rectangle>();

			if (fillStyle != FillStyle.Empty)
			{
				for (int i = 0; i < rows.Length; i++)
				{
					var row = rows[i];
					var y = i - 1;
					if (row != null)
					{
						row.Sort();
						bool on = false;
						int lastx = -1;
						for (int x = 0; x < row.Count; x++)
						{
							int curx = row[x];
							if (on)
							{
								Bar(lastx, y, curx, y, drawUpdates);
							}

							on = !on;
							lastx = curx;
						}
					}
				}
			}
			if (color != 0)
				DrawPoly(points, drawUpdates);

			if (updates == null)
				UpdateRegion(drawUpdates);

		}

		public void MoveTo(Point point)
		{
			currentPos = point;
		}

		public void MoveTo(int x, int y)
		{
			MoveTo(new Point(x, y));
		}

		public void LineTo(Point point, IList<Rectangle> updates = null)
		{
			LineTo(point.X, point.Y, updates);
		}

		public void LineTo(int x, int y, IList<Rectangle> updates = null)
		{
			Line(currentPos.X, currentPos.Y, x, y, updates);
			currentPos = new Point(x, y);
		}

		public void LineRel(int dx, int dy)
		{
			Line(currentPos.X, currentPos.Y, currentPos.X + dx, currentPos.Y + dy);
		}

		void FillX(int y, int startx, int count, ref int offset, ICollection<Rectangle> updates)
		{
			int starty = y - lineThickness / 2;
			int endy = starty + lineThickness - 1;
			int endx = startx + count;
			if (count > 0)
				endx--;
			else
			{
				endx++;
				offset -= count;
			}

			if (starty < 0)
				starty = 0;
			if (endy > viewPort.InnerBottom)
				endy = viewPort.InnerBottom;

			var inc = count >= 0 ? 1 : -1;
			if (startx > endx)
				Swap(ref startx, ref endx);

			if (startx > viewPort.InnerRight)
				return;
			if (startx < 0)
				startx = 0;
			if (endx > viewPort.InnerRight)
				endx = viewPort.InnerRight;

			for (int x = startx; x <= endx; x++)
			{
				if (lineStyleBits[Math.Abs(offset % lineStyleBits.Length)])
				{
					for (int cy = starty; cy <= endy; cy++)
					{
						PutPixelInternal(x, cy, color);
					}
				}
				offset += inc;
			}
			if (count < 0)
				offset -= count;

			var updateRect = new Rectangle(startx, starty, endx - startx + 1, endy - starty + 1);
			if (updates != null)
				updates.Add(updateRect);
			else
				UpdateRegion(updateRect);
		}

		void FillY(int x, int starty, int count, ref int offset, ICollection<Rectangle> updates)
		{
			int startx = x - lineThickness / 2;
			int endx = startx + lineThickness - 1;
			int endy = starty + count;
			if (count > 0)
				endy--;
			else
			{
				endy++;
				offset += count;
			}

			if (startx < 0)
				startx = 0;
			if (endx > viewPort.InnerRight)
				endx = viewPort.InnerRight;

			if (starty > endy)
				Swap(ref starty, ref endy);

			if (starty > viewPort.InnerBottom)
				return;
			if (starty < 0)
				starty = 0;
			if (endy > viewPort.InnerBottom)
				endy = viewPort.InnerBottom;
			for (int y = starty; y <= endy; y++)
			{
				if (lineStyleBits[Math.Abs(offset++ % lineStyleBits.Length)])
				{
					for (int cx = startx; cx <= endx; cx++)
					{
						PutPixelInternal(cx, y, color);
					}
				}
			}
			if (count < 0)
				offset += count;
			if (updates != null)
				updates.Add(new Rectangle(startx, starty, endx - startx + 1, endy - starty + 1));
			else
				UpdateRegion(startx, starty, endx, endy);
		}

		internal Graphics CreateGraphics()
		{
			if (graphics == null)
			{
				if (control != null && control.Visible)
				{
					graphics = control.CreateGraphics();
					graphics.AntiAlias = false;
					graphics.ImageInterpolation = ImageInterpolation.None;
				}
			}
			return graphics;
		}

		public void ResetGraphics()
		{
			if (graphics != null)
			{
				graphics.Flush();
				graphics.Dispose();
				graphics = null;
			}
		}

		public void Line(Point start, Point end, IList<Rectangle> updates = null)
		{
			Line(start.X, start.Y, end.X, end.Y, updates);
		}

		public void Line(int x1, int y1, int x2, int y2, IList<Rectangle> updates = null)
		{
			int lAdjUp, lAdjDown, lError, lAdvance;
			int lXDelta, lYDelta;
			int lWholeStep, lStartLength, lEndLength, lCount;
			int lRunLength;
			int lStep;
			int offset = 0;
			Point pos;
			lYDelta = Math.Abs(y2 - y1);
			lXDelta = Math.Abs(x2 - x1);
			var drawUpdates = updates ?? new List<Rectangle>();

			if (lXDelta == 0)
			{
				FillY(x1, Math.Min(y1, y2), lYDelta + 1, ref offset, drawUpdates);
			}
			else if (lYDelta == 0)
			{
				FillX(y1, Math.Min(x1, x2), lXDelta + 1, ref offset, drawUpdates);
			}
			else if (lXDelta >= lYDelta)
			{
				lAdvance = 1;
				if (y1 < y2)
				{
					pos = new Point(x1, y1);
					lStep = (x1 > x2) ? -1 : 1;
				}
				else
				{
					pos = new Point(x2, y2);
					lStep = (x2 > x1) ? -1 : 1;
				}

				lWholeStep = (lXDelta / lYDelta) * lStep;
				lAdjUp = (lXDelta % lYDelta);
				lAdjDown = lYDelta * 2;
				lError = lAdjUp - lAdjDown;
				lAdjUp *= 2;

				lStartLength = (lWholeStep / 2) + lStep;
				lEndLength = lStartLength;
				if ((lAdjUp == 0) && ((lWholeStep & 0x01) == 0))
				{
					lStartLength -= lStep;
				}

				if ((lWholeStep & 0x01) != 0)
					lError += lYDelta;

				FillX(pos.Y, pos.X, lStartLength, ref offset, drawUpdates);
				pos.X += lStartLength;
				pos.Y += lAdvance;

				for (lCount = 0; lCount < (lYDelta - 1); lCount++)
				{
					lRunLength = lWholeStep;
					if ((lError += lAdjUp) > 0)
					{
						lRunLength += lStep;
						lError -= lAdjDown;
					}
					FillX(pos.Y, pos.X, lRunLength, ref offset, drawUpdates);
					pos.X += lRunLength;
					pos.Y += lAdvance;
				}
				FillX(pos.Y, pos.X, lEndLength, ref offset, drawUpdates);
			}
			else if (lXDelta < lYDelta)
			{
				if (y1 < y2)
				{
					pos = new Point(x1, y1);
					lAdvance = (x1 > x2) ? -1 : 1;
				}
				else
				{
					pos = new Point(x2, y2);
					lAdvance = (x2 > x1) ? -1 : 1;
				}

				lWholeStep = lYDelta / lXDelta;
				lAdjUp = (lYDelta % lXDelta);
				lAdjDown = lXDelta * 2;
				lError = lAdjUp - lAdjDown;
				lAdjUp *= 2;
				lStartLength = (lWholeStep / 2) + 1;
				lEndLength = lStartLength;
				if ((lAdjUp == 0) && ((lWholeStep & 0x01) == 0))
				{
					lStartLength--;
				}
				if ((lWholeStep & 0x01) != 0)
				{
					lError += lXDelta;
				}

				FillY(pos.X, pos.Y, lStartLength, ref offset, drawUpdates);
				pos.Y += lStartLength;
				pos.X += lAdvance;

				for (lCount = 0; lCount < (lXDelta - 1); lCount++)
				{
					lRunLength = lWholeStep;
					if ((lError += lAdjUp) > 0)
					{
						lRunLength++;
						lError -= lAdjDown;
					}

					FillY(pos.X, pos.Y, lRunLength, ref offset, drawUpdates);
					pos.Y += lRunLength;
					pos.X += lAdvance;
				}
				FillY(pos.X, pos.Y, lEndLength, ref offset, drawUpdates);
			}
			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		public FillStyle GetFillStyle()
		{
			return fillStyle;
		}

		public byte GetFillColor()
		{
			return fillcolor;
		}

		public void SetFillStyle(FillStyle fillStyle, byte color)
		{
			fillcolor = (byte)(color % 16);
			this.fillStyle = fillStyle;
		}

		public void SetLineStyle(LineStyle lineStyle, uint pattern, int thickness)
		{
			lineThickness = thickness;
			if (lineStyle == LineStyle.User)
				line_style_bits[(int)LineStyle.User] = pattern;

			SetLineStyle(lineStyle);
		}

		public int GetLineThickness()
		{
			return lineThickness;
		}

		public void SetLineThickness(int thickness)
		{
			lineThickness = thickness;
		}

		public uint GetLinePattern(LineStyle lineStyle)
		{
			return line_style_bits[(int)lineStyle];
		}

		public LineStyle GetLineStyle()
		{
			return lineStyle;
		}

		public BitArray GetLineStyleBits(LineStyle lineStyle)
		{
			uint pattern = line_style_bits[(int)lineStyle];
			return new BitArray(new [] { (byte)(pattern >> 8), (byte)(pattern & 0xFF) });
		}

		public LineStyle SetLineStyle(LineStyle lineStyle)
		{
			var old = this.lineStyle;
			this.lineStyle = lineStyle;
			lineStyleBits = GetLineStyleBits(this.lineStyle);
			return old;
		}

		public byte GetPixel(Point point)
		{
			return GetPixel(point.X, point.Y);
		}

		public byte GetPixel(int x, int y)
		{
			lock (bitmapLock)
			{
				return bits[y * scanWidth + x];
			}
		}

		public bool PutPixelInternal(int x, int y, byte color)
		{
			if (!viewPort.Contains(x, y))
				return false;
			lock (bitmapLock)
			{
				var pos = y * scanWidth + x;
				switch (writeMode)
				{
					case WriteMode.Copy:
						bits[pos] = color;
						break;
					case WriteMode.Xor:
						bits[pos] ^= color;
						break;
					case WriteMode.Or:
						bits[pos] |= color;
						break;
					case WriteMode.And:
						bits[pos] &= color;
						break;
					case WriteMode.Not:
						bits[pos] = (byte)(bits[pos] & (byte)~color);
						break;
				}
			}
			return true;
		}

		public void PutPixel(int x, int y, byte c, IList<Rectangle> updates = null)
		{
			if (PutPixelInternal(x, y, c))
			{

				if (updates != null)
					updates.Add(new Rectangle(x, y, 1, 1));
				else
					UpdateRegion(x, y, x, y);
			}

		}

		static bool AlreadyDrawn(IList<List<LineInfo>> fillLines, int x, int y)
		{
			foreach (var li in fillLines[y])
			{
				if (y == li.y && x >= li.x1 && x <= li.x2)
					return true;
			}
			return false;
		}

		public void FloodFill(int x, int y, int border, IList<Rectangle> updates = null)
		{
			var fillLines = new List<LineInfo>[viewPort.InnerBottom + 1];
			for (int i = 0; i < fillLines.Length; i++)
			{
				fillLines[i] = new List<LineInfo>();
			}
			var pointStack = new Stack<FillLineInfo>();

			if (!viewPort.Contains(x, y))
				return;

			lock (bitmapLock)
			{
				if (bits[y * scanWidth + x] != border)
				{
					LineInfo li = FindLine(bits, x, y, border);
					if (li != null)
					{
						fillLines[li.y].Add(li);
						//this.Bar(li.x1, li.y, li.x2, li.y);
						pointStack.Push(new FillLineInfo(li, 1));
						pointStack.Push(new FillLineInfo(li, -1));

						while (pointStack.Count > 0)
						{
							FillLineInfo fli = pointStack.Pop();

							int cury = fli.y + fli.Dir;
							if (cury <= viewPort.InnerBottom && cury >= viewPort.Top)
							{
								int ypos = cury * scanWidth;
								for (int cx = fli.x1; cx <= fli.x2; cx++)
								{
									if (bits[ypos + cx] == border)
										continue; // it's a border color, so don't scan any more this direction
									if (AlreadyDrawn(fillLines, cx, cury))
										continue; // already been here

									li = FindLine(bits, cx, cury, border); // find the borders on this line
									if (li != null)
									{
										fillLines[li.y].Add(li);
										cx = li.x2;
										pointStack.Push(new FillLineInfo(li, fli.Dir));
										if (fillcolor != 0)
										{ // bgi doesn't go backwards when filling black!  why?  dunno.  it just does.
											// if we go out of current line's bounds, check the opposite dir for those
											if (li.x2 > fli.x2)
												pointStack.Push(new FillLineInfo(li.y, fli.x2 + 1, li.x2, -fli.Dir));
											if (li.x1 < fli.x1)
												pointStack.Push(new FillLineInfo(li.y, li.x1, fli.x1 - 1, -fli.Dir));
										}
									}
								}
							}
						}
					}
				}
			}
			var drawUpdates = updates ?? new List<Rectangle>();
			for (int i = 0; i < fillLines.Length; i++)
			{
				foreach (LineInfo cli in fillLines[i])
				{
					Bar(cli.x1, cli.y, cli.x2, cli.y, drawUpdates);
				}
			}
			if (updates == null)
				UpdateRegion(drawUpdates);

		}

		class FillLineInfo : LineInfo
		{
			public int Dir { get; set; }

			public FillLineInfo(LineInfo li, int dir)
				: base(li)
			{
				this.Dir = dir;
			}

			public FillLineInfo(int y, int x1, int x2, int dir)
				: base(y, x1, x2)
			{
				this.Dir = dir;
			}
		}

		class LineInfo
		{
			public int x1;
			public int x2;
			public int y;

			public LineInfo(LineInfo li)
			{
				this.x1 = li.x1;
				this.x2 = li.x2;
				this.y = li.y;
			}

			public LineInfo(int y, int x1, int x2)
			{
				this.x1 = x1;
				this.x2 = x2;
				this.y = y;
			}
		}

		LineInfo FindLine(byte* bits, int x, int y, int border)
		{
			// find end pixel
			int endx, startx;
			int pos = y * bd.ScanWidth + x;
			for (endx = x; endx <= viewPort.InnerRight; endx++)
			{
				byte col = bits[pos++];
				if (col == border)
					break;
			}
			endx--;

			// find beginning pixel
			pos = y * bd.ScanWidth + x - 1;
			for (startx = x - 1; startx >= viewPort.Left; startx--)
			{
				byte col = bits[pos--];
				if (col == border)
					break;
			}
			startx++;
			// a weird condition for solid fills and the sides of the screen
			if ((startx == 0 || endx == viewPort.InnerRight) && (endx == startx))
				return null;

			return new LineInfo(y, startx, endx);
		}

		static void arc_coords(double angle, double rx, double ry, out int x, out int y)
		{
			if (rx == 0 || ry == 0)
			{
				x = y = 0;
				return;
			}
			double s = Math.Sin(angle * deg2rad);
			double c = Math.Cos(angle * deg2rad);
			if (Math.Abs(s) < Math.Abs(c))
			{
				double tg = s / c;
				double xr = Math.Sqrt(rx * rx * ry * ry / (ry * ry + rx * rx * tg * tg));
				x = (int)((c >= 0) ? xr : -xr);
				y = (int)((s >= 0) ? -xr * tg : xr * tg);
			}
			else
			{
				double ctg = c / s;
				double yr = Math.Sqrt(rx * rx * ry * ry / (rx * rx + ry * ry * ctg * ctg));
				x = (int)((c >= 0) ? yr * ctg : -yr * ctg);
				y = (int)((s >= 0) ? -yr : yr);
			}
		}

		public void Rectangle(Rectangle rect, IList<Rectangle> updates = null)
		{
			Rectangle(rect.Left, rect.Top, rect.InnerRight, rect.InnerBottom, updates);
		}

		static void Swap<T>(ref T l, ref T r)
		{
			var temp = l;
			l = r;
			r = temp;
		}

		public void Rectangle(int left, int top, int right, int bottom, IList<Rectangle> updates = null)
		{
			var drawUpdates = updates ?? new List<Rectangle>();
			Line(left, top, right, top, drawUpdates);
			Line(left, bottom, right, bottom, drawUpdates);
			Line(right, top, right, bottom, drawUpdates);
			Line(left, top, left, bottom, drawUpdates);
			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		static List<int>[] CreateScanRows()
		{
			return new List<int>[352];
		}

		void AddScanVertical(IList<int>[] rows, int x, int y, int count)
		{
			for (int i = 0; i < count; i++)
			{
				AddScanRow(rows, x, y + i);
			}
		}

		void AddScanHorizontal(IList<int>[] rows, int x, int y, int count)
		{
			for (int i = 0; i < count; i++)
			{
				AddScanRow(rows, x + i, y);
			}
		}

		void AddScanRow(IList<int>[] rows, Point point)
		{
			AddScanRow(rows, point.X, point.Y);
		}

		static void AddScanRow(IList<IList<int>> rows, int x, int y)
		{
			if (y < -1 || y > 350)
				return;
			y++;
			var rowPoints = rows[y];
			if (rowPoints == null)
			{
				rows[y] = rowPoints = new List<int>();
			}
			rowPoints.Add(x);
		}

		void ScanEllipse(int x, int y, int startAngle, int endAngle, int radiusx, int radiusy, IList<int>[] rows)
		{
			// check if valid angles
			if (startAngle > endAngle)
			{
				var tt = startAngle;
				startAngle = endAngle;
				endAngle = tt;
			}

			radiusx = Math.Max(1, radiusx);
			radiusy = Math.Max(1, radiusy);

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

			do
			{
				long e2 = 2 * err;
				var angle = Math.Atan((double)yoffset * aspect / (double)xoffset) * rad2deg;

				symmetryScan(x, y, startAngle, endAngle, xoffset, yoffset, Round(angle), angle <= horizontal_angle, rows);
				if (Math.Abs(angle - horizontal_angle) < 1)
				{
					symmetryScan(x, y, startAngle, endAngle, xoffset, yoffset, Round(angle), !(angle <= horizontal_angle), rows);
				}

				if (e2 <= stopy)
				{
					yoffset++;
					err += stopy += incx;
				}
				if (e2 >= stopx)
				{
					xoffset--;
					err += stopx += incy;
				}
			} while (xoffset >= 0);

			xoffset++;
			while (yoffset < radiusy)
			{
				var angle = Math.Atan((double)yoffset * aspect / (double)xoffset) * rad2deg;
				symmetryScan(x, y, startAngle, endAngle, xoffset, yoffset, Round(angle), angle <= horizontal_angle, rows);
				if (angle == horizontal_angle)
				{
					symmetryScan(x, y, startAngle, endAngle, xoffset, yoffset, Round(angle), !(angle <= horizontal_angle), rows);
				}
				yoffset++;
			}
			/**
			int radiusx2 = radiusx * radiusx;
			int radiusy2 = radiusy * radiusy;
			int xoffset = 0;
			int yoffset = radiusy;
			int s = radiusx2 * (1 - 2 * radiusy) + 2 * radiusy2;
			int t = radiusy2 - 2 * radiusx2 * (2 * radiusy - 1);
			
			bool fill = true;
			symmetryScan (x, y, start_angle, end_angle, xoffset, yoffset, minrows, maxrows);
			do {
				if (s < 0) {
					s += 2 * radiusy2 * (2 * xoffset + 3);
					t += 4 * radiusy2 * (xoffset + 1);
					xoffset++;
				} else if (t < 0) {
					s += 2 * radiusy2 * (2 * xoffset + 3) - 4 * radiusx2 * (yoffset - 1);
					t += 4 * radiusy2 * (xoffset + 1) - 2 * radiusx2 * (2 * yoffset - 3);
					xoffset++;
					yoffset--;
					fill = true;
				} else {
					s -= 4 * radiusx2 * (yoffset - 1);
					t -= 2 * radiusx2 * (2 * yoffset - 3);
					yoffset--;
					fill = true;
				}
				if (fill) {
					symmetryScan (x, y, start_angle, end_angle, xoffset, yoffset, minrows, maxrows);
					fill = false;
				}
			} while (yoffset>0);
			/**/
		}

		void FillScan(List<int>[] rows, IList<Rectangle> updates = null)
		{
			var drawUpdates = updates ?? new List<Rectangle>();

			for (int y = 0; y < rows.Length - 2; y++)
			{
				var row = rows[y + 1];
				if (row != null && row.Count > 0)
				{
					row.Sort();
					Bar(row[0], y, row[row.Count - 1], y, drawUpdates);
				}
			}

			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		void DrawScan(List<int>[] rows, IList<Rectangle> updates = null)
		{
			var drawUpdates = updates ?? new List<Rectangle>();

			for (int i = 0; i < rows.Length; i++)
			{
				List<int> row = rows[i];
				if (row != null)
				{
					int y = i - 1;
					foreach (int x in row.Distinct())
					{
						PutPixel(x, y, color, drawUpdates);
					}
				}
			}


			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		void OutlineScan(List<int>[] rows, IList<Rectangle> updates = null)
		{
			var oldLineStyle = GetLineStyle();
			if (oldLineStyle != LineStyle.Solid)
				SetLineStyle(LineStyle.Solid);

			var drawUpdates = updates ?? new List<Rectangle>();
			int lastminx = 0;
			int lastmaxx = 0;
			bool first = true;
			foreach (List<int> row in rows.Where(r => r != null))
			{
				row.Sort();
				if (row.Count > 2)
					row.RemoveRange(1, row.Count - 2);
			}


			for (int i = 0; i < rows.Length; i++)
			{
				List<int> row = rows[i];
				int y = i - 1;
				if (row != null && row.Count > 0)
				{
					int minx = row[0];
					int maxx = row[row.Count - 1];
					bool hasnext = i < rows.Length - 1;
					bool last = false;
					int nextminx = 0;
					int nextmaxx = 0;
					var nextrow = (hasnext) ? rows[i + 1] : null;
					if (nextrow != null && nextrow.Count > 0)
					{
						nextminx = nextrow[0];
						nextmaxx = nextrow[nextrow.Count - 1];
					}
					else
					{
						last = true;
						hasnext = false;
						nextrow = null;
					}

					if (first)
					{
						if (hasnext)
						{
							if (nextmaxx > nextminx)
								Line(nextminx + 1, y, nextmaxx - 1, y, drawUpdates);
							else
								Line(nextminx, y, nextmaxx, y, drawUpdates);
						}
						first = false;
					}
					else if (last)
					{
						if (lastmaxx > lastminx)
							Line(lastminx + 1, y, lastmaxx - 1, y, drawUpdates);
						else
							Line(lastminx, y, lastmaxx, y, drawUpdates);
					}
					else
					{
						if (minx >= lastminx)
						{
							var mnx = (minx > lastminx) ? lastminx + 1 : lastminx;
							Line(mnx, y, minx, y, drawUpdates);
						}

						if (row.Count > 1 && maxx <= lastmaxx)
						{
							var mxx = (maxx < lastmaxx) ? lastmaxx - 1 : lastmaxx;
							Line(mxx, y, maxx, y, drawUpdates);
						}
					}
					if (hasnext)
					{
						if (minx < lastminx && minx >= nextminx)
						{
							var mnx = (minx > nextminx) ? nextminx + 1 : nextminx;
							Line(mnx, y, minx, y, drawUpdates);
						}

						if (row.Count > 1 && nextrow != null && nextrow.Count > 1 && maxx > lastmaxx && maxx <= nextmaxx)
						{
							var mxx = (maxx < nextmaxx) ? nextmaxx - 1 : nextmaxx;
							Line(mxx, y, maxx, y, drawUpdates);
						}
					}
					lastminx = minx;
					lastmaxx = maxx;
				}
			}
			/*first = true;
			for (int i = rows.Length - 1; i >= 0; i--) {
				List<int> row = rows [i];
				int y = i - 1;
				if (row != null && row.Count > 0) {
					int minx = row [0];
					int maxx = row [row.Count - 1];
					
					if (first) {
						Line (minx, y, maxx, y, drawUpdates);
						first = false;
					} else {
						if (minx >= lastminx) {
							var mnx = (minx > lastminx) ? lastminx + 1 : lastminx;
							Line (mnx, y, minx, y, drawUpdates);
						}
					
						if (maxx <= lastmaxx) {
							var mxx = (maxx < lastmaxx) ? lastmaxx - 1 : lastmaxx;
							Line (mxx, y, maxx, y, drawUpdates);
						}
					}
					lastminx = minx;
					lastmaxx = maxx;
				}
				else
					first = true;
			}*/
			if (oldLineStyle != LineStyle.Solid)
				SetLineStyle(oldLineStyle);
			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		public void Ellipse(int x, int y, int startAngle, int endAngle, int radiusx, int radiusy, IList<Rectangle> updates = null)
		{
			var rows = CreateScanRows();
			if (startAngle > endAngle)
			{
				ScanEllipse(x, y, 0, endAngle, radiusx, radiusy, rows);
				ScanEllipse(x, y, startAngle, 360, radiusx, radiusy, rows);
			}
			else
				ScanEllipse(x, y, startAngle, endAngle, radiusx, radiusy, rows);
			DrawScan(rows, updates);

#if BLAH
			var drawUpdates = updates ?? new List<Rectangle> ();
			
			// Adjust for screen aspect ratio
			radiusx = Math.Max (1, radiusx);
			radiusy = Math.Max (1, radiusy);
			// check for an ellipse with negligable x and y radius 
			if (radiusx <= 1 && radiusy <= 1) {
				PutPixel (x, y, color, updates);
				return;
			}
			// check if valid angles
			while (start_angle < 0)
				start_angle += 360;
			while (end_angle < 0)
				end_angle += 360;
			while (start_angle > 360)
				start_angle -= 360;
			while (end_angle > 360)
				end_angle -= 360;
		
			// now, if stangle is greater than endangle, let's increase endangle
			if (start_angle > end_angle)
				end_angle += 360;
			
			int angle = 0;
			// Calculate points
			var oldLineStyle = this.GetLineStyle ();
			if (oldLineStyle != LineStyle.Solid)
				SetLineStyle (LineStyle.Solid);
			int inc = 0;
			bool first = true;
			int lastx = 0;
			int lasty = 0;
			do {
				float alpha = (float)angle * (float)(Math.PI / 180.0);
				float sinalpha = (float)Math.Sin (alpha);
				float cosalpha = (float)Math.Cos (alpha);
				
				var nextx = (int)Math.Round ((float)radiusx * cosalpha);
				var nexty = (int)Math.Round ((float)radiusy * sinalpha);				
				
				if (first) {
					lastx = nextx;
					lasty = nexty;
					first = false;
				} else { /*if ((angle % 4) == 0 || angle == 90)*/
					var a = angle;
					if (a >= start_angle && a <= end_angle)
						Line (x + lastx, y + lasty, x + nextx, y + nexty, drawUpdates);
					a = 180 - angle;
					if (a >= start_angle && a <= end_angle)
						Line (x - lastx, y + lasty, x - nextx, y + nexty, drawUpdates);
					a = 180 + angle;
					if (a >= start_angle && a <= end_angle)
						Line (x - lastx, y - lasty, x - nextx, y - nexty, drawUpdates);
					a = 360 - angle;
					if (a >= start_angle && a <= end_angle)
						Line (x + lastx, y - lasty, x + nextx, y - nexty, drawUpdates);
					lastx = nextx;
					lasty = nexty;
				}
				

				angle++;
				
			} while (angle <= 90);
			if (oldLineStyle != LineStyle.Solid)
				SetLineStyle (oldLineStyle);
			
			/**/
			
			if (updates == null)
				UpdateRegion (drawUpdates);
#endif
		}

		void symmetryFill(int x, int y, int xoffset, int yoffset, IList<Rectangle> updates)
		{
			Bar(x - xoffset, y - yoffset, x + xoffset, y - yoffset, updates);
			Bar(x - xoffset, y + yoffset, x + xoffset, y + yoffset, updates);
		}

		static bool InAngle(int angle, int startAngle, int endAngle)
		{
			return (angle >= startAngle && angle <= endAngle);
		}

		void symmetryScan(int x, int y, int startAngle, int endAngle, int xoffset, int yoffset, int angle, bool horizontal, IList<int>[] rows)
		{
			if (lineThickness == 1)
			{
				if (InAngle(angle, startAngle, endAngle))
					AddScanRow(rows, x + xoffset, y - yoffset);
				if (InAngle(180 - angle, startAngle, endAngle))
					AddScanRow(rows, x - xoffset, y - yoffset);
				if (InAngle(180 + angle, startAngle, endAngle))
					AddScanRow(rows, x - xoffset, y + yoffset);
				if (InAngle(360 - angle, startAngle, endAngle))
					AddScanRow(rows, x + xoffset, y + yoffset);
			}
			else
			{
				int offset = lineThickness / 2;
				if (horizontal)
				{
					if (InAngle(angle, startAngle, endAngle))
						AddScanHorizontal(rows, x + xoffset - offset, y - yoffset, lineThickness);
					if (InAngle(180 - angle, startAngle, endAngle))
						AddScanHorizontal(rows, x - xoffset - offset, y - yoffset, lineThickness);
					if (InAngle(180 + angle, startAngle, endAngle))
						AddScanHorizontal(rows, x - xoffset - offset, y + yoffset, lineThickness);
					if (InAngle(360 - angle, startAngle, endAngle))
						AddScanHorizontal(rows, x + xoffset - offset, y + yoffset, lineThickness);
				}
				else
				{
					if (InAngle(angle, startAngle, endAngle))
						AddScanVertical(rows, x + xoffset, y - yoffset - offset, lineThickness);
					if (InAngle(180 - angle, startAngle, endAngle))
						AddScanVertical(rows, x - xoffset, y - yoffset - offset, lineThickness);
					if (InAngle(180 + angle, startAngle, endAngle))
						AddScanVertical(rows, x - xoffset, y + yoffset - offset, lineThickness);
					if (InAngle(360 - angle, startAngle, endAngle))
						AddScanVertical(rows, x + xoffset, y + yoffset - offset, lineThickness);
				}

			}
		}

		void symmetryVertical(int x, int y, int xoffset, int yoffset, IList<Rectangle> updates)
		{
			int ls = -lineThickness / 2;
			int le = (lineThickness + 1) / 2;
			for (int i = ls; i < le; i++)
			{
				PutPixel(x + xoffset, y + yoffset - i, color, updates);
				PutPixel(x - xoffset, y + yoffset - i, color, updates);
				PutPixel(x - xoffset, y - yoffset + i - ls, color, updates);
				PutPixel(x + xoffset, y - yoffset + i - ls, color, updates);
			}
		}

		void symmetryHorizontal(int x, int y, int xoffset, int yoffset, IList<Rectangle> updates)
		{
			int ls = -lineThickness / 2;
			//int le = (lineThickness + 1) / 2;
			for (int i = 0; i < lineThickness; i++)
			{
				PutPixel(x + xoffset - i, y + yoffset, color, updates);
				PutPixel(x - xoffset - i - ls, y + yoffset, color, updates);
				PutPixel(x - xoffset - i - ls, y - yoffset, color, updates);
				PutPixel(x + xoffset - i, y - yoffset, color, updates);
			}
		}

		void symmetryLine(int x, int y, int xoffset, int yoffset, int length, IList<Rectangle> updates)
		{
			Line(x + xoffset, y + yoffset, x + xoffset + length, y + yoffset, updates);
			Line(x - xoffset, y + yoffset, x - xoffset - length, y + yoffset, updates);
			Line(x - xoffset, y - yoffset, x - xoffset - length, y - yoffset, updates);
			Line(x + xoffset, y - yoffset, x + xoffset + length, y - yoffset, updates);
		}

		public void Circle(int x, int y, int radius, IList<Rectangle> updates = null)
		{
			int ry = (int)(radius * ASPECT);
			int rx = radius;
			Ellipse(x, y, rx, ry, updates);
		}

		public void Ellipse(Rectangle rect, IList<Rectangle> updates = null)
		{
			Ellipse(rect.Center.X, rect.Center.Y, rect.Width / 2, rect.Height / 2, updates);
		}

		public void Ellipse(int x, int y, int radiusx, int radiusy, IList<Rectangle> updates = null)
		{
			Ellipse(x, y, 0, 360, radiusx, radiusy, updates);
		}

		public void FillEllipse(Rectangle rect, IList<Rectangle> updates = null)
		{
			FillEllipse(rect.Center.X, rect.Center.Y, rect.Width / 2, rect.Height / 2, updates);
		}

		public void FillEllipse(int x, int y, int radiusx, int radiusy, IList<Rectangle> updates = null)
		{
			var drawUpdates = updates ?? new List<Rectangle>();

			var rows = CreateScanRows();
			ScanEllipse(x, y, 0, 360, radiusx, radiusy, rows);
			FillScan(rows, drawUpdates);
			DrawScan(rows, drawUpdates);

			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		public static Size GetAngleSize(int angle, Size radius)
		{
			return GetAngleSize(angle, radius.Width, radius.Height);
		}

		public static Size GetAngleSize(int angle, int radiusx, int radiusy)
		{
			return new Size(
				Round(Math.Cos(angle * deg2rad) * radiusx),
				-Round(Math.Sin(angle * deg2rad) * radiusy)
			);
		}

		public void Sector(int x, int y, int startAngle, int endAngle, int radiusx, int radiusy, IList<Rectangle> updates = null)
		{
			var drawUpdates = updates ?? new List<Rectangle>();
			var center = new Point(x, y);
			var rows = CreateScanRows();
			var startPoint = center + GetAngleSize(startAngle, radiusx, radiusy);
			var endPoint = center + GetAngleSize(endAngle, radiusx, radiusy);

			var oldthickness = GetLineThickness();
			if (lineStyle != LineStyle.Solid)
				lineThickness = 1;
			ScanEllipse(x, y, startAngle, endAngle, radiusx, radiusy, rows);

			ScanLine(center, startPoint, rows, true);
			ScanLine(center, endPoint, rows, true);

			if (fillStyle != FillStyle.Empty)
				FillScan(rows, drawUpdates);

			if (lineStyle == LineStyle.Solid)
			{
				rows = CreateScanRows(); // ugh, twice, really?!
				ScanEllipse(x, y, startAngle, endAngle, radiusx, radiusy, rows);
				DrawScan(rows, drawUpdates);
			}
			if (lineStyle != LineStyle.Solid)
				lineThickness = oldthickness;

			Line(center, startPoint, drawUpdates);
			Line(center, endPoint, drawUpdates);


			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		public void PieSlice(int x, int y, int startAngle, int endAngle, int radius, IList<Rectangle> updates = null)
		{
			Sector(x, y, startAngle, endAngle, radius, Trunc(radius * ASPECT), updates);
		}

		public void Bar(int left, int top, int right, int bottom, IList<Rectangle> updates = null)
		{
			Bar(new Rectangle(left, top, right - left + 1, bottom - top + 1), updates);
		}

		public void Bar(Rectangle rect, IList<Rectangle> updates = null)
		{
			rect.Restrict(viewPort);
			if (rect.Width == 0 || rect.Height == 0)
				return;
			lock (bitmapLock)
			{
				var innerRight = rect.InnerRight;
				var innerBottom = rect.InnerBottom;
				int ystart = rect.Top * bd.ScanWidth + rect.Left;
				if (fillStyle == FillStyle.Solid)
				{
					for (int y = rect.Top; y <= innerBottom; y++)
					{
						int xstart = ystart;
						for (int x = rect.Left; x <= innerRight; x++)
						{
							bits[xstart] = fillcolor;
							xstart++;
						}
						ystart += bd.ScanWidth;
					}
				}
				else
				{
					byte[] pattern = fillStyles[(int)fillStyle];
					int ypat = rect.Top % 8;
					for (int y = rect.Top; y <= innerBottom; y++)
					{
						int xstart = ystart;
						byte xpatmask = (byte)(128 >> (rect.Left % 8));
						byte pat = pattern[ypat];
						for (int x = rect.Left; x <= innerRight; x++)
						{
							bits[xstart] = ((pat & xpatmask) != 0) ? fillcolor : bkcolor;
							xstart++;
							xpatmask >>= 1;
							if (xpatmask == 0)
								xpatmask = 128;
						}
						ypat = (ypat + 1) % 8;
						ystart += bd.ScanWidth;
					}
				}
			}
			if (updates != null)
				updates.Add(rect);
			else
				UpdateRegion(rect);
		}

		public void Bar3d(Rectangle rect, int depth, int topflag, IList<Rectangle> updates = null)
		{
			Bar3d(rect.Left, rect.Top, rect.InnerRight, rect.InnerBottom, depth, topflag, updates);
		}

		public void Bar3d(int left, int top, int right, int bottom, int depth, int topflag, IList<Rectangle> updates = null)
		{
			int temp;
			const double tan30 = 1.0 / 1.73205080756887729352;
			if (left > right)
			{
				temp = left;
				left = right;
				right = temp;
			}
			if (bottom < top)
			{
				temp = bottom;
				bottom = top;
				top = temp;
			}
			var drawUpdates = updates ?? new List<Rectangle>();
			Bar(left + lineThickness, top + lineThickness, right - lineThickness + 1, bottom - lineThickness + 1, drawUpdates);

			int dy = (int)(depth * tan30);
			var p = new Point[topflag != 0 ? 11 : 8];
			p[0].X = right;
			p[0].Y = bottom;
			p[1].X = right;
			p[1].Y = top;
			p[2].X = left;
			p[2].Y = top;
			p[3].X = left;
			p[3].Y = bottom;
			p[4].X = right;
			p[4].Y = bottom;
			p[5].X = right + depth;
			p[5].Y = bottom - dy;
			p[6].X = right + depth;
			p[6].Y = top - dy;
			p[7].X = right;
			p[7].Y = top;

			if (topflag != 0)
			{
				p[8].X = right + depth;
				p[8].Y = top - dy;
				p[9].X = left + depth;
				p[9].Y = top - dy;
				p[10].X = left;
				p[10].Y = top;
			}
			DrawPoly(p, drawUpdates);
			UpdateRegion(drawUpdates);
			if (updates == null)
				UpdateRegion(drawUpdates);
		}

		public void Arc(int x, int y, int startAngle, int endAngle, int radius, IList<Rectangle> updates = null)
		{
			Ellipse(x, y, startAngle, endAngle, radius, Trunc(radius * ASPECT), updates);
		}

		public WriteMode GetWriteMode()
		{
			return writeMode;
		}

		public void SetWriteMode(WriteMode mode)
		{
			writeMode = mode;
		}

		public int[] GetPalette()
		{
			return colors;
		}

		public void SetPalette(int index, int color, IList<Rectangle> updates = null)
		{
			lock (bitmapLock)
			{
				colors[index] = color;
				pal[index] = palEga[color];
				Palette currentPal = bmp.Palette;
				currentPal[index] = pal[index];
				bmp.Palette = currentPal;
			}

			if (index == 0)
				bkcolor = 0;
			if (updates == null)
				UpdateRegion(viewPort);
			else
				updates.Add(viewPort);
		}

		public void SetPalette(int[] colors, IList<Rectangle> updates = null)
		{
			colors.CopyTo(this.colors, 0);

			lock (bitmapLock)
			{
				Palette currentPal = bmp.Palette;
				for (int i = 0; i < pal.Count; i++)
				{
					pal[i] = palEga[colors[i]];
					currentPal[i] = pal[i];
				}
				bmp.Palette = currentPal;
			}

			if (updates == null)
				UpdateRegion(viewPort);
			else
				updates.Add(viewPort);
		}

		public void SetFillPattern(byte[] upattern)
		{
			fillStyles[(int)FillStyle.User] = new byte[8];
			upattern.CopyTo(fillStyles[(int)FillStyle.User], 0);
		}

		public void SetFillPattern(byte[] upattern, byte c)
		{
			SetFillPattern(upattern);
			SetFillStyle(FillStyle.User, c);
		}

		public byte[] GetFillPattern(FillStyle fillStyle)
		{
			byte[] style = fillStyles[(int)fillStyle];
			var upattern = new byte[8];
			style.CopyTo(upattern, 0);
			return upattern;
		}

		public byte[] GetFillPattern()
		{
			return GetFillPattern(fillStyle);
		}

		public void ClearDevice(IList<Rectangle> updates = null)
		{
			Bar(new Rectangle(windowSize), updates);
			MoveTo(0, 0);
		}

		public void PutImage(Point point, BGIImage image, WriteMode bitblt, IList<Rectangle> updates = null)
		{
			PutImage(point.X, point.Y, image, bitblt, updates);
		}

		public void PutImage(int x, int y, BGIImage image, WriteMode bitblt, IList<Rectangle> updates = null)
		{
			if (image == null)
				return;

			int start = 0;
			int width = Math.Min(viewPort.InnerRight - x + 1, image.Size.Width);
			int height = Math.Min(viewPort.InnerBottom - y + 1, image.Size.Height);
			if (height <= 0 || width <= 0)
				return;
			lock (bitmapLock)
			{
				byte* pdest = bits + (y * bd.ScanWidth + x);
				if (bitblt == WriteMode.Copy)
				{
					for (int cy = 0; cy < height; cy++)
					{
						Marshal.Copy(image.Bits, start, new IntPtr(pdest), width);
						start += image.Size.Width;
						pdest += bd.ScanWidth;
					}
				}
				else
				{
					for (int cy = 0; cy < height; cy++)
					{
						switch (bitblt)
						{
							case WriteMode.Copy:
								Marshal.Copy(image.Bits, start, new IntPtr(pdest), width);
								break;
							case WriteMode.Xor:
								for (int cx = 0; cx < width; cx++)
									pdest[cx] ^= image.Bits[start + cx];
								break;
							case WriteMode.Or:
								for (int cx = 0; cx < width; cx++)
									pdest[cx] |= image.Bits[start + cx];
								break;
							case WriteMode.And:
								for (int cx = 0; cx < width; cx++)
									pdest[cx] &= image.Bits[start + cx];
								break;
							case WriteMode.Not:
								for (int cx = 0; cx < width; cx++)
									pdest[cx] = (byte)(pdest[cx] & (byte)~image.Bits[start + cx]);
								break;
						}
						start += image.Size.Width;
						pdest += bd.ScanWidth;
					}
				}
			}
			if (updates == null)
				UpdateRegion(new Rectangle(x, y, width, height));
			else
				updates.Add(new Rectangle(x, y, width, height));
		}

		public uint ImageSize(int x1, int y1, int x2, int y2)
		{
			return (uint)(2 * 2 + (x2 - x1 + 1) * (y2 - y1 + 1) * BITS_PER_PIXEL / 8);
		}

		public class BGIImage
		{
			public Point Origin { get; set; }

			public Size Size { get; set; }

			public byte[] Bits { get; set; }
		}

		public BGIImage GetImage(int x1, int y1, int x2, int y2)
		{
			return GetImage(new Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1) + 1, Math.Abs(y2 - y1) + 1));
		}

		public BGIImage GetImage(Rectangle rect)
		{
			rect.Restrict(viewPort);

			var bi = new BGIImage
			{
				Size = rect.Size,
				Origin = rect.TopLeft
			};

			int localImageSize = bi.Size.Width * bi.Size.Height;

			var imageBits = new byte[localImageSize];

			lock (bitmapLock)
			{
				int start = 0;
				byte* pdest = bits + (rect.Top * bd.ScanWidth + rect.Left);
				for (int cy = 0; cy < bi.Size.Height; cy++)
				{
					Marshal.Copy(new IntPtr(pdest), imageBits, start, bi.Size.Width);
					start += bi.Size.Width;
					pdest += bd.ScanWidth;
				}
			}
			bi.Bits = imageBits;

			return bi;
		}

		public void SetTextStyle(FontType font, Direction direction, int charSize)
		{
			if (charSize > 10)
				charSize = 10;
			if (charSize < 1)
				charSize = 1;

			this.font = font;
			textDirection = direction;
			characterSize = charSize;
		}

		public FontType GetFontType()
		{
			return font;
		}

		public Direction GetTextDirection()
		{
			return textDirection;
		}

		public int GetFontSize()
		{
			return characterSize;
		}

		public void OutText(string str, IList<Rectangle> updates = null)
		{
			if (string.IsNullOrEmpty(str))
				return;

			IBGIFont fd = LoadFont(font);
			if (fd != null)
			{
				Size textSize = fd.GetTextSize(str, textDirection, characterSize);
				PointF cur = currentPos;
				if (textDirection == Direction.Vertical)
					currentPos.Y += textSize.Height;
				var drawUpdates = updates ?? new List<Rectangle>();

				foreach (char c in str)
				{
					var width = fd.DrawCharacter(this, cur.X, cur.Y, textDirection, characterSize, (byte)c, drawUpdates);
					if (textDirection == Direction.Horizontal)
						cur.X += width;
					else
						cur.Y -= width;
				}
				if (updates == null)
					UpdateRegion(drawUpdates);
				currentPos = new Point(cur);
			}
		}

		public void OutTextXY(Point point, string str, IList<Rectangle> updates = null)
		{
			OutTextXY(point.X, point.Y, str, updates);
		}

		public void OutTextXY(int x, int y, string str, IList<Rectangle> updates = null)
		{
			if (string.IsNullOrEmpty(str))
				return;
			var loadedFont = LoadFont(font);
			int oldThickness = lineThickness;
			lineThickness = 1;
			var oldline = GetLineStyle();
			SetLineStyle(LineStyle.Solid);

			if (loadedFont != null)
			{
				var drawUpdates = updates ?? new List<Rectangle>();
				Size textSize = loadedFont.GetTextSize(str, textDirection, characterSize);
				float xf = x;
				float yf = y;

				if (textDirection == Direction.Vertical)
					yf += textSize.Height;
				foreach (byte c in BGICanvas.Encoding.GetBytes(str))
				{
					var width = loadedFont.DrawCharacter(this, xf, yf, textDirection, characterSize, c, drawUpdates);
					if (textDirection == Direction.Horizontal)
						xf += width;
					else
						yf -= width;
				}
				if (updates == null)
					UpdateRegion(drawUpdates);
				//UpdateRegion(x, y, x+textSize.Width-1, y+textSize.Height-1);
			}
			lineThickness = oldThickness;
			SetLineStyle(oldline);
		}

		public IBGIFont LoadFont()
		{
			return LoadFont(font);
		}

		public IBGIFont LoadFont(FontType font)
		{
			IBGIFont fontData;
			if (!fontCache.TryGetValue(font, out fontData))
			{
				string fontName = fontTypes[(int)font];
				if (fontName == string.Empty)
				{
					Pablo.Formats.Character.BitFont f = Pablo.Formats.Character.BitFont.GetStandard8x8();
					fontData = f;
				}
				else
				{
					var stream = GetType().Assembly.GetManifestResourceStream(string.Format("Pablo.BGI.Fonts.{0}", fontName));
					var bf = new BGIFont();
					bf.Load(stream);
					fontData = bf;
				}

				fontCache[font] = fontData;
			}

			return fontData;
		}


		#region IDisposable Members

		public void Dispose()
		{
			lock (bitmapLock)
			{
				if (bmp != null)
				{
					bd.Dispose();
					bd = null;
					bmp.Dispose();
					bmp = null;
				}
				if (graphics != null)
				{
					graphics.Dispose();
					graphics = null;
				}

			}
		}

		public void UpdateRegion(int left, int top, int right, int bottom, Graphics graphics = null)
		{
			if (left > right)
			{
				int temp = left;
				left = right;
				right = temp;
			}
			if (top > bottom)
			{
				int temp = top;
				top = bottom;
				bottom = temp;
			}

			UpdateRegion(new [] { new Rectangle(left, top, right - left + 1, bottom - top + 1) }, graphics);
		}

		public void UpdateRegion(Rectangle rect, Graphics graphics = null)
		{
			UpdateRegion(new [] { rect }, graphics);
		}

		readonly object bitmapLock = new object();
		public void UpdateRegion(IList<Rectangle> rects, Graphics graphics = null)
		{
			const int ComplexityMaximum = 1000;
			if (Application.Instance == null)
				return;
			if (rects.Count > 0)
			{
				var penCache = new Dictionary<int, List<Rectangle>>();
				bool toocomplex = !IsDefaultScale;
				var rectBounds = rects[0];
				foreach (var rect in rects)
				{
					rectBounds = Eto.Drawing.Rectangle.Union(rect, rectBounds);
				}
				rectBounds.Normalize();
				rectBounds.Restrict(viewPort);
				if (rectBounds.Width == 0 || rectBounds.Height == 0)
					return;

				var platform = Platform.Instance;
				if (true) //platform.IsWpf || platform.IsIos || platform.IsMac || platform.IsWinForms)
				{
					Application.Instance.AsyncInvoke(delegate
					{

						lock (bitmapLock)
						{
							if (bmp != null && Control != null)
							{ // in case it was disposed
							  /*foreach (var rect in rects) {
								  rect.Restrict (viewPort);
								  this.Control.Invalidate (rect);
							  }*/
							  //Console.WriteLine ("Rect: {0}, {1}", rectBounds, new System.Diagnostics.StackTrace());
								var rect = Eto.Drawing.Rectangle.Round((RectangleF)rectBounds / Scale);
								rect.Inflate(1, 1);
								rect.Restrict(Point.Empty, Control.Size);
								Control.Invalidate(rect);
							}
						}
					});
				}
				else
				{
					/**/
					if (rectBounds.Width * rectBounds.Height > 5000)
						toocomplex = true;
					/**/
					int rectcount = 0;

					/* Check each pixel if it is too complex */
					if (!toocomplex)
					{

						lock (bitmapLock)
						{
							foreach (var rect in rects)
							{
								rect.Restrict(viewPort);
								if (rect.Height <= 0 || rect.Width <= 0)
									continue;
								int left = rect.Left;
								int right = rect.InnerRight;
								int bottom = rect.InnerBottom;
								int top = rect.Top;

								if (bottom < 0 || right < 0 || left > viewPort.InnerRight || top > viewPort.InnerBottom)
									continue;

								List<Rectangle> al;
								int ystart = top * bd.ScanWidth + left;
								int col = 0;

								if (rect.Width > rect.Height)
								{
									for (int y = top; y <= bottom; y++)
									{
										int lastcol = -1;
										int lastcolx = -1;
										int xstart = ystart;
										Rectangle r;
										for (int x = left; x <= right; x++)
										{
											col = bits[xstart++];
											if (col != lastcol)
											{
												if (lastcolx != -1)
												{
													if (!penCache.TryGetValue(lastcol, out al))
													{
														al = new List<Rectangle>();
														penCache[lastcol] = al;
													}
													r = new Rectangle(lastcolx, y, x - lastcolx, 1);
													al.Add(r);
													rectcount += r.Width * r.Height;
												}
												lastcolx = x;
												lastcol = col;
											}
										}
										if (lastcolx == -1)
											lastcolx = left;

										if (!penCache.TryGetValue(col, out al))
										{
											al = new List<Rectangle>();
											penCache[lastcol] = al;
										}
										r = new Rectangle(lastcolx, y, right - lastcolx + 1, 1);
										al.Add(r);
										rectcount += r.Width * r.Height;

										ystart += bd.ScanWidth;
										if (rectcount > ComplexityMaximum)
										{
											toocomplex = true;
											break;
										}
									}
								}
								else
								{
									for (int x = left; x <= right; x++)
									{
										int lastcol = -1;
										int lastcoly = -1;
										int xstart = ystart;
										Rectangle r;
										for (int y = top; y <= bottom; y++)
										{
											col = bits[xstart];
											xstart += bd.ScanWidth;
											if (col != lastcol)
											{
												if (lastcoly != -1)
												{
													if (!penCache.TryGetValue(lastcol, out al))
													{
														al = new List<Rectangle>();
														penCache[lastcol] = al;
													}
													r = new Rectangle(x, lastcoly, 1, y - lastcoly);
													al.Add(r);
													rectcount += r.Width * r.Height;
												}
												lastcoly = y;
												lastcol = col;
											}
										}
										if (lastcoly == -1)
											lastcoly = top;

										if (!penCache.TryGetValue(col, out al))
										{
											al = new List<Rectangle>();
											penCache[lastcol] = al;
										}
										r = new Rectangle(x, lastcoly, 1, bottom - lastcoly + 1);
										al.Add(r);
										rectcount += r.Width * r.Height;

										ystart++;
										if (rectcount > ComplexityMaximum)
										{
											toocomplex = true;
											break;
										}
									}

								}
								/**/
								if (rectcount > ComplexityMaximum)
								{
									toocomplex = true;
									break;
								}
								/**/
							}
						}
					}
					/*
					 */
					Application.Instance.Invoke(delegate
					{

						lock (bitmapLock)
						{
							if (bmp != null)
							{ // in case it was disposed
								var g = graphics ?? CreateGraphics();
								if (g != null)
								{
									/**/
									if (toocomplex)
									{
										// too many rects to draw, so just blit out the bitmap, which is faster
										bd.Dispose();
										if (!IsDefaultScale)
										{
											rectBounds.Inflate(1, 1);
											rectBounds.Restrict(viewPort);
											var newBounds = Eto.Drawing.Rectangle.Round((RectangleF)rectBounds / Scale);
											g.DrawImage(bmp, Eto.Drawing.Rectangle.Round((RectangleF)newBounds * Scale), newBounds);
										}
										else
										{
											g.DrawImage(bmp, rectBounds, rectBounds);
										}

										bd = bmp.Lock();
										bits = (byte*)bd.Data;
										/**
										totalrectcomplexity += rectBounds.Width * rectBounds.Height;
										if (!DelayDraw || totalrectcomplexity > 300) {
											totalrectcomplexity = 0;	
											g.Flush ();
										}
										/**/
										g.Flush();
										/**/
									}
									else
									{
										/**/
										// doing really small bitmap blits is slow, so fill the rectangles here
										foreach (int curcol in penCache.Keys)
										{
											var col = pal[curcol];
											foreach (var rect in penCache[curcol])
											{
												g.FillRectangle(col, rect);
											}
										}
										totalrectcomplexity += rectcount;
										if (!DelayDraw || totalrectcomplexity > 300)
										{
											totalrectcomplexity = 0;
											g.Flush();
										}
										/**/
									}

								}
								//ResetGraphics ();
							}
						}
					});
				}
			}
		}


		#endregion
	}
}
