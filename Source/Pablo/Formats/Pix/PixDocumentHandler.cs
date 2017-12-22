using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using PabloDraw.Sauce;

namespace PabloDraw.Handlers.Pix
{
	public class PixDocumentHandler : DocumentHandler
	{
		Size drawSize = new Size(0,0);
		Size adjust = new Size(0,0);
		Size charSize = new Size(10,10);
		Bitmap drawImage = null;
		Font font = new Font(FontFamily.GenericMonospace, 8);
		
		public override void OnLoaded()
		{
			UpdateZoom();
			Loader.Update();
			base.OnLoaded ();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (drawImage != null) drawImage.Dispose();
			}
			base.Dispose (disposing);
		}

		public new PixDocument Document
		{
			get { return (PixDocument)base.Document; }
		}

		public Bitmap DrawImage
		{
			get { return drawImage; }
		}

		public override Size DrawSize
		{
			get
			{
				return new Size(Document.Size.Width * charSize.Width, Document.Size.Height * charSize.Height);
			}
		}

		public override void OnResize()
		{
			UpdateZoom();
			Loader.Update();
			base.OnResize();
		}

		public override void UpdateZoom()
		{
			int zoom = Loader.ZoomPercent;
			if (Viewer != null)
			{
				Graphics gscreen = Viewer.CreateGraphics();
				charSize = Size.Round(gscreen.MeasureString("a", font));
				gscreen.Dispose();
			}
			else
				charSize = new Size(8, font.Height);

			if (Viewer != null && zoom == -1)
			{
				int width = Viewer.ClientSize.Width;
				if (width < 50) width = 50;
				int height = width * DrawSize.Height / DrawSize.Width;
				drawSize = new Size(width, height); 
			}
			else
			{
				int width = (DrawSize.Width * zoom / 100);
				int height = width * DrawSize.Height / DrawSize.Width;
				drawSize = new Size(width, height); 
			}
			if (drawImage == null || drawSize != drawImage.Size)
			{
				if (drawImage != null) 
				{
					drawImage.Dispose();
					drawImage = null;
				}

				if (drawSize.Width != 0 && drawSize.Height != 0)
				{
					if (Viewer != null)
					{
						// if viewing from the viewer, create compatible bitmap so it goes faster
						Graphics gscreen = Viewer.CreateGraphics();
						drawImage = new Bitmap(drawSize.Width, drawSize.Height, gscreen);
						gscreen.Dispose();
					}
					else drawImage = new Bitmap(drawSize.Width, drawSize.Height, PixelFormat.Format32bppArgb);
					Graphics g = Graphics.FromImage(drawImage);

					Bitmap b = new Bitmap(DrawSize.Width, DrawSize.Height, PixelFormat.Format32bppArgb);
					Graphics gImg = Graphics.FromImage(b);
					DrawIt(gImg);
					gImg.Dispose();
					g.InterpolationMode = Loader.Quality; 
					g.DrawImage(b, 0,0, drawSize.Width, drawSize.Height);
					g.Dispose();
					b.Dispose();
				}
			}

			if (Viewer != null) 
			{
				Viewer.AutoScrollMinSize = drawSize;
				adjust.Width = (Viewer.AutoScrollMinSize.Width < Viewer.ClientSize.Width) ? ((Viewer.ClientSize.Width - Viewer.AutoScrollMinSize.Width) / 2) : 0; 
				adjust.Height = (Viewer.AutoScrollMinSize.Height < Viewer.ClientSize.Height) ? ((Viewer.ClientSize.Height - Viewer.AutoScrollMinSize.Height) / 2) : 0; 
			}
			base.UpdateZoom ();
		}

		public override void UpdateQuality()
		{
			if (drawImage != null)
			{
				drawImage.Dispose();
				drawImage = null;
			}
			UpdateZoom();
			base.UpdateQuality();
		}


		public override void OnPaint(PaintEventArgs pe)
		{
			try
			{
				Rectangle rect = pe.ClipRectangle;
				Rectangle rectScreen = rect;
				rect.Offset(-Viewer.AutoScrollPosition.X, -Viewer.AutoScrollPosition.Y);
				rect.Location -= adjust;

				if (rect.Y < 0) { rectScreen.Y -= rect.Y; rect.Y = 0; }
				if (rect.X < 0) { rectScreen.X -= rect.X; rect.X = 0; }

				if (rect.Left > Viewer.AutoScrollMinSize.Width) rect.Size = new Size(0,0);
				if (rect.Right > Viewer.AutoScrollMinSize.Width) rect.Size = new Size(Viewer.AutoScrollMinSize.Width-rect.Left+1, rect.Height);
				if (rect.Top > Viewer.AutoScrollMinSize.Height) rect.Size = new Size(0,0);
				if (rect.Bottom > Viewer.AutoScrollMinSize.Height) rect.Size = new Size(rect.Width, Viewer.AutoScrollMinSize.Height-rect.Top+1);

				if (drawImage != null && rect.Width > 0 && rect.Height > 0)
				{
					rectScreen.Size = rect.Size;

					pe.Graphics.DrawImage(drawImage, rectScreen, rect, GraphicsUnit.Pixel);

					Rectangle rectClip = new Rectangle(new Point(0,0) + adjust, Viewer.AutoScrollMinSize);
					pe.Graphics.ExcludeClip(new Region(rectClip));
				}
			}
			catch (Exception e)
			{
				string msg = e.Message;
			}
			 
			Brush bb = new SolidBrush(Color.FromArgb(0,0,0));
			pe.Graphics.FillRectangle(bb, pe.ClipRectangle);
		}

		private void DrawIt(Graphics g)
		{
			Canvas canvas = Document.Canvas;
			
			SolidBrush b = new SolidBrush(Color.White);
			g.FillRectangle(b, 0, 0, DrawSize.Width, DrawSize.Height);
			b = new SolidBrush(Color.Black);
			Point start = new Point(0,0);
			for (int y=0;y<Document.Size.Height; y++)
			{
				start.X = 0;
				for (int x=0; x<Document.Size.Width; x++)
				{
					foreach (char c in canvas.Rows[y][x])
					{
						g.DrawString(c.ToString(), font, b, start);
					}
					start.X += charSize.Width;	
				}
				start.Y += charSize.Height;

			}
		}

	}
	
}
