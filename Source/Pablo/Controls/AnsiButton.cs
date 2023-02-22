using System;
using System.Linq;
using Eto.Drawing;
using Pablo.Formats.Character;

namespace Pablo.Controls
{
	public class AnsiButton : CustomButton
	{
		bool sizeSet;
		CharacterDocument document;
		public CharacterDocument Document
		{
			get => document;
			set
			{
				if (document != value)
				{
					document = value;
				}
			}
		}

		public override Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
				sizeSet = true;
			}
		}

		public AnsiButton()
		{
			Padding = new Padding(2);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Document != null)
			{
				if (!sizeSet)
				{
					Size = DrawSize + Padding.Size;
				}
			}
		}

		Page Page => Document?.Pages.First();
		Size DrawSize
		{
			get
			{
				var page = Page;
				if (page == null)
					return Size.Empty;
				return new Size(page.Canvas.Width, page.Canvas.Height * page.Font.Height / page.Font.Width);
			}
		}

		protected override void OnPaint(Eto.Forms.PaintEventArgs pe)
		{
			base.OnPaint(pe);

			var page = Document.Pages.First();
			
			var oldFont = page.Font;
			var bitFont = new BitFont(page.Font);
			var pixelSize = ParentWindow?.LogicalPixelSize ?? 1f;
			bitFont.Resize((int)Math.Round(pixelSize), (int)Math.Round(bitFont.Height / bitFont.Width * pixelSize), true, true);
			page.Font = bitFont;
			
			var renderSize = page.Canvas.Size * page.Font.Size;
			page.Palette[7] = SystemColors.ControlText;

			//Console.WriteLine("{0}, {1}, {2}", Size, rectScreen, rectGenerate);
			var drawRectangle = new Rectangle(renderSize);
			var generator = new TransparentBackgroundRegionGenerator();
			using var bmp = new Bitmap(renderSize.Width, renderSize.Height, PixelFormat.Format32bppRgba);
			page.GenerateRegion(bmp, drawRectangle, page.Font.Size, null, null, Document.ICEColours, false, null, generator);

			page.Font = oldFont;

			var drawSize = DrawSize;
			var offset = new Point((Size - drawSize) / 2);
			var rect = new Rectangle(offset, drawSize);
			pe.Graphics.DrawImage(bmp, rect);
		}
	}
}

