using System;
using System.IO;
using Eto.Drawing;

namespace Pablo.Formats.Character
{
	public class Page
	{
		BitFont font;
		Palette pal;
		Canvas canvas;
		CharacterDocument document;

		public event EventHandler<EventArgs> Loaded;
		public event EventHandler<EventArgs> PaletteChanged;
		public event EventHandler<EventArgs> FontChanged;

		public Page(CharacterDocument document, Size canvasSize, BitFont font, Palette palette)
		{
			this.document = document;
			canvas = new MemoryCanvas(canvasSize);
			pal = palette;
			pal.CollectionChanged += InternalPaletteChanged;
			this.Font = font;
		}

		void InternalPaletteChanged(object sender, EventArgs e)
		{
			if (PaletteChanged != null)
				PaletteChanged(this, EventArgs.Empty);
		}

		void InternalFontChanged(object sender, EventArgs e)
		{
			OnFontChanged(EventArgs.Empty);
		}

		bool isChangingFont;
		protected virtual void OnFontChanged(EventArgs e)
		{
			if (!isChangingFont)
			{
				isChangingFont = true;
				if (FontChanged != null)
					FontChanged(this, EventArgs.Empty);
				document.SetFont(false, true);
				isChangingFont = false;
			}
		}

		public Canvas Canvas
		{
			get { return canvas; }
		}

		public Size PixelSize
		{
			get { return new Size(canvas.Size.Width * font.Width, canvas.Size.Height * font.Height); }
		}

		public Palette Palette
		{
			get { return pal; }
			set
			{
				if (pal != null)
				{
					pal.CollectionChanged -= InternalPaletteChanged;
				}
				pal = value;
				canvas.UpdatePalette(pal);
				pal.CollectionChanged += InternalPaletteChanged;
				if (PaletteChanged != null)
					PaletteChanged(this, EventArgs.Empty);
			}
		}

		public BitFont Font
		{
			get { return font; }
			set
			{
				if (font != null)
				{
					font.Changed -= InternalFontChanged;
				}
				font = new BitFont(value);
				font.Changed += InternalFontChanged;
				OnFontChanged(EventArgs.Empty);
			}
		}

		public void Load(Stream fs, CharacterFormat format, CharacterHandler handler, bool resizeCanvas)
		{
			// load the file!
			Canvas.Clear();
			format.Load(fs, document, handler);
			if (Loaded != null)
				Loaded(this, EventArgs.Empty);
		}

		public void GenerateRegion(Bitmap bitmap, Rectangle rect, Size fontSize, BitFont customFont, Palette customPalette, bool iceColor, bool blinkOn, Point? cursorPosition, IGenerateRegion generator)
		{
			GenerateRegion(canvas, bitmap, rect, fontSize, customFont ?? font, customPalette ?? pal, iceColor, blinkOn, cursorPosition, generator);
		}

		class EmptyGenerator : IGenerateRegion
		{
			public CanvasElement? GetElement(Point point, Canvas canvas)
			{
				return null;
			}

			public void TranslateColour(Point point, ref CanvasElement ce, ref int foreColour, ref int backColour)
			{
			}
		}

