using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.BGI;
using System.Linq;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Rip.Controls
{
	public class FontTypeSelector : Dialog<DialogResult>
	{
		RipHandler handler;
		BGICanvas canvas;
		ListBox fontList;
		int fontSize;
		List<CustomButton> sizeButtons = new List<CustomButton>();
		Font font = new Font(SystemFont.Default, 8);

		public int FontSize
		{
			get { return fontSize; }
			set
			{
				if (fontSize != value)
				{
					fontSize = Math.Min(10, Math.Max(1, value));
					UpdateSizeButtons();
					DrawPreview();
				}
			}
		}

		public BGICanvas.FontType SelectedFont
		{
			get
			{
				BGICanvas.FontType result;
				if (!Enum.TryParse<BGICanvas.FontType>(fontList.SelectedKey, out result))
					result = BGICanvas.FontType.Default;
				return result;
			}
			set
			{
				fontList.SelectedKey = value.ToString();
			}
		}

		public FontTypeSelector(RipHandler handler)
		{
			this.handler = handler;
			this.Title = "Font Style";
#if DESKTOP
			this.Resizable = true;
#endif

			var layout = new DynamicLayout();

			layout.Add(FontAndSizes(), true, true);

			layout.AddCentered(Preview(), horizontalCenter: true, verticalCenter: false, padding: Padding.Empty);

			layout.BeginVertical(Padding.Empty);
			layout.BeginHorizontal();
			layout.Add(null, true);
			layout.Add(CancelButton());
			layout.Add(OkButton());

			layout.EndHorizontal();
			layout.EndVertical();

			Content = layout;
		}

		void DrawPreview()
		{
			if (canvas == null)
				return;
			var updates = new List<Rectangle>();
			canvas.GraphDefaults(updates);
			canvas.SetTextStyle(SelectedFont, handler.TextDirection, fontSize);
			var font = canvas.LoadFont(SelectedFont);
			var str = "The quick brown Fox Jumps over the lazy dog";
			var size = font.GetRealTextSize(str, handler.TextDirection, fontSize);
			size = (canvas.WindowSize - size) / 2;
			size.Width = Math.Max(size.Width, 0);
			canvas.OutTextXY(size.Width, size.Height, str, updates);

			canvas.Control.Invalidate();
		}

		ListItem CreateItem(BGICanvas.FontType fontType)
		{
			return new ListItem
			{
				Key = fontType.ToString(),
				Text = fontType.ToString()
			};
		}

		void UpdateSizeButtons()
		{
			foreach (var b in sizeButtons)
			{
				b.Pressed = (int)b.Tag == fontSize;
			}
		}

		Control ChangeSizes()
		{
			var layout = new TableLayout(2, 7);

			layout.SetRowScale(0);
			layout.SetRowScale(6);
			for (int i = 1; i <= 10; i++)
			{
				var x = (i - 1) / 5;
				var y = (i - 1) % 5;
				layout.Add(ChangeSize(i), x, y + 1);
			}

			return layout;
		}

		Control ChangeSize(int size)
		{
			var control = new CustomButton
			{
				Size = new Size(16, 16),
				Persistent = true,
				Tag = size,
				Pressed = fontSize == size
			};

			control.Click += delegate
			{
				this.FontSize = size;
			};

			control.Paint += delegate(object sender, PaintEventArgs pe)
			{
				var textSize = Size.Round(pe.Graphics.MeasureString(font, size.ToString()));
				textSize = (control.Size - textSize) / 2;
				pe.Graphics.DrawText(font, Colors.Black, textSize.Width, textSize.Height, size.ToString());
			};

			sizeButtons.Add(control);

			return control;
		}

		Control FontList()
		{
			var control = fontList = new ListBox
			{
				Size = new Size(150, 150)
			};

			var items = from r in Enum.GetValues(typeof(BGICanvas.FontType)).Cast<BGICanvas.FontType>()
			            where r != BGICanvas.FontType.User
			            select CreateItem((BGICanvas.FontType)Enum.ToObject(typeof(BGICanvas.FontType), r));

			control.Items.AddRange(items.OrderBy(r => r.Text).Cast<IListItem>());

			control.Activated += delegate
			{
				Result = DialogResult.Ok;
				Close();
			};

			control.SelectedIndexChanged += delegate
			{
				DrawPreview();
			};

			return control;
		}

		Control FontAndSizes()
		{
			var layout = new DynamicLayout { Padding = new Padding(5) };
			layout.BeginHorizontal();

			layout.Add(null, true);
			layout.Add(new GroupBox { Text = "Font", Content = FontList(), Padding = new Padding(5) });
			layout.Add(new GroupBox { Text = "Size", Content = ChangeSizes(), Padding = new Padding(5) });
			layout.Add(null, true);

			layout.EndHorizontal();
			return layout;
		}

		Control Preview()
		{
			var control = new Drawable
			{
				Size = new Size(400, 100)
			};

			control.Paint += delegate(object sender, PaintEventArgs pe)
			{
				if (canvas != null)
					canvas.DrawRegion(pe.Graphics, new Rectangle(canvas.WindowSize));
			};

			control.LoadComplete += delegate
			{
				canvas = new BGICanvas(control, control.Size);
				DrawPreview();
			};

			return control;
		}

		Control CancelButton()
		{
			var control = new Button { Text = "Cancel" };

			control.Click += delegate
			{
				Result = DialogResult.Cancel;
				Close();
			};
			base.AbortButton = control;

			return control;
		}

		Control OkButton()
		{
			var control = new Button { Text = "Ok" };

			control.Click += delegate
			{
				Result = DialogResult.Ok;
				Close();
			};

			base.DefaultButton = control;

			return control;
		}

	}
}

