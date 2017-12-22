using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.BGI;
using Pablo.Controls;
using System.Reflection;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Controls
{
	public class FontStylePad : Panel
	{
		RipHandler handler;
		ImageButton vertical;
		ImageButton horizontal;
		BGICanvas canvas;
		NumericUpDown fontSize;

		public FontStylePad(RipHandler handler)
		{
			this.handler = handler;
			var layout = new DynamicLayout { Padding = Padding.Empty };

			layout.BeginVertical(Padding.Empty, Size.Empty);
			layout.Add(new Label { Text = "Font Size", HorizontalAlign = HorizontalAlign.Center, Font = new Font(SystemFont.Default, 7) });
			layout.Add(FontSize());
			layout.EndVertical();

			layout.BeginVertical(Padding.Empty, Size.Empty);
			layout.AddRow(null, HorizontalButton(), VerticalButton(), null);
			layout.EndVertical();

			layout.AddCentered(FontType(), horizontalCenter: true, verticalCenter: false, padding: Padding.Empty);
			layout.AddCentered(FillStyleButton(), horizontalCenter: true, verticalCenter: false, padding: Padding.Empty);
			Content = layout;
		}

		void DrawCanvas()
		{
			var updates = new List<Rectangle>();
			canvas.GraphDefaults(updates);
			canvas.SetTextStyle(handler.FontType, handler.TextDirection, handler.FontSize);
			var font = canvas.LoadFont(handler.FontType);
			var str = "AaBb";
			var size = font.GetRealTextSize(str, handler.TextDirection, handler.FontSize);
			size = (canvas.WindowSize - size) / 2;
			size.Width = Math.Max(size.Width, 0);
			canvas.OutTextXY(size.Width, size.Height, str, updates);
			canvas.Control.Invalidate();
		}

		void SelectFont()
		{
			var dialog = new FontTypeSelector(handler)
			{
				SelectedFont = handler.FontType,
				FontSize = handler.FontSize
			};
			if (dialog.ShowModal(this) == DialogResult.Ok)
			{
				handler.FontType = dialog.SelectedFont;
				handler.FontSize = dialog.FontSize;
				fontSize.Value = handler.FontSize;
				DrawCanvas();
			}
		}

		Control FontType()
		{
			var drawable = new Drawable
			{
				Size = new Size(40, 40)
			};

			drawable.MouseUp += delegate
			{
				SelectFont();
			};

			drawable.Paint += delegate(object sender, PaintEventArgs pe)
			{
				if (canvas != null)
					canvas.DrawRegion(pe.Graphics, new Rectangle(Point.Empty, canvas.WindowSize));
			};

			drawable.LoadComplete += delegate // Shown?
			{
				if (drawable.Size.IsEmpty)
					return;
				canvas = new BGICanvas(drawable, drawable.Size);
				DrawCanvas();
			};
			return drawable;
		}


		Control HorizontalButton()
		{
			var control = horizontal = new ImageButton
			{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Text-Horizontal.png"),
				Persistent = true,
				Pressed = handler.TextDirection == BGICanvas.Direction.Horizontal
			};

			control.Click += delegate
			{
				handler.TextDirection = BGICanvas.Direction.Horizontal;
				vertical.Pressed = false;
				DrawCanvas();
			};

			return control;
		}

		Control VerticalButton()
		{
			var control = vertical = new ImageButton
			{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Text-Vertical.png"),
				Persistent = true,
				Pressed = handler.TextDirection == BGICanvas.Direction.Vertical
			};

			control.Click += delegate
			{
				handler.TextDirection = BGICanvas.Direction.Vertical;
				horizontal.Pressed = false;
				DrawCanvas();
			};

			return control;
		}

		Control FillStyleButton()
		{
			var button = new CustomButton
			{
				Size = new Size(20, 12)
			};
			button.Paint += delegate(object sender, PaintEventArgs pe)
			{
				pe.Graphics.FillPolygon(Colors.Black, new PointF[] { new Point(4, 2), new Point(16, 5), new Point(4, 8) });
			};
			button.Click += delegate
			{
				SelectFont();
			};

			return button;
		}

		Control FontSize()
		{
			var control = fontSize = new NumericUpDown
			{
				MinValue = 1,
				MaxValue = 10,
				Font = new Font(SystemFont.Default, 8),
				Value = handler.FontSize,
				Size = new Size(20, -1)
			};
			control.ValueChanged += delegate
			{
				handler.FontSize = Math.Max(1, Math.Min(10, (int)control.Value));
				DrawCanvas();
			};
			return control;
		}
	}
}

