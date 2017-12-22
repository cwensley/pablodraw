using System;
using System.IO;
using Eto.Drawing;

namespace Pablo.Formats.Image
{
	public class FormatImage : Format
	{
		ImageFormat imageFormat;
		public FormatImage(DocumentInfo info, ImageFormat imageFormat, string id, string name, params string[] extensions) : base(info, id, name, extensions)
		{
			this.imageFormat = imageFormat;
		}
		
		public override bool CanSave {
			get { return true; }
		}

		public void Load(Stream fs, Document document, Pablo.Handler handler)
		{
			ImageDocument doc = (ImageDocument)document;
			doc.Image = new Bitmap(fs);
		}
		
		public void Save(Stream stream, Document document)
		{
			ImageDocument doc = (ImageDocument)document;
			if (doc.Image != null) doc.Image.Save(stream, imageFormat);
		}
	}
}
