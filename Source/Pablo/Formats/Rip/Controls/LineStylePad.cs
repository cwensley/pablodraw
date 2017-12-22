using System;
using Eto.Forms;
using Pablo.BGI;
using Pablo.Controls;
using Eto.Drawing;
using System.Collections.Generic;
using System.Collections;

namespace Pablo.Formats.Rip.Controls
{
	public class LineStylePad : Panel
	{
		RipHandler handler;
		List<CustomButton> lineStyles = new List<CustomButton>();
		List<CustomButton> lineThicknesses = new List<CustomButton>();

		public LineStylePad(RipHandler handler, bool allowStyles)
		{
			this.handler = handler;
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };

			layout.BeginVertical(Padding.Empty, Size.Empty);
			layout.BeginHorizontal();

			layout.Add(LineSize(1), true);
			layout.Add(LineSize(3), true);

			layout.EndHorizontal();
			layout.EndVertical();

			if (allowStyles)
			{
				layout.Add(LineStyle(BGICanvas.LineStyle.Solid));
				layout.Add(LineStyle(BGICanvas.LineStyle.Dotted));
				layout.Add(LineStyle(BGICanvas.LineStyle.Dashed));
				layout.Add(LineStyle(BGICanvas.LineStyle.Center));
				layout.Add(LineStyle(BGICanvas.LineStyle.User));

				layout.BeginVertical(new Padding(0, 5, 0, 0));
				layout.AddRow(null, EditLineStyle(), null);
				layout.EndVertical();
			}
			Content = layout;
		}

		void UpdateButtons()
		{
			lineStyles.ForEach(r =>
			{
				r.Pressed = (BGICanvas.LineStyle)r.Tag == handler.LineStyle;
			});
		}

		Control EditLineStyle()
		{
			var button = new CustomButton
			{
				Size = new Size(20, 10)
			};
			button.Paint += delegate(object sender, PaintEventArgs pe)
			{
				pe.Graphics.FillPolygon(Colors.Black, new PointF[] { new Point(4, 2), new Point(16, 5), new Point(4, 8) });
			};
			button.Click += delegate
			{
				var dialog = new LineStyleEditor(handler);
				if (dialog.ShowModal(this) == DialogResult.Ok)
				{
					handler.LinePattern = dialog.LinePattern;
					handler.LineStyle = BGICanvas.LineStyle.User;
					UpdateButtons();
					Invalidate();
				}
			};

			return button;
		}

		Control LineSize(int thickness)
		{
			var button = new CustomButton
			{
				Size = new Size(8, 16),
				Persistent = true,
				Pressed = handler.LineThickness == thickness
			};

			button.Paint += delegate(object sender, PaintEventArgs pe)
			{
				var pt = new Point(4, button.Size.Height / 2 - (thickness / 2));
				var size = new Size(button.Size.Width - 8, thickness);
				pe.Graphics.FillRectangle(Colors.Black, new Rectangle(pt, size));
			};

			button.Click += delegate
			{
				handler.LineThickness = thickness;
				lineThicknesses.ForEach(r =>
				{
					if (r != button)
						r.Pressed = false;
				});
				lineStyles.ForEach(r => r.Invalidate());
			};
			lineThicknesses.Add(button);
			return button;
		}

		Control LineStyle(BGICanvas.LineStyle style)
		{
			var button = new CustomButton
			{
				Size = new Size(8, 12),
				Persistent = true,
				Pressed = handler.LineStyle == style,
				Tag = style
			};

			button.Paint += delegate(object sender, PaintEventArgs pe)
			{
				var pattern = (style == BGICanvas.LineStyle.User) ? handler.LinePattern : handler.BGI.GetLinePattern(style);
				var bits = new BitArray(new byte[] { (byte)(pattern >> 8), (byte)(pattern & 0xFF) });
				var pt = new Point(0, button.Size.Height / 2 - (handler.LineThickness / 2));
				var size = new Size(1, handler.LineThickness);
				int i = 0;
				for (pt.X = 4; pt.X < button.Size.Width - 4; pt.X++)
				{
					if (bits[i++ % bits.Length])
						pe.Graphics.FillRectangle(Colors.Black, new Rectangle(pt, size));
				}
			};

			button.Click += delegate
			{
				handler.LineStyle = style;
				lineStyles.ForEach(r =>
				{
					if (r != button)
						r.Pressed = false;
				});
			};
			lineStyles.Add(button);
			return button;
		}
	}
}

