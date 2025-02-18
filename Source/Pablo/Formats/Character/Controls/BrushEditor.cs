using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Formats.Character.Controls
{
	public class BrushEditor : Dialog<DialogResult>
	{
		FontTextBox lastSet;
		readonly List<Canvas> brushCanvases = new List<Canvas>();

		public CharacterHandler CharacterHandler
		{
			get;
			private set;
		}

		public int SelectedBrush
		{
			get;
			set;
		}


		public BrushEditor(CharacterHandler handler)
		{
			this.Title = "Brush Editor";
			this.CharacterHandler = handler;
		}

		protected override void OnPreLoad(EventArgs e)
		{
			base.OnPreLoad(e);

			var layout = new DynamicLayout();
			layout.Padding = 6;
			layout.Spacing = new Size(4, 4);

			layout.Add(Brushes());
			layout.Add(Characters());
			layout.BeginVertical(Padding.Empty);
			layout.AddRow(ResetButton(), null, CancelButton(), OkButton());

			Content = layout;
			LoadBrushes(CharacterHandler.Info.Brushes);
		}

		void SaveBrushes()
		{
			for (int i = 0; i < CharacterHandler.Info.Brushes.Length; i++)
			{
				var canvas = brushCanvases[i];
				var endx = canvas.FindEndX(0, CanvasElement.Default);
				var brush = (endx == -1) ? null : new BrushInfo(canvas.GetElements(Point.Empty, endx + 1).Select(r => r.Character));
				CharacterHandler.Info.Brushes[i] = brush;
			}
		}
		
		void LoadBrushes(BrushInfo[] brushes)
		{
			for (int i = 0; i < brushCanvases.Count; i++)
			{
				var canvas = brushCanvases[i];
				var brush = (i < brushes.Length) ? brushes[i] : new BrushInfo();
				if (brush != null)
				{
					var characters = brush.GetCharacters(CharacterHandler.CurrentPage.Font.Encoding) ?? new Character[0];
					for (int c = 0; c < characters.Length; c++)
					{
						var ce = canvas[c, 0];
						ce.Character = characters[c];
						canvas[c, 0] = ce;
					}
				}
			}
		}

		Control Brushes()
		{
			var layout = new TableLayout(6, 5);
			layout.Spacing = new Size(10, 5);
			layout.SetColumnScale(0);
			layout.SetColumnScale(5);

			RadioButton master = null;
			for (int i = 0; i < CharacterDocumentInfo.MAX_BRUSHES; i++)
			{
				var control = new FontTextBox(CharacterHandler, new Size(CharacterDocumentInfo.MAX_BRUSH_SIZE, 1));
				control.ReadOnly = false;
				control.SetAttribute(CharacterHandler.DrawAttribute);
				brushCanvases.Add(control.Canvas);

				control.GotFocus += delegate
				{
					lastSet = control;
				};
				var x = (i / 5) * 2;
				var y = i % 5;
				var radio = new RadioButton(master) { Text = "Brush " + (i + 1), Tag = i };
				if (i == SelectedBrush)
				{
					radio.Checked = true;
					radio.Focus();
				}
				radio.MouseDoubleClick += (sender, e) =>
				{
					e.Handled = true;
					var c = (RadioButton)sender;
					SelectedBrush = (int)c.Tag;
					Result = DialogResult.Ok;
					SaveBrushes();
					Close();
				};
				radio.CheckedChanged += delegate(object sender, EventArgs e)
				{
					var c = (RadioButton)sender;
					SelectedBrush = (int)c.Tag;
				};
				if (master == null)
					master = radio;
				layout.Add(radio, x + 1, y);
				layout.Add(control, x + 2, y);
			}

			/**
			var scroll = new Scrollable{  };
			var pl = new PixelLayout(scroll);
			pl.Add(layout.Container, 0, 0);
			return scroll;
			/**/
			return new GroupBox { Content = layout, Padding = 6 };
			/**/
		}

		Control Characters()
		{
			var font = CharacterHandler.CurrentPage.Font;
			const int width = 64; //font.NumChars > 256 ? 64 : 32;
			var control = new FontTextBox(CharacterHandler, new Size(width, (font.NumChars + width - 1) / width));
			control.Bordered = true;
			control.SetAttribute(CharacterHandler.DrawAttribute);
			control.MouseDown += (sender, e) =>
			{
				if (lastSet != null)
				{
					lastSet.Insert(control.CursorElement);
					lastSet.Invalidate();
					lastSet.Focus();
				}
			};
			control.ReadOnly = true;
			control.CanFocus = false;
			int character = 0;
			for (int y = 0; y < control.Canvas.Height; y++)
			{
				for (int x = 0; x < control.Canvas.Width; x++)
				{
					if (character < font.NumChars)
					{
						var ce = control.Canvas[x, y];
						ce.Character = character++;
						control.Canvas[x, y] = ce;
					}
				}
			}

			return control;
		}

		Button CancelButton()
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
			AbortButton = control;

			return control;
		}

		Button OkButton()
		{
			var control = new Button
			{
				Text = "Ok"
			};

			control.Click += delegate
			{
				Result = DialogResult.Ok;
				SaveBrushes();
				Close();
			};

			DefaultButton = control;

			return control;
		}

		Button ResetButton()
		{
			var control = new Button
			{
				Text = "Set Defaults"
			};

			control.Click += delegate
			{
				LoadBrushes(CharacterDocumentInfo.DefaultBrushes);
			};

			return control;
		}
	}
}

