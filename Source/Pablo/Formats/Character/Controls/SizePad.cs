using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Formats.Character.Controls
{
	public class SizePad : Panel
	{
		Tools.SizeTool tool;
		
		public SizePad (Tools.SizeTool tool)
		{
			this.tool = tool;
			var layout = new DynamicLayout { Padding = Padding.Empty };
			
			layout.BeginVertical (Padding.Empty, Size.Empty);
			layout.Add (new Label{ Text = "Size", HorizontalAlign = HorizontalAlign.Center, Font = new Font (SystemFont.Default, 7)});
			layout.Add (BrushSize ());
			layout.EndVertical ();
			Content = layout;
		}
		
		
		Control BrushSize()
		{
			var control = new NumericUpDown{
				Font = new Font (SystemFont.Default, 8),
				MinValue = 1,
				MaxValue = 9,
				Value = tool.Size,
				Size = new Size(20, -1)
			};
			
			control.ValueChanged += delegate {
				tool.Size = (int)control.Value;
			};
			
			return control;
		}
		
	}
}

