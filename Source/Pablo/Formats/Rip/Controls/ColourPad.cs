using System;
using Eto.Forms;
using Eto.Drawing;
using Eto;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Rip.Controls
{
	public class ColourPad : Panel
	{
		ColourBox foreground;
		ColourBox background;
		int start = 0;
		int max;
		SelectColourBox[] colours;

		public new RipHandler Handler { get; private set; }

		public Palette Palette {
			get { return Handler.RipDocument.Palette; }
		}
		
		#region ColourBox
		
		public abstract class ColourBox : Drawable
		{
			public abstract Color Color { get; set; }

			protected override void OnPaint (PaintEventArgs pe)
			{
				base.OnPaint (pe);
				var rect = new Rectangle (Point.Empty, this.Size);
				pe.Graphics.DrawInsetRectangle (Colors.Gray, Colors.White, rect);
				rect.Inflate (-1, -1);
				pe.Graphics.FillRectangle (Color, rect);
			}
		}
		
		class FBColourBox : ColourBox
		{
			public ColourPad Pad { get; set; }
			
			public override Color Color {
				get;
				set;
			}

			public FBColourBox ()
			{
			}
			
			protected override void OnMouseDown (MouseEventArgs e)
			{
				base.OnMouseDown (e);
				
				if (e.Buttons == MouseButtons.Primary) {
					/*var dialog = new AttributeDialog (Pad.Handler, Pad.Palette, Pad.Attribute, Pad.Handler.CharacterDocument.iCEColours);
					DialogResult dr = dialog.ShowDialog (this);
					if (dr == DialogResult.Ok)
						Pad.Attribute = dialog.Attribute;
					e.Handled = true;*/
				}
			}
			
		}
		
		class SelectColourBox : ColourBox
		{
			public int Index { get; set; }

			public ColourPad Pad { get; set; }
			
			public override Color Color {
				get {
					return Pad.Palette [Index];
				}
				set {
					//Pad.Handler.Undo.Save (new Undo.UndoColour{ Color = Pad.Handler.CurrentPage.Palette [Index], Index = Index });
					Pad.Palette [Index] = value;
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
					if (Pad.Handler.Foreground == Index) {
						var size = g.MeasureString (font, "F");
						
						pe.Graphics.DrawText (font, col, (int)(4 - size.Width / 2), (int)(8 - size.Height / 2), "F");
					}
				
					if (Pad.Handler.Background == Index) {
						var size = g.MeasureString (font, "B");
						pe.Graphics.DrawText (font, col, (int)(12 - size.Width / 2), (int)(8 - size.Height / 2), "B");
					}
				}
			}
			
			protected override void OnMouseDown (MouseEventArgs e)
			{
				if (!Enabled) return;
				base.OnMouseDown (e);
				switch (e.Buttons) {
				case MouseButtons.Primary:
					Pad.Handler.Foreground = (byte)Index;
					e.Handled = true;
					break;
				case MouseButtons.Alternate:
					Pad.Handler.Background = (byte)Index;
					e.Handled = true;
					break;
				}
				Pad.HandleColourChanged (this, EventArgs.Empty);
			}
			
			protected override void OnMouseDoubleClick (MouseEventArgs e)
			{
				base.OnMouseDoubleClick (e);
				e.Handled = true;
				
				var dialog = new ColourEditor(this.Pad.Handler) {
					SelectedColor = this.Index
				};
				if (dialog.ShowModal(this) == DialogResult.Ok) {
					this.Pad.SetColours ();
					this.Pad.Invalidate ();
				}
				
			}
		}
		
		#endregion
		
		void HandleColourChanged (object sender, EventArgs e)
		{
			SetColours ();
			this.Invalidate ();
		}
		
		void SetColours ()
		{
			foreground.Color = Palette [Math.Min (Palette.Count-1, Handler.Foreground)];
			background.Color = Palette [Math.Min (Palette.Count-1, Handler.Background)];
		}
		
		public ColourPad (RipHandler handler)
		{
			this.Handler = handler;
			//this.Handler.ColourStart = start;
			
			this.Handler.AttributeChanged += new EventHandler<EventArgs>(HandleColourChanged).MakeWeak(e => this.Handler.AttributeChanged -= HandleColourChanged);
			
			/*this.Handler.CurrentPage.PaletteChanged += new EventHandler<EventArgs> (delegate{
				SetColours ();
				UpdateButtons ();
				Invalidate ();
			}).MakeWeak ((e) => this.Handler.CurrentPage.PaletteChanged -= e);*/
			
			var boxSize = new Size (30, 30);
			int boxOffset = 10;
			int boxesPadding = 10;
			int boxesOffset = boxSize.Width + boxOffset + boxesPadding;
			
			max = 8; //Math.Min (16, Palette.Count / 2);

			Size = new Size (40, boxesOffset + (max * 18));
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

			Content = layout;
			
			UpdateButtons ();
		}
		
		void UpdateButtons ()
		{
			for (int i=0; i<colours.Length; i++) {
				colours[i].Index = start + i;
				colours[i].Enabled = (start + i) < Palette.Count;
			}
			Invalidate ();
			//Console.WriteLine ("start: {0} max: {1}", start, max);
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

