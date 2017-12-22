using System;
using System.IO;
using Eto.Drawing;
using Pablo.Network;

namespace Pablo.Formats.Image
{
	public class ImageDocument : Document
	{
		Bitmap image;

		public ImageDocument(DocumentInfo info) : base(info)
		{
		}

		public override Pablo.Handler CreateHandler()
		{
			return new Handler(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (image != null)
					image.Dispose();
			}
			base.Dispose(disposing);
		}

		public override Size Size
		{
			get { return image.Size; }
		}

		public Bitmap Image
		{
			get { return image; }
			set
			{
				if (image != null)
					image.Dispose();
				image = value;
			}
		}

		protected override void LoadStream(Stream stream, Format format, Pablo.Handler handler)
		{
			var formatImage = (FormatImage)format;
			formatImage.Load(stream, this, handler);
		}

		protected override void SaveStream(Stream stream, Format format, Pablo.Handler handler)
		{
			var formatImage = (FormatImage)format;
			formatImage.Save(stream, this);
		}

		public override Document ConvertDocument(DocumentInfo targetDocumentInfo, Pablo.Handler handler)
		{
			if (handler != null && targetDocumentInfo.ID == Info.ID && Math.Abs(handler.Zoom - 1f) > 0.001f)
			{
				var doc = (ImageDocument)targetDocumentInfo.Create();
				doc.Image = new Bitmap(Image, handler.DrawSize.Width, handler.DrawSize.Height, ImageInterpolation.High);
				return doc;
			}
			return base.ConvertDocument(targetDocumentInfo, handler);
		}

		public override bool IsModified { get; set; }

		public override bool Send(SendCommandArgs args)
		{
			base.Send(args);
			using (var stream = new MemoryStream())
			{
				Image.Save(stream, ImageFormat.Png);
				stream.Seek(0, SeekOrigin.Begin);
				args.Message.WriteStream((Stream)stream);
			}
			return true;
		}

		public override void Receive(ReceiveCommandArgs args)
		{
			base.Receive(args);
			var stream = args.Message.ReadStream();
			Image = new Bitmap(stream);
		}
	}
}
