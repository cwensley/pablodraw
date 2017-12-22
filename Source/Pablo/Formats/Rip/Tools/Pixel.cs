using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Pablo.Controls;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Tools
{
	public class Pixel : TwoPointTool<Commands.Pixel>
	{
		bool enableMulti = true;
		
		public override string Description {
			get { return "Draws a pixel (P)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.P;
			}
		}
		
		protected override bool AllowSameEndPoint {
			get {
				return true;
			}
		}
		
		public override IEnumerable<RipOptionalCommand> Styles {
			get {
				foreach (var style in base.Styles)
					yield return style;
				yield return Document.Create<Commands.Color>();
			}
		}
		
		bool EnableMulti (Keys modifiers)
		{
			return enableMulti ^ (modifiers == Keys.Shift);
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Pixel.png"); }
		}
		
		protected override void SetStartLocation (Point start, Keys modifiers, Point location)
		{
			this.Command.Point = start;
			if (!EnableMulti (modifiers))
				FinishCommand (modifiers);
		}
		
		protected override void SetEndLocation (Point end, Keys modifiers, Point location)
		{
			this.Command.Point = end;
		}
		
		public override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);
			if (EnableMulti (e.Modifiers))
				ApplyDuplicate ();
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };

			layout.Add(TopSeparator());
			layout.BeginVertical (Padding.Empty);
			layout.BeginHorizontal ();
			
			var b = new ImageButton{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Pixel-Multi.png"),
				Toggle = true,
				Pressed = enableMulti
			};
			b.Click += delegate {
				enableMulti = b.Pressed;
			};
			
			layout.Add (b);
			layout.Add (null);
			layout.EndHorizontal ();
			layout.EndVertical ();
			
			return layout;
		}
		
	}
}

