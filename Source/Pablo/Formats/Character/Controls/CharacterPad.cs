using System;
using Eto.Forms;
using System.Linq;
using Eto;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Character.Controls
{
	public class CharacterPad : Panel
	{
		CustomButton upButton;
		CustomButton downButton;
		
		public new CharacterHandler Handler { get; private set; }

		List<FontTextBox> chars = new List<FontTextBox> ();

		public CharacterPad (CharacterHandler handler)
		{
			this.Handler = handler;
			handler.CharacterSetChanged += new EventHandler<EventArgs> (delegate {
				Update ();
			}).MakeWeak ((e) => handler.CharacterSetChanged -= e);
			handler.DrawAttributeChanged += new EventHandler<EventArgs> (delegate {
				Update ();
			}).MakeWeak ((e) => handler.DrawAttributeChanged -= e);
			handler.CurrentPage.PaletteChanged += new EventHandler<EventArgs> (delegate {
				Update ();
			}).MakeWeak ((e) => handler.CurrentPage.PaletteChanged -= e);
			handler.CurrentPage.FontChanged += new EventHandler<EventArgs> (delegate {
				UpdateSizes ();
				//this.ParentLayout.Container.ParentLayout.Update ();
			}).MakeWeak ((e) => handler.CurrentPage.FontChanged -= e);
			
			CreateControls ();
		}
		
		protected override void OnSizeChanged (EventArgs e)
		{
			base.OnSizeChanged (e);
			Invalidate ();
		}
		
		void UpdateSizes()
		{
			SuspendLayout();
			foreach (var charbox in chars) {
				charbox.UpdateCanvasSize();
				charbox.Invalidate ();
			}
			ResumeLayout();
		}
		
		void CreateControls ()
		{
			var handler = this.Handler;
			
			//int charHeight = handler.CurrentPage.Font.Size.Height + 6;
			var characterSet = handler.Info.GetCharacterSet (handler.CharacterSet).ToList ();
			//int width = 40;
			//int height = charHeight * characterSet.Count + 18 + 17;
			//this.MinimumSize = new Size (width, height);
			
			var layout = new DynamicLayout { Padding = new Padding(10, 0) };
			
			layout.BeginHorizontal ();
			//var layout = this.Layout as PixelLayout ?? new PixelLayout (this);
			//layout.Remove (this.Controls.ToArray ());
			chars.Clear ();
			
			for (int i=0; i<characterSet.Count; i++) {
				var character = new FontTextBox (handler, new Size(1, 1));

				var label = new Label{ };//Size = new Size (width - character.Size.Width, charHeight)};
				label.TextAlignment = TextAlignment.Right;
				label.VerticalAlignment = VerticalAlignment.Center;
				label.Text = string.Format ("F{0}", i + 1);
				layout.Add (label);
				//layout.Add (label, 0, i * charHeight);
				
				character.Tag = i;
				character.CanFocus = false;
				character.MouseDown += HandleCharacterMouseDown;
				layout.AddCentered (character, horizontalCenter: false, padding: Padding.Empty, spacing: Size.Empty);
				//layout.Add (character, width - character.Size.Width, i * charHeight + (charHeight - character.Size.Height) / 4);
				chars.Add (character);
			}
			
			layout.BeginVertical (Padding.Empty, Size.Empty);
			layout.Add (null);
			layout.Add (UpButton ());
			layout.Add (DownButton ());
			layout.Add (null);
			layout.EndVertical ();
			
			layout.AddCentered (SelectButton (), Padding.Empty, Size.Empty);
			layout.EndHorizontal ();

			Content = layout;
			Update ();
			
		}
		
		Control SelectButton ()
		{
			var button = new CustomButton{
				Size = new Size (10, 20),
#if DESKTOP
				ToolTip = "Edit character sets"
#endif
			};
			button.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (button.DrawColor, new PointF[] { new Point (2, 14), new Point (5, 2), new Point (8, 14)});
			};

			button.Click += delegate {
				var dlg = new CharacterSetEditor (this.Handler);
				dlg.Owner = ParentWindow;
				dlg.ShowModal();
			};
			
			return button;
		}
		
		Control UpButton ()
		{
			var control = new CustomButton{ Size = new Size (12, 14) };
			control.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (control.DrawColor, new PointF[] { new Point (2, 12), new Point (6, 2), new Point (10, 12) });
				
			};
			control.Click += delegate {
				Handler.CharacterSet = Math.Max (Handler.CharacterSet - 1, 0);
				Update ();
			};
			upButton = control;
			return control;
		}

		Control DownButton ()
		{
			var control = new CustomButton{ Size = new Size (12, 14) };
			control.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (control.DrawColor, new PointF[] { new Point (2, 2), new Point (6, 12), new Point (10, 2) });
			};
			control.Click += delegate {
				Handler.CharacterSet = Math.Min (Handler.CharacterSet + 1, CharacterDocumentInfo.MAX_CHARACTER_SETS - 1);
				Update ();
			};
			downButton = control;
			return control;
		}

		void HandleCharacterMouseDown (object sender, MouseEventArgs e)
		{
			var tb = sender as FontTextBox;
			var charIndex = (int)tb.Tag;
			var character = Handler.Info.GetCharacter (Handler.CharacterSet, charIndex);
			Handler.InsertCharacter (character);
			e.Handled = true;
		}

		void Update ()
		{
			upButton.Enabled = Handler.CharacterSet > 0;
			downButton.Enabled = Handler.CharacterSet < CharacterDocumentInfo.MAX_CHARACTER_SETS - 1;
			var characterSet = Handler.Info.GetCharacterSet (Handler.CharacterSet).ToList ();
			for (int i = 0; i < chars.Count; i++) {
				var ch = chars [i];
				var ce = new CanvasElement (new Character (characterSet [i]), Handler.DrawAttribute);
				ch.Canvas [0, 0] = ce;
				if (Loaded)
					ch.Invalidate ();
			}
		}
		
	}
}

