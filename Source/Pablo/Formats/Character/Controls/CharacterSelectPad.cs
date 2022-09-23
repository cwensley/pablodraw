using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Controls;
using Eto;

namespace Pablo.Formats.Character.Controls
{
	public interface ICharacterSelectSource
	{
		CharacterHandler Handler { get; }
		Character CurrentCharacter { get; set; }
	}

	public class CharacterSelectPad : Panel
	{
		ICharacterSelectSource tool;
		FontTextBox preview;

		new CharacterHandler Handler
		{
			get { return this.tool.Handler; }
		}

		public CharacterSelectPad(ICharacterSelectSource tool)
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

			layout.Add(new Label { Text = "Char", TextAlignment = TextAlignment.Center, Font = new Font(SystemFont.Default, 7) });

			layout.AddCentered(GradientPreview(), Padding.Empty, Size.Empty);

			layout.AddCentered(SelectButton(), new Padding(0, 5, 0, 0), Size.Empty);

			Content = layout;

			this.Update();
		}

		void Update()
		{
			preview.Canvas.Fill(new CanvasElement(tool.CurrentCharacter, Handler.DrawAttribute));
			preview.UpdateCanvasSize();
			preview.Invalidate();
		}

		Control GradientPreview()
		{
			var control = preview = new FontTextBox(Handler, new Size(4, 2))
			{
				ReadOnly = true,
				Bordered = true
			};
			preview.MouseUp += (sender, e) =>
			{
				SelectCharacter();
				e.Handled = true;
			};
			return control;
		}

		void SelectCharacter()
		{
			var dlg = new CharacterSelection(this.Handler)
			{
				SelectedCharacter = this.tool.CurrentCharacter
			};
			var result = dlg.ShowModal(this);
			if (result == DialogResult.Ok)
			{
				this.tool.CurrentCharacter = dlg.SelectedCharacter;
				this.Update();
			}
		}

		Control SelectButton()
		{
			var button = new CustomButton
			{
				Size = new Size(20, 12),
#if DESKTOP
				ToolTip = "Edit selected character"
#endif
			};
			button.Paint += delegate(object sender, PaintEventArgs pe)
			{
				pe.Graphics.FillPolygon(button.DrawColor, new PointF[] { new Point(4, 2), new Point(16, 5), new Point(4, 8) });
			};

			button.Click += delegate
			{
				SelectCharacter();
			};

			return button;
		}

	}
}

