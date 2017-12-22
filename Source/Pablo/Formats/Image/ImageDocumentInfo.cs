using System;
using Eto.Drawing;
using Eto;

namespace Pablo.Formats.Image
{
	/// <summary>
	/// Summary description for ImageDocumentInfo.
	/// </summary>
	public class ImageDocumentInfo : DocumentInfo
	{
		
		public const string DocumentID = "image";
		
		public ImageDocumentInfo() : base(DocumentID, "Image Document")
		{
			Formats.Add(new FormatImage(this, ImageFormat.Jpeg, "jpeg", "Jpeg", "jpeg", "jpg" ));
			Formats.Add(new FormatImage(this, ImageFormat.Gif, "gif", "GIF", "gif" ));
			Formats.Add(new FormatImage(this, ImageFormat.Tiff, "tiff", "TIFF", "tiff" ));
			Formats.Add(new FormatImage(this, ImageFormat.Png, "png", "PNG", "png" ));
			Formats.Add(new FormatImage(this, ImageFormat.Bitmap, "bmp", "Bitmap", "bmp" ));
			ZoomInfo.FitWidth = true;
			ZoomInfo.FitHeight = true;
		}

		public override Format DefaultFormat
		{
			get { return Formats["png"]; }
		}

		public override Document Create(Generator generator)
		{
			Document doc = new ImageDocument(this);
			doc.Generator = generator;
			return doc;
		}
		
		public override bool CanEdit
		{
			get { return false; }
		}
		
		
	}
}
