using System;
using System.Collections;
using System.IO;
using PabloDraw.Sauce;
using Eto.Drawing;

namespace PabloDraw.Handlers.Pix
{

	/// <summary>
	/// Summary description for Document.
	/// </summary>
	public class PixDocument : Document
	{
		private int width = 78;
		private Canvas canvas;

		public PixDocument()
		{
			Handler = new Handler(this);
			canvas = new Canvas(width);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose (disposing);
		}

		public override Size Size
		{
			get { return new Size(width, canvas.Rows.Count); }
		}

		public Canvas Canvas
		{
			get { return canvas; }
		}
			
		protected override void LoadStream(Stream stream, Format format)
		{
			format.Load(stream, this);
			OnLoaded(new EventArgs());
		}

	}
}
