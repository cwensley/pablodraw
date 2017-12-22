using System;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo.Formats.Character.Controls
{
	public class AttributeDialog : Dialog<bool>
	{
		ColourSelection colours;
		CharacterHandler handler;

		public Attribute Attribute
		{
			get { return colours.Attribute; }
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyData == Keys.Escape ||
				e.KeyData == Keys.Enter)
			{
				Result = true;
				Close();
				e.Handled = true;
			}
		}

		public AttributeDialog(CharacterHandler handler, Palette palette, Attribute attribute, bool iceColours)
		{
			this.handler = handler;
			Title = "Select attribute";
			var pos = handler.CharacterDocument.Info.AttributeDialogBounds;
			if (pos != null)
				Bounds = pos.Value;
			else
				ClientSize = handler.CharacterDocument.Info.AttributeDialogSize ?? new Size(260, 260);
#if DESKTOP
			this.Resizable = true;
#endif
			
			TableLayout layout = new TableLayout(1, 2);

			colours = new ColourSelection(palette, attribute);
			colours.Size = new Size(250, 200);
			colours.Selected += colours_Selected;
			colours.iCEColours = iceColours;
			layout.Add(colours, 0, 0, true, true);

			layout.Add(GenerateButtons(), 0, 1, true, false);

			Content = layout;
		}

		protected override void OnLoadComplete (EventArgs e)
		{
			base.OnLoadComplete (e);
			colours.Focus ();
		}
		
		protected override void OnClosed (EventArgs e)
		{
			base.OnClosed (e);
			handler.CharacterDocument.Info.AttributeDialogBounds = this.RestoreBounds;
			handler.CharacterDocument.Info.AttributeDialogSize = this.ClientSize;
		}

		void colours_Selected(object sender, EventArgs e)
		{
			Result = true;
			Close();
		}

		private Control GenerateButtons()
		{
			TableLayout layout = new TableLayout(1,1) {
				Size = new Size(150, 28)
			};
			layout.Spacing = new Size(5, 5);
			layout.Padding = Padding.Empty;
			//layout.SetColumnScale(0);

			Button bOk = new Button();
			bOk.Text = "O&k";
			bOk.Size = new Size(50, 28);
			bOk.Click += bOk_Click;
			//DefaultButton = bOk;
			layout.Add(bOk, 0, 0);

			return layout;
		}

		void bOk_Click(object sender, EventArgs e)
		{
			Result = true;
			Close();
		}
	}
}
