using System;
using Eto.Forms;
using Eto.Drawing;
using Eto;
using System.Collections.Generic;
using Pablo.Controls;
using Pablo.Drawing;

namespace Pablo.Formats.Character.Controls
{
	public class ColourPad : Panel
	{
		ColourBox foreground;
		ColourBox background;
		int start;
		int max;
		CustomButton upButton;
		CustomButton downButton;
		SelectColourBox[] colours;

		public new CharacterHandler Handler { get; private set; }

		public Palette Palette {
			get { return Handler.CurrentPage.Palette; }
		}

		#region ColourBox
		
		class FBColourBox : ColourBox
		{
			public ColourPad Pad { get; set; }
			
			public override Color Color {
				get;
				set;
			}

			protected override void OnMouseDown (MouseEventArgs e)
			{
				base.OnMouseDown (e);
				
				if (e.Buttons == MouseButtons.Primary) {
					var dialog = new AttributeDialog (Pad.Handler, Pad.Palette, Pad.Attribute, Pad.Handler.CharacterDocument.ICEColours);
					if (dialog.ShowModal(this))
						Pad.Attribute = dialog.Attribute;
					e.Handled = true;
				}
			}
			
		}
		
		class SelectColourBox : ColourBox
		{
			public int Index { get; set; }

			public ColourPad Pad { get; set; }
			
			public override Color Color {
				get {
					return Pad.Handler.CurrentPage.Palette [Index];
				}
				set {
					Pad.Handler.Undo.Save (new Undo.UndoColour{ Color = Pad.Handler.CurrentPage.Palette [Index], Index = Index });
					Pad.Handler.CurrentPage.Palette [Index] = value;
					Pad.Handler.Document.IsModified = true;
					Invalidate ();
					Pad.SetColours ();
					Pad.foreground.Invalidate ();
					Pad.background.Invalidate ();
				}
			}

			Font font;
			
			public SelectColourBox ()
			{
				font = Fonts.Sans(8);
			}
			
			protected override void OnPaint (PaintEventArgs pe)
			{
				if (Enabled) {
					base.OnPaint (pe);
					
					var g = pe.Graphics;
					
					var col = this.Color;
					col.Invert ();
					if (Pad.Handler.DrawAttribute.Foreground == Index) {
						var size = g.MeasureString (font, "F");
						
						pe.Graphics.DrawText (font, col, (int)(4 - size.Width / 2), (int)(8 - size.Height / 2), "F");
					}
				
					if (Pad.Handler.DrawAttribute.Background == Index) {
						var size = g.MeasureString (font, "B");
						pe.Graphics.DrawText (font, col, (int)(12 - size.Width / 2), (int)(8 - size.Height / 2), "B");
					}
				}
			}
			
			protected override void OnMouseDown (MouseEventArgs e)
			{
				if (!Enabled) return;
				base.OnMouseDown (e);
				var attr = Pad.Attribute;
				switch (e.Buttons) {
				case MouseButtons.Primary:
					attr.Foreground = (byte)Index;
					e.Handled = true;
					break;
				case MouseButtons.Alternate:
					attr.Background = (byte)Index;
					e.Handled = true;
					break;
				}
				Pad.Attribute = attr;
				Pad.HandleColourChanged (this, EventArgs.Empty);
			}
			
			protected override void OnMouseDoubleClick (MouseEventArgs e)
			{
				base.OnMouseDoubleClick (e);
				e.Handled = true;
				
				var col = new ColorDialog{ Color = this.Color };
				col.ColorChanged += delegate {
					var client = Pad.Handler.Client;
					if (client != null) {
						client.SendCommand (new Actions.SetColour(Pad.Handler){ Color = col.Color, Index = this.Index });
					}
					else
						this.Color = col.Color;
				};
				col.ShowDialog (this.ParentWindow);
			}
		}
		
		#endregion
		
		public Attribute Attribute {
			get { return Handler.DrawAttribute; }
			set { Handler.DrawAttribute = value; }
		}
		
		void HandleColourChanged (object sender, EventArgs e)
		{
			SetColours ();
			this.Invalidate ();
		}
		
