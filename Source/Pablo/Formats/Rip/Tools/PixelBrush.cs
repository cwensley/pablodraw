using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Rip.Tools
{
	public class PixelBrush : RipTool
	{
		Random rnd = new Random ();
		bool enableMulti = true;
		int width = 10;
		int points = 5;
		bool useForeground = true;
		const int MAX_POINTS = 99;
		const int MAX_WIDTH = 99;
		
		public override string Description {
			get { return "Draws a scattered set of pixels (B)"; }
		}

		public override Key Accelerator {
			get {
				return Key.B;
			}
		}
		
		bool EnableMulti (Key modifiers)
		{
			return (modifiers == Key.Shift) ^ enableMulti;
		}

		bool EnableForegroundBackground (Key modifiers, bool useForeground)
		{
			return (modifiers == Key.Alt) ^ useForeground;
		}
		
		public override Eto.Drawing.Image Image {
			get { return Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Brush.png"); }
		}
		
		class BackColor : Commands.Color
		{
			public override void Set (RipHandler handler, bool forDrawing)
			{
				this.Value = handler.Background;
			}
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				if (useForeground)
					yield return Document.Create<Commands.Color>();
				else
					yield return Document.Create<BackColor, BackColor.Type>();
			}
		}
		
		public override void Unselected ()
		{
		}
		
		void DrawBrush (Point location)
		{
			ApplyStyles ();
			var drawUpdates = new List<Rectangle> ();
			for (int i=0; i<points; i++) {
				var x = rnd.Next (width) - width / 2;
				var y = rnd.Next (width) - width / 2;
				var pixel = RipCommands.Create<Commands.Pixel> (Document);
				pixel.Point = new Point (location.X + x, location.Y + y);
				pixel.SetUndoPoint (i == 0);

				Handler.AddCommand (pixel, drawUpdates);
			}
			Handler.FlushCommands (drawUpdates);
		}
		
		public override void OnMouseDown (MouseEventArgs e)
		{
			
			switch (e.Buttons) {
			case MouseButtons.Primary:
				useForeground = EnableForegroundBackground (e.Modifiers, true);
				DrawBrush ((Point)e.Location);
				e.Handled = true;
				break;
			case MouseButtons.Alternate:
				useForeground = EnableForegroundBackground (e.Modifiers, false);
				DrawBrush ((Point)e.Location);
				e.Handled = true;
				break;
			default:
				base.OnMouseDown (e);
				break;
			}
		}

		public override void OnMouseMove (MouseEventArgs e)
		{
			switch (e.Buttons) {
			case MouseButtons.Primary:
				if (EnableMulti (e.Modifiers)) {
					useForeground = EnableForegroundBackground (e.Modifiers, true);
					DrawBrush ((Point)e.Location);
					e.Handled = true;
				}
				break;
			case MouseButtons.Alternate:
				if (EnableMulti (e.Modifiers)) {
					useForeground = EnableForegroundBackground (e.Modifiers, false);
					DrawBrush ((Point)e.Location);
					e.Handled = true;
				}
				break;
			default:
				base.OnMouseMove (e);
				break;
			}				
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);

			layout.Add(Separator());
			layout.BeginVertical (Padding.Empty);
			layout.BeginHorizontal ();
			
			var b = new ImageButton{
				Image = Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Brush-Multi.png"),
				Toggle = true,
				Pressed = enableMulti
			};
			b.Click += delegate {
				enableMulti = b.Pressed;
			};
			
			layout.Add (b);
			layout.Add (null);
			layout.EndHorizontal ();
			layout.EndVertical ();
			
			var font = new Font (SystemFont.Default, 7);
			var updownfont = new Font (SystemFont.Default, 8);

			layout.BeginVertical(Padding.Empty, Size.Empty);
			layout.Add(new Label { Text = "Width", Font = font, HorizontalAlign = HorizontalAlign.Center });
			var widthBox = new NumericUpDown{
				Font = updownfont,
				Value = width,
				MinValue = 1,
				MaxValue = MAX_WIDTH,
				Size = new Size(20, -1)
			};
			widthBox.ValueChanged += delegate {
				width = Math.Max (1, Math.Min (MAX_WIDTH, (int)widthBox.Value));
			};
			layout.Add (widthBox);
			layout.EndBeginVertical(Padding.Empty, Size.Empty);

			layout.Add (new Label{ Text = "Density", Font = font, HorizontalAlign = HorizontalAlign.Center});
			var pointsBox = new NumericUpDown{
				Font = updownfont,
				Value = points,
				MinValue = 1,
				MaxValue = MAX_POINTS,
				Size = new Size(20, -1)
			};
			pointsBox.ValueChanged += delegate {
				points = Math.Max (1, Math.Min (MAX_POINTS, (int)pointsBox.Value));
			};
			layout.Add (pointsBox);
			layout.EndVertical();
			
			return layout;
		}
		
		public override void RemoveDrawing (IList<Rectangle> updates)
		{
		}
		
		public override void ApplyDrawing (IList<Rectangle> updates)
		{
		}
		
	}
}

