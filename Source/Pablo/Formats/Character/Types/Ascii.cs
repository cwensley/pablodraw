using System;
using System.IO;
using System.Text;
using Eto.Drawing;

namespace Pablo.Formats.Character.Types
{
	public class Ascii : Ansi
	{
		public Ascii(DocumentInfo info)
			: base(info, "ascii", "Ascii", "txt", "asc", "nfo")
		{
		}

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Ascii;
			FillSauceSize(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return document.Pages[0].Canvas.Size.Width != DefaultWidth || !document.IsUsingStandard8x16Font;
		}

		public override bool CanSave
		{
			get { return true; }
		}

		public override void Save(Stream stream, CharacterDocument document)
		{
			Page page = document.Pages[0];
			Save(stream, page.Canvas, page.Palette);
		}

		protected override int GetWidth(Stream stream, CharacterDocument document, object state)
		{
			if (stream != null && stream.CanSeek)
			{
				var last = stream.Position;
				int max = document.Size.Width;
				var reader = new StreamReader(stream);
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					max = Math.Max(max, line.Length);
				}
				stream.Position = last;
				return max;
			}
			return base.GetWidth(stream, document, state);
		}

		public override void Save(Stream stream, Canvas canvas, Palette palette)
		{
			CanvasElement element;
			
			Rectangle rect = new Rectangle(0, 0, canvas.Width, canvas.FindEndY(CanvasElement.Default) + 1);
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
			{
				int iXEnd = canvas.FindEndX(y, rect.Left, rect.InnerRight, CanvasElement.Default);
				
				for (int x = rect.Left; x <= iXEnd; x++)
				{
					element = canvas[x, y];
					
					if (element.Character == 0) // bad character for ascii, convert to space
						stream.WriteByte(32);
					else
						stream.WriteByte(element.Character);
				}
				stream.WriteByte(13);
				stream.WriteByte(10);
			}
		}

		public string SaveToText(Canvas canvas, Encoding encoding)
		{
			using (var stream = Save(canvas, Palette.GetDosPalette()))
			{
				var sr = new StreamReader(stream, encoding);
				return sr.ReadToEnd();
			}
		}
	}
}