		void SetColours ()
		{
			var ice = this.Handler.CharacterDocument.ICEColours;
			foreground.Color = Palette [Math.Min (Palette.Count-1, Attribute.Foreground)];
			background.Color = Palette [Math.Min (Palette.Count-1, ice ? Attribute.Background : Attribute.BackgroundOnly)];
		}
		
		public ColourPad (CharacterHandler handler)
		{
			this.Handler = handler;
			this.Handler.ColourStart = start;

			this.Handler.DrawAttributeChanged += new EventHandler<EventArgs> (HandleColourChanged).MakeWeak ((e) => this.Handler.DrawAttributeChanged -= e);
			
			this.Handler.CurrentPage.PaletteChanged += new EventHandler<EventArgs> (delegate{
				SetColours ();
				UpdateButtons ();
				Invalidate ();
			}).MakeWeak ((e) => this.Handler.CurrentPage.PaletteChanged -= e);
			
			var boxSize = new Size (30, 30);
			int boxOffset = 10;
			int boxesPadding = 10;
			int boxesOffset = boxSize.Width + boxOffset + boxesPadding;
			
			max = 8; //Math.Min (16, Palette.Count / 2);

			Size = new Size (40, boxesOffset + (max * 18) + 16 + 17);
			foreground = new FBColourBox{
				Size = boxSize,
				Pad = this
			};
			
			background = new FBColourBox{
				Size = boxSize,
				Pad = this
			};
			
			SetColours ();
			
			var layout = new PixelLayout ();
			layout.Add (background, boxOffset, boxOffset);
			layout.Add (foreground, 0, 0);

			colours = new SelectColourBox[max*2];
			
			for (int i = 0; i < max; i++) {
				CreateButton (layout, i, 2, boxesOffset + i * 18);
				CreateButton (layout, i + max, 22, boxesOffset + i * 18);
			}
			
			layout.Add (SelectButton (), 10, this.Size.Height - 16);
			layout.Add (UpButton (), 2, this.Size.Height - 16 - 15);
			layout.Add (DownButton (), 22, this.Size.Height - 16 - 15);

			Content = layout;
			
			UpdateButtons ();
		}
		
		void UpdateButtons ()
		{
			upButton.Enabled = start > 0;
			downButton.Enabled = start + max * 2 < Palette.Count; 
			for (int i=0; i<colours.Length; i++) {
				colours[i].Index = start + i;
				colours[i].Enabled = (start + i) < Palette.Count;
			}
			Invalidate ();
			//Console.WriteLine ("start: {0} max: {1}", start, max);
		}
		
		Control UpButton()
		{
			var control = new CustomButton{ Size = new Size(16, 12) };
			control.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (control.DrawColor, new PointF[] { new Point (3, 6), new Point (13, 10), new Point (13, 2) });
				
			};
			control.Click += delegate {
				start -= max;
				Handler.ColourStart = start;
				UpdateButtons ();
			};
			upButton = control;
			return control;
		}

		Control DownButton()
		{
			var control = new CustomButton{ Size = new Size(16, 12) };
			control.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (control.DrawColor, new PointF[] { new Point (13, 6), new Point (3, 10), new Point (3, 2) });
			};
			control.Click += delegate {
				start += max;
				Handler.ColourStart = start;
				UpdateButtons ();
			};
			downButton = control;
			return control;
		}
		
		Control SelectButton ()
		{
			var button = new CustomButton{
				Size = new Size (20, 12),
#if DESKTOP
				ToolTip = "Edit the colour palette"
#endif
			};
			button.Paint += delegate(object sender, PaintEventArgs pe) {
				pe.Graphics.FillPolygon (button.DrawColor, new PointF[] { new Point (4, 2), new Point (16, 5), new Point (4, 8)});
			};

			button.Click += delegate {
				var dlg = new ColourEditor (this.Handler);
				dlg.ShowModal(this);
			};
			return button;
		}
		
		void CreateButton (PixelLayout layout, int index, int x, int y)
		{
			var b = new SelectColourBox{
				Pad = this,
				Size = new Size(16, 16),
				Index = index,
				Enabled = index < Palette.Count
			};
			colours[index] = b;
			layout.Add (b, x, y);
		}
	}
}

