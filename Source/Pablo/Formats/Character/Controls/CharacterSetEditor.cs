using System;
using Eto.Forms;
using Eto.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace Pablo.Formats.Character.Controls
{
	public class CharacterSetEditor : Dialog<DialogResult>
	{
		FontTextBox lastSet;
		int selected;
		List<Canvas> sets = new List<Canvas>();

		public CharacterHandler CharacterHandler { get; private set; }

		public CharacterSetEditor(CharacterHandler handler)
		{
			this.CharacterHandler = handler;
			this.Title = "Character Set Editor";

			var layout = new DynamicLayout();

			selected = CharacterHandler.CharacterSet;

			layout.AddCentered(CharacterSelection(), xscale: true, yscale: true);

			layout.Add(new Label { Text = "Click a character above, then select the character below to change it", Size = new Size(100, 32), HorizontalAlign = HorizontalAlign.Center });

			layout.Add(Characters());

			layout.Add(Buttons());

			Content = layout;
		}

		Control Characters()
		{
			var font = CharacterHandler.CurrentPage.Font;
			int width = 64; //font.NumChars > 256 ? 64 : 32;
			var size = new Size(width, (font.NumChars + width - 1) / width);
			var control = new FontTextBox(CharacterHandler, size);
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
				e.Handled = true;
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

		Control CharacterSelection()
		{
			var layout = new DynamicLayout();
			layout.Spacing = new Size(10, 5);

			RadioButton master = null;
			for (int i = 0; i < CharacterDocumentInfo.MAX_CHARACTER_SETS; i++)
			{
				var characterSet = CharacterHandler.Info.GetCharacterSet(i).ToArray();
				var control = new FontTextBox(CharacterHandler, new Size(characterSet.Length, 1));
				control.ReadOnly = false;
				control.SetAttribute(CharacterHandler.DrawAttribute);
				sets.Add(control.Canvas);
				for (int c = 0; c < characterSet.Length; c++)
				{
					var ce = control.Canvas[c, 0];
					ce.Character = characterSet[c];
					control.Canvas[c, 0] = ce;
				}
				control.GotFocus += delegate
				{
					lastSet = control;
				};
				var radio = new RadioButton(master) { Text = "Set " + (i + 1), Tag = i };
				if (i == CharacterHandler.CharacterSet)
				{
					radio.Checked = true;
					radio.Focus();
				}
				radio.CheckedChanged += delegate(object sender, EventArgs e)
				{
					var c = (RadioButton)sender;
					selected = (int)c.Tag;
				};
				radio.MouseDoubleClick += (sender, e) =>
				{
					e.Handled = true;
					Save();
					Result = DialogResult.Ok;
					Close();
				};
				if (master == null)
					master = radio;
				if (i % 2 == 0)
					layout.BeginHorizontal();
				layout.Add(radio);
				layout.Add(control);
				if (i % 2 == 1)
					layout.EndHorizontal();
			}

			return new GroupBox { Content = layout };
		}

		Control Buttons()
		{
			var layout = new DynamicLayout();
			layout.Padding = Padding.Empty;
			layout.AddRow(SetDefaultButton(), null, CancelButton(), OkButton());

			return layout;
		}

		void Save()
		{
			CharacterHandler.CharacterSet = selected;
			for (int i = 0; i < sets.Count; i++)
			{
				var characters = from r in sets[i].GetLine(0) select (int)r.Character;
				CharacterHandler.Info.SetCharacterSet(i, characters.ToArray());
			}
		}

		Control SetDefaultButton()
		{
			var control = new Button
			{
				Text = "Set Defaults"
			};
			control.Click += delegate
			{
				var result = MessageBox.Show(this, "Are you sure you wish to reset all character sets to default?", MessageBoxButtons.YesNo);
				if (result == DialogResult.Yes)
				{
					for (int i = 0; i < sets.Count; i++)
					{
						var charset = sets[i];
						for (int x = 0; x < charset.Width; x++)
						{
							var ce = charset[x, 0];
							ce.Character = CharacterDocumentInfo.DefaultCharacterSets[i, x];
							charset[x, 0] = ce;
						}
					}
				}
			};
			return control;
		}

		Control OkButton()
		{
			var control = new Button
			{
				Text = "OK"
			};
			control.Click += delegate
			{
				Save();
				this.CharacterHandler.TriggerCharacterSetChanged();
				Result = DialogResult.Ok;
				Close();
			};
			base.DefaultButton = control;
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

	}
}

