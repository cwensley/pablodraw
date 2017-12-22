using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Controls;

namespace Pablo.Formats.Character.Controls
{
	public class ColourEditor : Dialog
	{
		readonly CharacterHandler handler;
		Scrollable coloursHolder;
		CustomButton removeButton;
		readonly Font font = Fonts.Sans(12, FontStyle.Bold);

		Palette Palette { get; set; }

		int SelectedIndex { get; set; }

		class SelectColourBox : ColourPad.ColourBox
		{
			public int Index { get; set; }

			public ColourEditor Editor { get; set; }

			public override Color Color
			{
				get
				{
					return Editor.Palette[Index];
				}
				set
				{
					Editor.handler.Undo.Save(new Undo.UndoColour { Color = Editor.Palette[Index], Index = Index });
					Editor.Palette[Index] = value;
					Invalidate();
				}
			}

			public override void OnPaint(PaintEventArgs pe)
			{
				base.OnPaint(pe);
				if (HasFocus)
				{
					var col = Color;
					col.Invert();
					var rect = new Rectangle(Size);
					rect.Inflate(-2, -2);
					pe.Graphics.DrawRectangle(col, rect);
				}
			}

			public SelectColourBox()
			{
				Size = new Size(16, 16);
				CanFocus = true;
			}

			public override void OnGotFocus(EventArgs e)
			{
				base.OnGotFocus(e);
				Invalidate();
			}

			public override void OnLostFocus(EventArgs e)
			{
				base.OnLostFocus(e);
				Invalidate();
			}

			public override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
				Editor.SelectedIndex = Index;
			}

			public override void OnMouseDoubleClick(MouseEventArgs e)
			{
				base.OnMouseDoubleClick(e);

				e.Handled = true;
				var col = new ColorDialog { Color = Color };
				col.ColorChanged += delegate
				{
					Color = col.Color;
				};
				col.ShowDialog(ParentWindow);
			}
		}

		public ColourEditor(CharacterHandler handler)
		{
			this.handler = handler;
			Title = "Palette Editor";
#if DESKTOP
			Resizable = true;
#endif
			this.Palette = handler.CurrentPage.Palette.Clone();
			var layout = new DynamicLayout();
			
			layout.Add(new Label
			{ 
				Text = "Double click a colour to change it. Note that most formats only support up to 16 colours, except for Tundra and Ansi format",
				Wrap = WrapMode.Word,
				HorizontalAlign = HorizontalAlign.Center,
				VerticalAlign = VerticalAlign.Middle,
				Size = new Size(50, 50)
			});
			
			layout.BeginVertical(true, true);
			layout.BeginHorizontal();
			layout.Add(ColoursHolder(), true);
			layout.Add(ColourButtons());
			layout.EndHorizontal();
			layout.EndVertical();
			
			layout.BeginVertical();
			layout.AddRow(SetDefaultButton(), null, CancelButton(), OkButton());
			layout.EndVertical();

			Content = layout;
			
			UpdateColours();
		}

		Control ColoursHolder()
		{
			coloursHolder = new Scrollable
			{ 
				Size = new Size(16 * 18 + 14 + 16, 4 * 18 + 14),
				ExpandContentWidth = false,
				ExpandContentHeight = false
			};
			return coloursHolder;
		}

		Control ColourButtons()
		{
			var layout = new PixelLayout { Size = new Size(17, 17 * 2 + 4) };
			
			layout.Add(AddButton(), 0, 0);
			layout.Add(RemoveButton(), 0, 17 + 4);
			
			return layout;
		}

		Control AddButton()
		{
			var control = new CustomButton { Size = new Size(17, 17) };
			control.Paint += delegate(object sender, PaintEventArgs pe)
			{
				var size = pe.Graphics.MeasureString(font, "+");
				pe.Graphics.DrawText(font, control.DrawColor, (int)(control.Size.Width - size.Width) / 2, (int)(control.Size.Height - size.Height - 1) / 2, "+");
			};
			
			control.Click += delegate
			{
				Palette.Add(Colors.White);
				SelectedIndex = Palette.Count - 1;
				UpdateColours();
			};
			return control;
		}

		Control RemoveButton()
		{
			var control = removeButton = new CustomButton { Size = new Size(17, 17) };
			control.Paint += delegate(object sender, PaintEventArgs pe)
			{
				var size = pe.Graphics.MeasureString(font, "-");
				pe.Graphics.DrawText(font, control.DrawColor, (int)(control.Size.Width - size.Width) / 2, (int)(control.Size.Height - size.Height - 1) / 2, "-");
			};
			
			control.Click += delegate
			{
				if (SelectedIndex >= 0 && Palette.Count > 1)
				{
					Palette.RemoveAt(SelectedIndex);
					if (SelectedIndex >= Palette.Count)
						SelectedIndex = Palette.Count - 1;
					UpdateColours();
				}
			};
			return control;
		}

		void UpdateColours()
		{
			var size = new Size(16, ((Palette.Count + 15) / 16));
			var layout = new TableLayout(size);
			layout.Spacing = new Size(2, 2);
			
			int count = 0;
			for (int y = 0; y < size.Height; y++)
			{
				for (int x = 0; x < size.Width; x++)
				{
					if (count < Palette.Count)
					{
						var sel = new SelectColourBox { Index = count++, Editor = this };
						if (SelectedIndex == sel.Index)
							sel.Load += delegate
							{
								sel.Focus();	
							};
						layout.Add(sel, x, y);
					}
					else
						layout.Add(new Panel { Size = new Size(16, 16) }, x, y);
				}
			}
			
			coloursHolder.Content = layout;
			
			removeButton.Enabled = Palette.Count > 1;
		}

		Control CancelButton()
		{
			var control = new Button { Text = "Cancel" };
			
			control.Click += delegate
			{
				DialogResult = DialogResult.Cancel;
				Close();
			};
			AbortButton = control;
			
			return control;
		}

		Control OkButton()
		{
			var control = new Button { Text = "Ok" };
			
			control.Click += delegate
			{
				DialogResult = DialogResult.Ok;
				if (handler.Client != null)
				{
					handler.Client.SendCommand(new Actions.SetPalette(handler) { Palette = Palette });
				}
				else
				{
					handler.CurrentPage.Palette = Palette;
					handler.Document.IsModified = true;
				}
				Close();
			};
			
			DefaultButton = control;
			
			return control;
		}

		Control SetDefaultButton()
		{
			var control = new Button { Text = "Set Defaults" };
			
			control.Click += delegate
			{
				Palette = Palette.GetDosPalette();
				UpdateColours();
				Invalidate();
			};
			return control;
		}
	}
}

