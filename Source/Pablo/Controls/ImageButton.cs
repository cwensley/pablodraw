using System;
using Eto.Drawing;

namespace Pablo.Controls
{
	public class ImageButton : CustomButton
	{
		bool sizeSet;
		public Image Image { get; set; }

		public Image DisabledImage { get; set; }
		
		public override Size Size {
			get {
				return base.Size;
			}
			set {
				base.Size = value;
				sizeSet = true;
			}
		}
		
		public ImageButton ()
		{
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			if (Image != null) {
				if (!sizeSet)
					this.Size = Image.Size + 4;
			}
		}
		
		protected override void OnLoadComplete (EventArgs e)
		{
			base.OnLoadComplete (e);
			
			if (DisabledImage == null) {
				var image = Image;
				if (image != null) {
					var disabledImage = new Bitmap (image.Size.Width, image.Size.Height, PixelFormat.Format32bppRgba);
					using (var graphics = new Graphics(disabledImage)) {
						graphics.DrawImage (image, 0, 0);
					}
					using (var bd = disabledImage.Lock())
					{
						unsafe
						{
							var data = (int*)bd.Data;
							for (int i = 0; i < bd.ScanWidth * disabledImage.Size.Height; i++)
							{
								var col = Color.FromArgb(bd.TranslateDataToArgb(*data));
								var gray = (col.R + col.G + col.B) / 3;
								*data = bd.TranslateArgbToData(Color.FromGrayscale(gray, 0.8f).ToArgb());
							}
						}
					}
				}
			}
		}
		
		protected override void OnPaint (Eto.Forms.PaintEventArgs pe)
		{
			base.OnPaint (pe);
			
			var image = this.Enabled ? Image : DisabledImage;
			var size = image.Size.FitTo (this.Size - 2);
			var xoffset = (this.Size.Width - size.Width) / 2;
			var yoffset = (this.Size.Height - size.Height) / 2;
			pe.Graphics.DrawImage (image, xoffset, yoffset, size.Width, size.Height);
		}
	}
}

