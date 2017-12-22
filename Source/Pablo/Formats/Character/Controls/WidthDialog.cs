using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Formats.Character.Controls
{
	public class WidthDialog : Dialog
	{
		int width;
		TextBox widthControl;
		
		Control WidthControl ()
		{
			widthControl = new TextBox ();
			widthControl.TextChanged += delegate {
				int.TryParse (widthControl.Text, out width);
			};
			return widthControl;
		}
		
		Control OkButton ()
		{
			var control = new Button{
				Text = "Ok"
			};
			control.Click += delegate {
				if (int.TryParse (widthControl.Text, out width)) {
					if (width > 0 && width <= 5000) {
						DialogResult = DialogResult.Ok;
						Close ();
					} else {
						MessageBox.Show(this, "Width must be a numeric value between 1 and 5000");
					}
				}
			};
			this.DefaultButton = control;
			return control;
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
		
		Control Buttons ()
		{
			var layout = new TableLayout (3, 1);
			layout.Padding = Padding.Empty;
			
			layout.SetColumnScale (0);
			
			layout.Add (CancelButton (), 1, 0);
			layout.Add (OkButton (), 2, 0);
			return layout;
		}
		
		public int Width {
			get { return width; }
			set {
				width = value;
				widthControl.Text = width.ToString ();
			}
		}
		
		Control WidthRow ()
		{
			var layout = new TableLayout (2, 2);
			layout.Padding = new Padding(20, 10, 20, 0);
			
			layout.Add (new Label{ Text = "Canvas Width", VerticalAlign = VerticalAlign.Middle }, 0, 0);
			layout.Add (WidthControl (), 1, 0, true, false);
			return layout;
		}
		
		public WidthDialog ()
		{
			//this.ClientSize = new Size (350, 160);
			
			var toplayout = new TableLayout (1, 3);
			toplayout.Padding = new Padding (10);
			
			toplayout.Add (new Label{ 
				Text = "Set the width of the canvas.\nNote that ANSI and ASCII are usually maximum 80 columns,\nand BIN (Binary) files are usually 160 characters.\nAnything larger that 500 wide may cause PabloDraw to become slow or unresponsive.",
				VerticalAlign = VerticalAlign.Middle,
				HorizontalAlign = HorizontalAlign.Center
			}, 0, 0, true, true);
			toplayout.Add (WidthRow (), 0, 1);
			toplayout.Add (Buttons (), 0, 2);

			Content = toplayout;
		}
	}
}

