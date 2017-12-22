using System;
using Eto.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Eto.Forms;

namespace Pablo.Formats.Rip.Tools
{
	public class Fill : RipTool
	{
		public override string Description {
			get { return "Fills the region up to the selected foreground color (F)"; }
		}

		public override Key Accelerator {
			get {
				return Key.F;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get {
				return Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Fill.png");
			}
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.FillStyle>();
				yield return Document.Create<Commands.FillPattern>();
			}
		}
		
		public override void Unselected ()
		{
			
		}
		
		public override void OnMouseUp (Eto.Forms.MouseEventArgs e)
		{
			ApplyStyles ();
			var fill = RipCommands.Create <Commands.Fill>(Document);
			fill.Point = (Point)e.Location;
			fill.Border = Handler.Foreground;
			Handler.AddCommand (fill);
			Handler.FlushCommands (null);
			e.Handled = true;
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout(Padding.Empty);
			layout.Add(Separator());
			layout.Add (new Controls.FillStylePad(Handler));
			return layout;
		}
		
		public override void ApplyDrawing (IList<Rectangle> updates)
		{
			
		}
		
		public override void RemoveDrawing (IList<Rectangle> updates)
		{
			
		}
		
	}
}

