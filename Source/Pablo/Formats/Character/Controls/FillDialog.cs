using System;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo.Formats.Character.Controls
{
	[Flags]
	public enum FillMode
	{
		None = 0,
		Foreground = 0x01,
		Background = 0x02,
		Character = 0x04,
		Attribute = Foreground | Background,
		Both = Attribute | Character
	}

	public class FillDialog : Dialog
	{
		RadioButton rbAttr;
		RadioButton rbFore;
		RadioButton rbBack;
		RadioButton rbChar;
		RadioButton rbBoth;
		RadioButton rbController;
		ColourSelection colours;
		FillMode mode = FillMode.None;
		Control characterPanel;
		FontTextBox charTextBox;

		public Attribute Attribute {
			get { return colours.Attribute; }
		}

		public Character Character {
			get { return charTextBox.Canvas [0, 0].Character; }
		}
		
		new CharacterHandler Handler { get; set; }

		public FillMode FillMode {
			get { return mode; }
		}

		public override void OnKeyDown (KeyEventArgs e)
		{
			RadioButton rb = null;
			switch (e.KeyData) {
			case Key.F:
				rb = rbFore;
				break;
			case Key.B:
				rb = rbBack;
				break;
			case Key.C:
				rb = rbChar;
				break;
			case Key.A:
				rb = rbAttr;
				break;
			case Key.O:
				rb = rbBoth;
				break;
			case Key.Escape:
				DialogResult = DialogResult.Cancel;
				e.Handled = true;
				this.Close ();
				break;
			case Key.Enter:
				if (colours.HasFocus && characterPanel.Visible) {
					// now set character
					charTextBox.Focus ();
				} else {
					DialogResult = DialogResult.Ok;
					e.Handled = true;
					this.Close ();
				}
				break;
			}
			if (rb != null) {
				rb.Checked = true;
				e.Handled = true;
				UpdateSelection ();
			}
			base.OnKeyDown (e);
		}
		
		Control ColoursControl ()
		{
			colours = new ColourSelection (Handler.CurrentPage.Palette, Handler.DrawAttribute);
			colours.Size = new Size (250, 220);
			colours.Selected += delegate {
				if (colours.HasFocus && characterPanel.Visible) {
					// now set character
					charTextBox.Focus ();
				} else {
					DialogResult = DialogResult.Ok;
					Close ();
				}
			};
			colours.Changed += delegate {
				charTextBox.SetAttribute (this.Attribute);
			};
			return colours;
		}

		Control RightSection ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			
			layout.Add (ColoursControl (), yscale: true);
			layout.Add (CreateDrawCharacter ());
			
			return layout;
		}
		
		Control TopSection ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			layout.BeginHorizontal ();
			layout.Add (RightSection (), true);
			
			layout.Add (CreateSelections ());
			layout.EndHorizontal ();

			return layout;
		}

		public FillDialog (CharacterHandler handler)
		{
			this.Handler = handler;
#if DESKTOP
			this.Resizable = true;
#endif
			Title = "Fill Options";
			
			var layout = new DynamicLayout ();
			layout.Padding = new Padding (10);
			layout.Add (TopSection (), true, true);
			layout.AddSeparateRow (null, CancelButton (), OkButton ());

			Content = layout;

			UpdateFillMode ();
			colours.Focus ();
		}
		
		public override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			UpdateSelection ();
		}
		
		Control CreateDrawCharacter ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			layout.BeginHorizontal ();
			
			characterPanel = layout;
			
			layout.Add (new Label { Text = "C&haracter:" });
			
			charTextBox = new FontTextBox (Handler, new Size (1, 1));
			charTextBox.ReadOnly = false;
			charTextBox.SetAttribute (this.Attribute);
			
			layout.Add (charTextBox);
			layout.Add (null);
			
			layout.EndHorizontal ();
			
			
			return layout;
		}

		Control CreateSelections ()
		{
			var layout = new DynamicLayout (Padding.Empty);

			rbAttr = new RadioButton (null);
			rbAttr.Text = "&Attribute";
			rbAttr.Checked = true;
			rbAttr.CheckedChanged += rb_CheckedChanged;
			layout.Add (rbAttr);
			rbController = rbAttr;

			rbFore = new RadioButton (rbController);
			rbFore.Text = "&Foreground";
			rbFore.CheckedChanged += rb_CheckedChanged;
			layout.Add (rbFore);

			rbBack = new RadioButton (rbController);
			rbBack.Text = "&Background";
			rbBack.CheckedChanged += rb_CheckedChanged;
			layout.Add (rbBack);

			rbChar = new RadioButton (rbController);
			rbChar.Text = "&Character";
			rbChar.CheckedChanged += rb_CheckedChanged;
			layout.Add (rbChar);

			rbBoth = new RadioButton (rbController);
			rbBoth.Text = "B&oth";
			rbBoth.CheckedChanged += rb_CheckedChanged;
			layout.Add (rbBoth);
			
			layout.Add (null);
			
			return layout;
		}

		private void rb_CheckedChanged (Object sender, EventArgs e)
		{
			UpdateSelection ();
		}

		private void UpdateSelection ()
		{
			colours.Visible = rbFore.Checked | rbBack.Checked | rbAttr.Checked | rbBoth.Checked;

			colours.ShowBackground = rbBack.Checked | rbAttr.Checked | rbBoth.Checked;
			colours.ShowForeground = rbFore.Checked | rbAttr.Checked | rbBoth.Checked;
			characterPanel.Visible = rbChar.Checked | rbBoth.Checked;
			
			UpdateFillMode ();
			if (colours.Visible) {
				colours.Focus ();
				charTextBox.SetAttribute (this.Attribute);
			} else {
				charTextBox.SetAttribute (CanvasElement.Default.Attribute);
				charTextBox.Focus ();
			}
		}

		void UpdateFillMode ()
		{
			mode = FillMode.None;
			if (rbAttr.Checked)
				mode |= FillMode.Attribute;
			if (rbFore.Checked)
				mode |= FillMode.Foreground;
			if (rbBack.Checked)
				mode |= FillMode.Background;
			if (rbChar.Checked)
				mode |= FillMode.Character;
			if (rbBoth.Checked)
				mode |= FillMode.Both;
		}

		Control OkButton ()
		{
			var control = new Button {
				Text = "O&k"
			};
			control.Click += delegate {
				Close (DialogResult.Ok);
			};
			DefaultButton = control;
			return control;
		}

		Control CancelButton ()
		{
			var control = new Button {
				Text = "Cancel"
			};
			control.Click += delegate {
				Close (DialogResult.Cancel);
			};
			AbortButton = control;
			return control;
		}

	}
}
