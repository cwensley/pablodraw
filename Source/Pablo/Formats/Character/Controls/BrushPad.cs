using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Controls;
using Eto;

namespace Pablo.Formats.Character.Controls
{
	public class BrushPad : Panel
	{
		Tools.BrushTool tool;
		FontTextBox preview;
		CustomButton upButton;
		CustomButton downButton;

		new CharacterHandler Handler
		{
			get { return this.tool.Handler; }
		}

		public BrushPad(Tools.BrushTool tool)
		{
			this.tool = tool;
			var page = Handler.CurrentPage;
			page.FontChanged += new EventHandler<EventArgs>(delegate
			{
				Update();
			}).MakeWeak(e => page.FontChanged -= e);
			Handler.DrawAttributeChanged += new EventHandler<EventArgs>(delegate
			{
				Update();
			}).MakeWeak(e => Handler.DrawAttributeChanged -= e);

			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };

			layout.Add(new Label { Text = "Brush", TextAlignment = TextAlignment.Center, Font = new Font(SystemFont.Default, 7) });

			layout.AddCentered(GradientPreview(), Padding.Empty, Size.Empty);

			layout.BeginVertical(new Padding(2, 5), Size.Empty);
			layout.AddRow(UpButton(), null, DownButton());
			layout.EndVertical();
			layout.AddCentered(SelectButton(), Padding.Empty, Size.Empty);

			Content = layout;

			this.Update();
		}

		void Update()
		{
			UpdateButtons();
			UpdatePreview();
		}

		void UpdatePreview()
		{
			var page = Handler.CurrentPage;
			var font = page.Font;
			var brush = this.tool.CurrentBrush;
			var characters = brush?.GetCharacters(font.Encoding) ?? new Character[0];

			var length = Math.Max(CharacterDocumentInfo.MAX_BRUSH_SIZE, characters.Length);
			var width = (38 / font.Width);
			var size = new Size(Math.Min(width, length), (length + width - 1) / width);
			preview.TextSize = size;
			var attr = Handler.DrawAttribute;
			int num = 0;
			for (int y = 0; y < size.Height; y++)
				for (int x = 0; x < size.Width; x++)
				{
					if (num < characters.Length)
						preview.Canvas[x, y] = new CanvasElement(characters[num++], attr);
				}
			/*
			if (this.Loaded) {
				this.Layout.Update ();
				//this.ParentWindow.Layout.Update ();
				//this.ParentLayout.Update ();
			}*/
		}

		void UpdateButtons()
		{
			var brushes = Handler.Info.Brushes;
			var index = this.tool.CurrentBrushIndex;
			upButton.Enabled = index > 0;
			downButton.Enabled = index < brushes.Length - 1;
		}

		Control GradientPreview()
		{
			var control = preview = new FontTextBox(Handler, Size.Empty)
			{
				ReadOnly = true,
				Bordered = true
			};
			control.MouseDown += (sender, e) =>
			{
				EditBrushes();
				e.Handled = true;
			};
			return control;
		}

		void EditBrushes()
		{
			var dlg = new BrushEditor(Handler)
			{
				SelectedBrush = this.tool.CurrentBrushIndex
			};
			var result = dlg.ShowModal(this);
			if (result == DialogResult.Ok)
			{
				this.tool.CurrentBrushIndex = dlg.SelectedBrush;
				this.Update();
			}
		}


		Control SelectButton()
		{
			var button = new CustomButton
			{
				Size = new Size(20, 12),
#if DESKTOP
				ToolTip = "Edit brushes"
#endif
			};
			button.Paint += delegate(object sender, PaintEventArgs pe)
			{
				pe.Graphics.FillPolygon(SystemColors.ControlText, new PointF[] { new Point(4, 2), new Point(16, 5), new Point(4, 8) });
			};

			button.Click += delegate
			{
				EditBrushes();
			};

			return button;
		}

		Control UpButton()
		{
			var control = upButton = new CustomButton { Size = new Size(16, 12) };
			control.Paint += delegate(object sender, PaintEventArgs pe)
			{
				pe.Graphics.FillPolygon(control.DrawColor, new PointF[] { new Point(3, 6), new Point(13, 10), new Point(13, 2) });

			};
			control.Click += delegate
			{
				tool.CurrentBrushIndex--;
				this.Update();
			};
			return control;
		}

		Control DownButton()
		{
			var control = downButton = new CustomButton { Size = new Size(16, 12) };
			control.Paint += delegate(object sender, PaintEventArgs pe)
			{
				pe.Graphics.FillPolygon(control.DrawColor, new PointF[] { new Point(13, 6), new Point(3, 10), new Point(3, 2) });
			};
			control.Click += delegate
			{
				tool.CurrentBrushIndex++;
				this.Update();
			};
			return control;
		}


	}
}

