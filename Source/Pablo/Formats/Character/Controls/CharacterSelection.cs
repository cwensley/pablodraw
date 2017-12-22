using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Formats.Character.Controls
{
	public class CharacterSelection : Dialog<DialogResult>
	{
		Drawable preview;
		Bitmap previewImage;
		Label charNum;
		BitFont font;
		FontTextBox characters;
		int magnification = 2;
		Character selectedCharacter;

		public CharacterHandler CharacterHandler
		{
			get;
			private set;
		}

		public Character SelectedCharacter
		{
			get { return selectedCharacter; }
			set
			{
				if (selectedCharacter != value)
				{
					selectedCharacter = value;
					int y = selectedCharacter / characters.TextSize.Width;
					int x = selectedCharacter % characters.TextSize.Width;
					characters.TextCursor = new Point(x, y);
				}
			}
			
		}

		public CharacterSelection(CharacterHandler handler)
		{
			this.Title = "Select Character";
			this.CharacterHandler = handler;
			font = CharacterHandler.CurrentPage.Font;
			
			var layout = new DynamicLayout();
			
			layout.AddCentered(Preview(), verticalCenter: false);
			layout.AddCentered(CharNum(), verticalCenter: false);
			layout.Add(Characters());
			layout.BeginVertical(Padding.Empty);
			layout.AddRow(null, CancelButton(), OkButton());
			layout.EndVertical();

			Content = layout;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Update();
			characters.Focus();
		}

		void Update()
		{
			GenerateImage();
			preview.Invalidate();
			charNum.Text = string.Format("Num: {0}  Hex: {0:X}", (int)SelectedCharacter);
		}

		void GenerateImage()
		{
			using (var bd = previewImage.Lock ())
			{

				unsafe
				{
					var page = CharacterHandler.CurrentPage;
					var pal = page.Palette;

					var fg = pal[CharacterHandler.DrawAttribute.Foreground].ToArgb();
					var bg = pal[CharacterHandler.DrawAttribute.Background].ToArgb();
					int onColor = bd.TranslateArgbToData(fg);
					int offColor = bd.TranslateArgbToData(bg);

					byte* scanline = (byte*)bd.Data;
					var character = font[SelectedCharacter];
					for (int y = 0; y < font.Size.Height * magnification; y++)
					{
						int* row = (int*)scanline;
						for (int x = 0; x < font.Size.Width * magnification; x++)
						{
							var bit = character[x / magnification, y / magnification];
							if (bit)
								*row = onColor;
							else
								*row = offColor;
							row++;
						}
						scanline += bd.ScanWidth;
					}

				}
			}
		}

		Control CharNum()
		{
			var control = charNum = new Label
			{
			};
			return control;
		}

		Control Preview()
		{
			var control = preview = new Drawable
			{
				Size = font.Size * magnification
			};
			
			previewImage = new Bitmap(font.Size * magnification, PixelFormat.Format32bppRgb);
			control.Paint += (sender, pe) => pe.Graphics.DrawImage(previewImage, 0, 0);

			var panel = new Panel { Content = control, Padding = new Padding(10) };
			var col = CharacterHandler.DrawAttribute.Background;
			var pal = CharacterHandler.CurrentPage.Palette;
			panel.BackgroundColor = pal[col];
			
			return panel;
		}

		Control Characters()
		{
			var font = CharacterHandler.CurrentPage.Font;
			int width = 64; //font.NumChars > 256 ? 64 : 32;
			var control = characters = new FontTextBox(CharacterHandler, new Size(width, (font.NumChars + width - 1) / width));
			control.ReadOnly = true;
			control.CanFocus = true;
			control.Bordered = true;
			control.SetAttribute(CharacterHandler.DrawAttribute);
			control.KeyDown += (sender, e) =>
			{
				if (e.KeyData == Keys.Enter)
				{
					Result = DialogResult.Ok;
					Close();
					e.Handled = true;
				}
			};
			control.CursorElementChanged += delegate
			{
				this.SelectedCharacter = control.CursorElement.Character;
				Update();
			};
			control.MouseDown += delegate(object sender, MouseEventArgs e)
			{
				e.Handled = true;
				this.SelectedCharacter = control.CursorElement.Character;
				Update();
			};
			control.MouseDoubleClick += delegate(object sender, MouseEventArgs e)
			{
				e.Handled = true;
				this.SelectedCharacter = control.CursorElement.Character;
				Result = DialogResult.Ok;
				Close();
			};
			int character = 0;
			for (int y=0; y<control.Canvas.Height; y++)
			{
				for (int x = 0; x<control.Canvas.Width; x++)
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
			AbortButton = control;
			
			return control;
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
			
			DefaultButton = control;
			
			return control;
		}
	}
}