		public unsafe static void GenerateRegion(Canvas canvas, Bitmap bitmap, Rectangle rect, Size fontSize, BitFont font, Palette pal, bool iceColor, bool blinkOn, Point? cursorPosition, IGenerateRegion generator)
		{
			unchecked
			{
				//Console.WriteLine ("Generating region: {0}, bitmap size: {1}", rect, bitmap.Size);
				/*
			 * hmm.. quite a bit faster drawing character by character (less array lookups)
			 */
				var startCanvas = new Point(rect.Left / fontSize.Width, rect.Top / fontSize.Height);
				var startFont = new Point(((rect.Left % fontSize.Width) * font.Width) / fontSize.Width, ((rect.Top % fontSize.Height) * font.Height) / fontSize.Height);

				rect.Width = (rect.Width + fontSize.Width);
				rect.Height = rect.Height + fontSize.Height;
				var endCanvas = new Point(rect.Right / fontSize.Width, rect.Bottom / fontSize.Height);
				var endFont = new Point(((rect.Right % fontSize.Width) * font.Width) / fontSize.Width, ((rect.Bottom % fontSize.Height) * font.Height) / fontSize.Height);

				var rDraw = new Rectangle(startCanvas.X, startCanvas.Y, endCanvas.X - startCanvas.X, endCanvas.Y - startCanvas.Y);
				if (endFont.X == 0 && rDraw.Width > 1)
				{
					rDraw.Width--;
					endFont.X = font.Width;
				}
				if (endFont.Y == 0 && rDraw.Height > 1)
				{
					rDraw.Height--;
					endFont.Y = font.Height;
				}
				rDraw.Restrict(canvas.Size);
				//if (rDraw.InnerRight > canvas.Width) { rDraw.Width = rDraw.X

				var rectScn = new Rectangle(0, 0, rDraw.Width * font.Width - startFont.X - (font.Width - endFont.X), rDraw.Height * font.Height - startFont.Y - (font.Height - endFont.Y));
				if (rectScn.Width == 0 || rectScn.Height == 0)
					return;

				CanvasElement ce;

				BitmapData bd = bitmap.Lock();
				var pRow = (byte*)bd.Data;

				uint* pCharStart;
				int foreColor, backColor;
				int cursorForeColor = 0, cursorBackColor = 0;

				int charStartY = startFont.Y;
				if (startFont.Y < 0)
				{
					pRow += (bd.ScanWidth * (-startFont.Y));
					charStartY = 0;
				}
				int fontHeight = font.Height;

				generator = generator ?? new EmptyGenerator();

				int scanWidth = bd.ScanWidth;
				if (bd.Flipped)
				{
					pRow += bd.ScanWidth * (rDraw.Height - 1);
					scanWidth = -scanWidth;
				}
				var pt = new Point();
				for (pt.Y = rDraw.Top; pt.Y < rDraw.Bottom; pt.Y++)
				{
					pCharStart = (uint*)pRow;
					int charStartX;
					if (startFont.X < 0)
					{
						pCharStart -= startFont.X;
						charStartX = 0;
					}
					else
						charStartX = startFont.X;
					if (pt.Y == rDraw.Bottom - 1)
						fontHeight = endFont.Y;
					if (pt.Y >= 0 && pt.Y < canvas.Height)
					{
						for (pt.X = rDraw.Left; pt.X < rDraw.Right; pt.X++)
						{
							var charEndX = pt.X == rDraw.Right - 1 ? endFont.X : font.Width;
							if (pt.X >= 0 && pt.X < canvas.Width)
							{
								ce = generator.GetElement(pt, canvas) ?? canvas[pt];
								/*
							 	*/
								if (ce.Foreground >= pal.Count)
									throw new InvalidOperationException();
								foreColor = pal.GetRGBColor(ce.Foreground);
								backColor = pal.GetRGBColor(iceColor ? ce.Background : ce.Attribute.BackgroundOnly);
								generator.TranslateColour(pt, ref ce, ref foreColor, ref backColor);
								bool cursor = false;
								if (cursorPosition != null && cursorPosition.Value == pt)
								{
									cursorForeColor = bd.TranslateArgbToData(~foreColor | (int)0xff000000);
									cursorBackColor = bd.TranslateArgbToData(~backColor | (int)0xff000000);
									cursor = true;
								}

								/*
							 *
							foreColor = pal[ce.Foreground]; //foreu = pal.GetRGBColor(ce.Foreground);
							if (iceColor) backColor = pal[ce.Background]; //backu = pal.GetRGBColor(ce.Background);
							else backColor = pal[ce.Attribute.BackgroundOnly]; //backu = pal.GetRGBColor(ce.Background & 0x7);
							/*
							 */
								bool shouldBeOn = iceColor | blinkOn | !ce.Attribute.Blink;
								var fc = font[ce.Character];
								foreColor = bd.TranslateArgbToData(foreColor);
								backColor = bd.TranslateArgbToData(backColor);
								// got our char & attrib info, now draw the character PIXEL BY PIXEL!
								var pCharY = (byte*)pCharStart;
								var chpos = charStartY * font.Width;
								for (int chary = charStartY; chary < fontHeight; chary++)
								{
									if (cursor && chary > fontHeight - 3)
									{
										foreColor = cursorForeColor;
										backColor = cursorBackColor;
										cursor = false;
									}
									var pCharX = (int*)pCharY;
									var endpos = chpos + charEndX;
									var charx = chpos + charStartX;
									var data = fc.GetData(charx / 32);
									var idx = 1 << charx;
									for (; charx < endpos; charx++)
									{
										if (idx == 0)
										{
											idx = 1 << charx;
											data = fc.GetData(charx / 32);
										}

										var on = shouldBeOn && (data & idx) != 0;
										idx <<= 1;
										*(pCharX++) = on ? foreColor : backColor;
									}
									pCharY += scanWidth;
									chpos += font.Width;
								}
							}
							pCharStart += (font.Width - charStartX);
							if (pt.X == rDraw.Left)
								charStartX = 0;
						}
					}
					pRow += (fontHeight - charStartY) * scanWidth;
					if (pt.Y == rDraw.Top)
						charStartY = 0;
				}

				bd.Dispose();
			}
		}


	}
}
