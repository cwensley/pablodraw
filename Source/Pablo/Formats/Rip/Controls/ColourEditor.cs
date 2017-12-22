using System;
using Eto.Forms;
using Pablo.Controls;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Formats.Rip.Controls
{
	public class ColourEditor : Dialog
	{
		RipHandler handler;
		int selectedColor;
		int[] colors;
		Slider redSlider;
		Slider greenSlider;
		Slider blueSlider;
		CustomButton[] colourButtons;
		CustomButton[] egaButtons;
		
		public int SelectedColor {
			get { return selectedColor; }
			set {
				if (selectedColor != value) {
					selectedColor = value;
					SetupSliders ();
				}
			}
		}
		
		public int SelectedEgaColor {
			get { return colors [selectedColor]; }
			set {
				var old = colors [selectedColor];
				if (old != value) {
					colors [selectedColor] = value;
					colourButtons [selectedColor].Invalidate ();
					SetupSliders ();
				}
			}
		}
		
		Palette Palette {
			get { return handler.BGI.Palette; }
		}
		
		public ColourEditor (RipHandler handler)
		{
			this.handler = handler;
			this.Title = "Color Editor";
			
			colors = handler.BGI.GetPalette ();
			
			var layout = new DynamicLayout ();
			
			layout.BeginVertical ();
			layout.BeginHorizontal ();
			layout.Add (Colours ());
			
			//layout.Add (null, true);
			layout.Add (Sliders (), true);
			//layout.Add (null, true);
			layout.Add (EGAColours ());
			
			layout.EndHorizontal ();
			layout.EndVertical ();
			
			layout.BeginVertical ();
			layout.BeginHorizontal ();
			
			layout.Add (SetDefaultButton ());
			layout.Add (null, true);
			layout.Add (CancelButton ());
			layout.Add (OkButton ());
			
			layout.EndHorizontal ();
			layout.EndVertical ();

			Content = layout;
			SetupSliders ();
		}
		
		void SetupSliders ()
		{
			foreach (var b in colourButtons)
				b.Pressed = (int)b.Tag == selectedColor;
			foreach (var b in egaButtons)
				b.Pressed = (int)b.Tag == colors [selectedColor];

			var val = SelectedEgaColor;
			var red = ((val & 0x20) >> 5) + ((val & 0x04) >> 1);
			var green = ((val & 0x10) >> 4) + (val & 0x02);
			var blue = ((val & 0x08) >> 3) + ((val & 0x01) << 1);
			
			//Console.WriteLine ("Col: {3}, Red: {0}, Green: {1}, Blue: {2}", red, green, blue, val);
			
			redSlider.Value = red;
			greenSlider.Value = green;
			blueSlider.Value = blue;
			
		}
		
		Control EGAColours ()
		{
			var pal = handler.BGI.EGAPalette;
			
			egaButtons = new CustomButton[pal.Count];
			var layout = new TableLayout (8, 8);
			layout.Spacing = Size.Empty;
			
			for (int i = 0; i< pal.Count; i++) {
				var control = ChooseEgaColor (i);
				egaButtons [i] = control;
				layout.Add (control, i / 8, i % 8);
			}
			return layout;
		}
		
		Control Colours ()
		{
			colourButtons = new CustomButton[Palette.Count];
			int half = (Palette.Count + 1) / 2;
			var layout = new TableLayout (2, half);
			layout.Spacing = Size.Empty;
			
			for (int i = 0; i<Palette.Count; i++) {
				var control = ChooseColor (i);
				colourButtons [i] = control;
				layout.Add (control, i / half, i % half);
			}
			
			return layout;
		}
		
		CustomButton ChooseEgaColor (int col)
		{
			var button = new CustomButton{
				Size = new Size (16, 16),
				Persistent = true,
				Pressed = colors [SelectedColor] == col,
				Tag = col
			};
			
			button.Paint += delegate(object sender, PaintEventArgs pe) {
				var rect = new Rectangle (button.Size);
				//rect.Inflate (-1, -1);
				var color = handler.BGI.EGAPalette [col];
				pe.Graphics.FillRectangle (color, rect);
				if (button.Pressed) {
					rect.Inflate (-2, -2);
					color.Invert ();
					pe.Graphics.DrawRectangle (color, rect);
				}
					
			};
			
			button.Click += delegate {
				SelectedEgaColor = col;
			};
			
			return button;
		}
		
		Slider CreateSlider ()
		{
			var slider = new Slider{ 
				MinValue = 0, 
				MaxValue = 3, 
				Orientation = SliderOrientation.Vertical,
				TickFrequency = 1
			};
			slider.ValueChanged += HandleSliderValueChanged;
			return slider;
		}

		void HandleSliderValueChanged (object sender, EventArgs e)
		{
			var red = redSlider.Value;
			var green = greenSlider.Value;
			var blue = blueSlider.Value;
			
			var val = ((red & 0x01) << 5) + ((red & 0x02) << 1);
			val += ((green & 0x01) << 4) + (green & 0x02);
			val += ((blue & 0x01) << 3) + ((blue & 0x02) >> 1);
			SelectedEgaColor = val;
		}
		
		Control Sliders ()
		{
			var layout = new TableLayout (5, 2);
			
			layout.SetColumnScale (0);
			layout.SetColumnScale (4);
			layout.Add (new Label{ Text = "Red" }, 1, 0);
			layout.Add (redSlider = CreateSlider (), 1, 1);
			layout.Add (new Label{ Text = "Green" }, 2, 0);
			layout.Add (greenSlider = CreateSlider (), 2, 1);
			layout.Add (new Label{ Text = "Blue" }, 3, 0);
			layout.Add (blueSlider = CreateSlider (), 3, 1);
			
			return layout;
		}
		
		CustomButton ChooseColor (int col)
		{
			var button = new CustomButton{
				Size = new Size (16, 16),
				Persistent = true,
				Pressed = col == SelectedColor,
				Tag = col
			};
			
			button.Paint += delegate(object sender, PaintEventArgs pe) {
				var rect = new Rectangle (button.Size);
				rect.Inflate (-1, -1);
				pe.Graphics.FillRectangle (handler.BGI.EGAPalette [colors [col]], rect);
			};
			
			button.Click += delegate {
				SelectedColor = col;
			};
			
			return button;
		}
		
		Control CancelButton ()
		{
			var control = new Button{ 
				Text = "Cancel" 
			};
			
			control.Click += delegate {
				DialogResult = DialogResult.Cancel;
				Close ();
			};
			base.AbortButton = control;
			
			return control;
		}
		
		Control OkButton ()
		{
			var control = new Button{ 
				Text = "Ok" 
			};
			
			control.Click += delegate {
				DialogResult = DialogResult.Ok;
				
				var command = handler.RipDocument.Create<Commands.SetPalette>();
				command.Palette = this.colors;
				handler.AddCommand (command);
				
				Close ();
			};
			
			base.DefaultButton = control;
			
			return control;
		}
		
		Control SetDefaultButton ()
		{
			var control = new Button {
				Text = "Set Defaults"
			};
			
			control.Click += delegate {
				Palette.EGAColors.CopyTo (colors, 0);
				SetupSliders ();
				Invalidate ();
			};
			return control;
		}
		

	}
}

