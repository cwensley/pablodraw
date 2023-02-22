using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.BGI;
using Pablo.Controls;
using System.Collections;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Controls
{
	public class FillStylePad : Panel
	{
		RipHandler handler;
		List<CustomButton> buttons = new List<CustomButton> ();
		
		public FillStylePad (RipHandler handler)
		{
			this.handler = handler;
			var layout = new DynamicLayout { Padding = Padding.Empty };
			
			layout.BeginVertical (Padding.Empty, Size.Empty);
			
			layout.BeginHorizontal();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.Empty));
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.Solid));
			layout.Add (null, true);

			layout.EndBeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.Line));
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.Interleave));
			
			layout.EndBeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.LtSlash));
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.Slash));
			
			layout.EndBeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.LtBkSlash));
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.BkSlash));
			
			layout.EndBeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.Hatch));
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.XHatch));
			
			layout.EndBeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.WideDot));
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.CloseDot));
			
			layout.EndBeginHorizontal ();
			layout.Add (null, true);
			layout.Add (CreateFillStyle (BGICanvas.FillStyle.User));
			layout.EndHorizontal ();
			
			layout.EndVertical ();
			layout.AddCentered (FillStyleButton (), horizontalCenter: true, verticalCenter: false);

			Content = layout;
		}
		
		void UpdateButtons()
		{
			foreach (var b in buttons)
				b.Pressed = (BGICanvas.FillStyle)b.Tag == handler.FillStyle;
		}
		
		Control FillStyleButton ()
		{
			var button = new CustomButton{
				Size = new Size (20, 12)
			};
			button.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (SystemColors.ControlText, new PointF[] { new Point (4, 2), new Point (16, 5), new Point (4, 8)});
			};
			button.Click += delegate {
				var dialog = new FillStyleEditor (handler);
				if (dialog.ShowModal (this) == DialogResult.Ok) {
					handler.FillPattern = dialog.FillPattern;
					handler.FillStyle = BGICanvas.FillStyle.User;
					UpdateButtons ();
					Invalidate ();
				}
			};
			
			return button;
		}
		
		
		Control CreateFillStyle (BGICanvas.FillStyle fillStyle)
		{
			var button = new CustomButton{
				Size = new Size (20, 12),
				Persistent = true,
				Pressed = fillStyle == handler.FillStyle,
				Tag = fillStyle
			};
			
			button.Paint += delegate(object sender, PaintEventArgs pe) {
				var bytes = fillStyle == BGICanvas.FillStyle.User ? handler.FillPattern : handler.BGI.GetFillPattern (fillStyle);
				if (bytes.Length > 0) {
					for (int y = 2; y<button.Size.Height-2; y++) {
						var bits = new BitArray (new byte[] { bytes [(y - 2) % bytes.Length] });
						if (bits.Length > 0) {
							for (int x = 2; x < button.Size.Width - 2; x++) {
								if (bits [bits.Length - ((x - 2) % bits.Length) - 1]) {
									pe.Graphics.FillRectangle (SystemColors.ControlText, new Rectangle (x, y, 1, 1));
								}
							}
						}
					}
				}
			};
			
			button.Click += delegate {
				handler.FillStyle = fillStyle;
				buttons.ForEach (r => {
					if (r != button)
						r.Pressed = false;
				});
			};
			
			buttons.Add (button);
			return button;
		}
	}
}

