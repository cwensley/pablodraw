using System;
using System.IO;
using PabloDraw.Interface;
using PabloDraw.Handlers.Character;
using Eto.Drawing;
using Eto.Forms;

namespace PabloDraw.Handlers.Pix
{
	/// <summary>
	/// Summary description for TestHandler.
	/// </summary>
	public class Handler : IHandler
	{
		PixDocument doc;
		public Handler(PixDocument doc)
		{
			this.doc = doc;
		}
		#region IHandler Members

		public Size Size
		{
			get
			{
				return new Size(640, 350);
			}
		}

		public void GenerateRegion(Bitmap bitmap, Rectangle rect)
		{
			if (doc.Image != null)
			{
				//g.InterpolationMode = Loader.Quality; 
				Rectangle source = rect;
				Eto.Drawing.Image img = doc.Image;
				Size size = img.Size;
				if (source.X < 0) { source.Width += source.X; source.X = 0; }
				if (source.Y < 0) { source.Height += source.Y; source.Y = 0; }
				if (source.Right > size.Width) { source.Width -= (source.Right-size.Width); }
				if (source.Bottom > size.Height) { source.Height -= (source.Bottom-size.Height);  }

				if (rect.Width > 0 && rect.Height > 0)
				{
					Graphics g = new Graphics(bitmap.Generator, bitmap);
					g.DrawImage(img, source, new Point((rect.X < 0) ? -rect.X : 0, (rect.Y < 0) ? -rect.Y : 0));
					g.Commit();
					g.Dispose();
				}
			}
			
		}

		public void GenerateActions(ActionCollection actions, ActionItemCollection menu, ActionItemCollection toolbar)
		{
			//actions.Add("9px", "Use 9px Font|9px Font|Toggle the 9th pixel to emulate text mode", new EventHandler(Use9px));

			//ActionItemSubMenu aiFile = menu.AddSubMenu("&File");

			//aiFile.Actions.Add(actions["9px"]);
		}

		#endregion
	}
}
