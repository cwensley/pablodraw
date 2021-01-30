using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Formats.Character.Controls
{
	public class WidthDialog : Dialog<DialogResult>
	{
		NumericStepper widthControl;
		NumericStepper heightControl;

		Control WidthControl()
		{
			widthControl = new NumericStepper { MinValue = 1, MaxValue = 5000 };
			return widthControl;
		}

		Control HeightControl()
		{
			heightControl = new NumericStepper { MinValue = 1, MaxValue = 10000 };
			return heightControl;
		}

		Control OkButton()
		{
			var control = new Button
			{
				Text = "Ok"
			};
			control.Click += delegate
			{
				Result = DialogResult.Ok;
				Close();
			};
			this.DefaultButton = control;
			return control;
		}

		Control CancelButton()
		{
			var control = new Button
			{
				Text = "Cancel"
			};
			control.Click += delegate
			{
				Result = DialogResult.Cancel;
				Close();
			};
			base.AbortButton = control;
			return control;
		}

		Control Buttons()
		{
			var layout = new TableLayout(3, 1);
			layout.Spacing = new Size(5, 5);

			layout.SetColumnScale(0);

			layout.Add(CancelButton(), 1, 0);
			layout.Add(OkButton(), 2, 0);
			return layout;
		}

		public Size CanvasSize
		{
			get { return new Size((int)widthControl.Value, (int)heightControl.Value); }
			set
			{
				widthControl.Value = value.Width;
				heightControl.Value = value.Height;
			}
		}

		Control WidthRow()
		{
			var layout = new DynamicLayout();
			layout.Padding = new Padding(20, 10, 20, 0);

			layout.BeginHorizontal();
			layout.AddSpace();
			layout.Add(new Label { Text = "Canvas Size", VerticalAlignment = VerticalAlignment.Center });
			layout.Add(WidthControl());
			layout.Add(HeightControl());
			layout.AddSpace();

			layout.EndHorizontal();

			return layout;
		}

		public WidthDialog()
		{
			//this.ClientSize = new Size (350, 160);

			var toplayout = new DynamicLayout();
			toplayout.Padding = new Padding(10);
			toplayout.Spacing = new Size(5, 5);

			toplayout.Add(new Label
			{
				Text = "Set the size of the canvas.\nNote that ANSI and ASCII are usually maximum 80 columns,\nand BIN (Binary) files are usually 160 characters.\nAnything larger that 500 wide may cause PabloDraw to become slow or unresponsive.",
				Wrap = WrapMode.Word,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = TextAlignment.Center
			}, yscale: true);
			toplayout.Add(WidthRow());
			toplayout.Add(Buttons());

			Content = toplayout;
		}
	}
}

