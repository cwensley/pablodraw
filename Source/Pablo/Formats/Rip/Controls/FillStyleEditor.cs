using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Controls;
using Pablo.BGI;
using System.Collections;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Controls
{
	public class FillStyleEditor : Dialog
	{
		RipHandler handler;
		ToggleGrid grid;
		BGICanvas canvas;
		
		public byte[] FillPattern {
			get {
				byte[] pattern = new byte[8];
				grid.Bits.CopyTo (pattern, 0);
				return pattern;
			}
			set {
				grid.Bits = new BitArray (value);
			}
		}
		
		public FillStyleEditor (RipHandler handler)
		{
			this.Title = "Custom Fill Style";
			this.handler = handler;
			//this.Resizable = true;
			
			var layout = new DynamicLayout ();
			
			layout.BeginVertical ();
			layout.BeginHorizontal ();
			layout.Add (Design ());
			
			layout.Add (Preview ());

			layout.EndHorizontal ();
			layout.EndVertical ();

			layout.BeginVertical ();
			layout.BeginHorizontal ();
			
			layout.Add (null, true);
			layout.Add (CancelButton ());
			layout.Add (OkButton ());
			
			layout.EndHorizontal ();
			layout.EndVertical ();

			Content = layout;
			
		}
		
		Control Design ()
		{
			var layout = new DynamicLayout();
			
			layout.AddCentered (Grid (), true, false);
			
			layout.BeginVertical (Padding.Empty);
			layout.BeginHorizontal ();
			
			layout.Add (null, true);
			layout.Add (ClearAllButton ());
			layout.Add (InvertButton ());
			layout.Add (null, true);
			
			layout.EndHorizontal ();
			layout.EndVertical ();
			layout.BeginVertical (Padding.Empty);
			layout.BeginHorizontal ();
			
			layout.Add (MoveUpButton ());
			layout.Add (MoveDownButton ());
			layout.Add (MoveLeftButton ());
			layout.Add (MoveRightButton ());
			
			layout.EndHorizontal ();
			layout.EndVertical ();
			return new GroupBox { Text = "Design", Content = layout };
		}
		
		Control ClearAllButton()
		{
			var button = new Button{ Text = "Clear" };
			button.Click += delegate {
				var bytes = FillPattern;
				for (int i=0; i<bytes.Length; i++) {
					bytes[i] = 0;
				}
				FillPattern = bytes;
			};
			return button;
		}

		Control InvertButton()
		{
			var button = new Button{ Text = "Invert" };
			button.Click += delegate {
				var bytes = FillPattern;
				for (int i=0; i<bytes.Length; i++) {
					bytes[i] = (byte)~bytes[i];
				}
				FillPattern = bytes;
			};
			return button;
		}

		Control MoveUpButton()
		{
			var button = new Button{ Text = "^", Size = new Size(40, Button.DefaultSize.Height) };
			button.Click += delegate {
				var oldpattern = this.FillPattern;
				var pattern = this.FillPattern;
				for (int i = 0; i<pattern.Length; i++) {
					var ofs = i < pattern.Length - 1 ? i + 1 : 0;
					pattern[i] = oldpattern[ofs];
				}
				this.FillPattern = pattern;
			};
			return button;
		}
		Control MoveDownButton()
		{
			var button = new Button{ Text = "v", Size = new Size(40, Button.DefaultSize.Height) };
			button.Click += delegate {
				var oldpattern = this.FillPattern;
				var pattern = this.FillPattern;
				for (int i = 0; i<pattern.Length; i++) {
					var ofs = i > 0 ? i - 1 : pattern.Length - 1;
					pattern[i] = oldpattern[ofs];
				}
				this.FillPattern = pattern;
			};
			return button;
		}
		
		Control MoveLeftButton()
		{
			var button = new Button{ Text = "<", Size = new Size(40, Button.DefaultSize.Height) };
			button.Click += delegate {
				var pattern = this.FillPattern;
				for (int i = 0; i<pattern.Length; i++) {
					pattern[i] = (byte)((pattern[i] << 1) + ((pattern[i] & 0x80) >> 7));
				}
				this.FillPattern = pattern;
			};
			return button;
		}
		Control MoveRightButton()
		{
			var button = new Button{ Text = ">", Size = new Size(40, Button.DefaultSize.Height) };
			button.Click += delegate {
				var pattern = this.FillPattern;
				for (int i = 0; i<pattern.Length; i++) {
					pattern[i] = (byte)((pattern[i] >> 1) + ((pattern[i] & 0x01) << 7));
				}
				this.FillPattern = pattern;
			};
			return button;
		}
		
		Control Grid ()
		{
			grid = new ToggleGrid (new Size (8, 8)) { Invert = true };
			grid.BitsChanged += delegate {
				DrawCanvas ();
				
			};
			this.FillPattern = handler.FillPattern;
			return grid;
		}
		
		void DrawCanvas ()
		{
			if (canvas == null)
				return;
			var updates = new List<Rectangle> ();
			canvas.GraphDefaults (updates);
			canvas.SetFillPattern (FillPattern, handler.Background);
			canvas.Bar (new Rectangle(canvas.WindowSize), updates);
			canvas.Control.Invalidate ();
		}
		
		Control Preview ()
		{
			var size = new Size (128, 128);
			var drawable = new Drawable{ Size = size };
			drawable.Paint += (sender, pe) =>
			{
				if (canvas != null)
					canvas.DrawRegion(pe.Graphics, new Rectangle(size));
			};
			drawable.LoadComplete += delegate {
				canvas = new BGICanvas(Generator, drawable, size);
				DrawCanvas ();
			};
			
			var layout = new DynamicLayout();
			layout.AddCentered (drawable, verticalCenter:false);
			return new GroupBox { Text = "Preview", Content = layout };
		}
		
		Control CancelButton ()
		{
			var control = new Button{ 
				Text = "Cancel" 
			};
			
			control.Click += delegate {
				DialogResult = DialogResult.Cancel;
				Close ();
			};
			AbortButton = control;
			
			return control;
		}
		
		Control OkButton ()
		{
			var control = new Button{ 
				Text = "Ok" 
			};
			
			control.Click += delegate {
				DialogResult = DialogResult.Ok;
				
				
				Close ();
			};
			
			base.DefaultButton = control;
			
			return control;
		}
		
	}
}

