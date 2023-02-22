using Eto.Drawing;
using System;

namespace Pablo.Formats.Image
{
	public class ImageHandler : Pablo.Handler
	{
		public ImageHandler(ImageDocument doc) : base(doc)
		{
		}
		
		public ImageDocument ImageDocument
		{
			get { return (ImageDocument)Document; }
		}

		public override Size Size
		{
			get { return (ImageDocument.Image != null) ? ImageDocument.Image.Size : new Size(0,0); }
		}

		public override SizeF Ratio
		{
			get { return new SizeF(1, 1); }
		}

		public override bool CanEdit
		{
			get { return false; }
		}
		
		public override void GenerateRegion(Graphics graphics, Rectangle rectSource, Rectangle rectDest)
		{
			if (ImageDocument.Image != null)
			{
				/*
				//g.InterpolationMode = Loader.Quality;
				Rectangle source = rectSource;
				Eto.Drawing.Image img = doc.Image;
				Size size = img.Size;
				if (source.X < 0) { source.Width += source.X; source.X = 0; }
				if (source.Y < 0) { source.Height += source.Y; source.Y = 0; }
				if (source.InnerRight > size.Width) { source.Width -= (source.InnerRight-size.Width); }
				if (source.InnerBottom > size.Height) { source.Height -= (source.InnerBottom-size.Height);  }

				Graphics g = new Graphics(bitmap.Generator, bitmap);
				g.DrawImage(img, source, new Point((rectSource.X < 0) ? -rectSource.X : 0, (rectSource.Y < 0) ? -rectSource.Y : 0));
				g.Commit();
				g.Dispose();
				*/

				graphics.DrawImage(ImageDocument.Image, rectSource, rectDest); //new Point((rect.X < 0) ? -rect.X : 0, (rect.Y < 0) ? -rect.Y : 0));
			}
			
		}

	}
}
