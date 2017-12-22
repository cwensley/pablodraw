using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Controls;
using Pablo.BGI;
using System.Collections;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Controls
{
	public class LineStyleEditor : Dialog<DialogResult>
	{
		RipHandler handler;
		ToggleGrid grid;
		BGICanvas canvas;

		public uint LinePattern
		{
			get
			{
				byte[] pattern = new byte[2];
				grid.Bits.CopyTo(pattern, 0);
				return ((uint)pattern[0] << 8) + pattern[1];
			}
			set
			{
				grid.Bits = new BitArray(new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) });
			}
		}

		public LineStyleEditor(RipHandler handler)
		{
			this.Title = "Custom Line Style";
			this.handler = handler;
			//this.Resizable = true;
			
			var layout = new DynamicLayout();
			
			layout.BeginVertical();
			
			layout.Add(Design());
			
			layout.Add(Preview());
			
			layout.EndVertical();

			layout.BeginVertical();
			layout.BeginHorizontal();
			
			layout.Add(null, true);
			layout.Add(CancelButton());
			layout.Add(OkButton());
			
			layout.EndHorizontal();
			layout.EndVertical();

			Content = layout;
		}

		Control Design()
		{
			var layout = new DynamicLayout();
			
			layout.AddCentered(Grid(), xscale: true, yscale: false);
			
			layout.BeginVertical();
			layout.BeginHorizontal();
			
			layout.Add(null, true);
			layout.Add(ClearAllButton());
			layout.Add(InvertButton());
			layout.Add(MoveLeftButton());
			layout.Add(MoveRightButton());
			layout.Add(null, true);
			
			layout.EndHorizontal();
			layout.EndVertical();
			return new GroupBox { Text = "Design", Content = layout };
		}

		Control ClearAllButton()
		{
			var button = new Button{ Text = "Clear" };
			button.Click += delegate
			{
				this.LinePattern = 0;
			};
			return button;
		}

		Control InvertButton()
		{
			var button = new Button{ Text = "Invert" };
			button.Click += delegate
			{
				this.LinePattern = ~this.LinePattern;
			};
			return button;
		}

		Control MoveRightButton()
		{
			var button = new Button{ Text = ">", Size = new Size(40, -1) };
			button.Click += delegate
			{
				var pattern = this.LinePattern;
				pattern = (pattern << 1) + ((pattern & 0x8000) >> 15);
				this.LinePattern = pattern;
			};
			return button;
		}

		Control MoveLeftButton()
		{
			var button = new Button{ Text = "<", Size = new Size(40, -1) };
			button.Click += delegate
			{
				var pattern = this.LinePattern;
				pattern = (pattern >> 1) + ((pattern & 0x0001) << 15);
				this.LinePattern = pattern;
			};
			return button;
		}

		Control Grid()
		{
			grid = new ToggleGrid(new Size(16, 1));
			grid.BitsChanged += delegate
			{
				DrawCanvas();
				
			};
			this.LinePattern = handler.LinePattern;
			return grid;
		}

		void DrawCanvas()
		{
			if (canvas == null)
				return;
			var updates = new List<Rectangle>();
			canvas.GraphDefaults(updates);
			canvas.SetColor(handler.Foreground);
			
			canvas.SetLineStyle(BGICanvas.LineStyle.User, this.LinePattern, 1);
			
			canvas.Line(20, 20, 108, 20, updates);
			canvas.Line(20, 20, 108, 64, updates);
			canvas.Line(20, 20, 108, 108, updates);
			canvas.Line(20, 20, 64, 108, updates);
			canvas.Line(20, 20, 20, 108, updates);

			canvas.SetLineStyle(BGICanvas.LineStyle.User, this.LinePattern, 3);
			
			canvas.Line(236, 20, 148, 20, updates);
			canvas.Line(236, 20, 148, 64, updates);
			canvas.Line(236, 20, 148, 108, updates);
			canvas.Line(236, 20, 192, 108, updates);
			canvas.Line(236, 20, 236, 108, updates);
			
			canvas.Control.Invalidate();
		}

		Control Preview()
		{
			var size = new Size(256, 128);
			var drawable = new Drawable{ Size = size };
			drawable.Paint += delegate(object sender, PaintEventArgs pe)
			{
				this.canvas.DrawRegion(pe.Graphics, new Rectangle(size));
			};
			drawable.LoadComplete += delegate
			{
				canvas = new BGI.BGICanvas(drawable, size);
				DrawCanvas();
			};
			
			var layout = new DynamicLayout();
			layout.AddCentered(drawable);
			return new GroupBox{ Text = "Preview", Content = layout };
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
			
			base.DefaultButton = control;
			
			return control;
		}
		
	}
}

