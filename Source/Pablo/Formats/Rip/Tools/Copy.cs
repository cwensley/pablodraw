using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Pablo.BGI;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Rip.Tools
{
	public class Copy : CenterAspectTool<Commands.GetImage>
	{
		bool enableClear = false;
		
		public override string Description {
			get { return "Copies the selected region into the clipboard (C)"; }
		}

		public override Keys Accelerator {
			get {
				return Keys.C;
			}
		}
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Copy.png"); }
		}
		
		protected override void SetPosition (Rectangle rect)
		{
			rect.Restrict (new Rectangle(this.BGI.WindowSize));
			this.Command.Rectangle = rect;
		}
		
		bool EnableClear (Keys modifiers)
		{
			return enableClear/* ^ (modifiers == (Key.Alt | Key.Shift))*/;
		}
		
		protected override void ApplyInvertedDrawing (IList<Rectangle> updates = null)
		{
			var rect = this.Command.Rectangle;
			var oldlinestyle = this.BGI.GetLineStyle ();
			var oldcol = this.BGI.GetColor ();
			this.BGI.SetLineStyle (BGICanvas.LineStyle.Dashed);
			this.BGI.SetColor (15);
			
			this.BGI.Rectangle (rect, updates);
			
			this.BGI.SetColor (oldcol);
			this.BGI.SetLineStyle (oldlinestyle);
		}
		
		protected override void FinishCommand (Keys modifiers, IList<Rectangle> updates)
		{
			var rect = this.Command.Rectangle;
			base.FinishCommand (modifiers, updates);
			
			if (EnableClear (modifiers)) {
				Handler.ApplyIfNeeded<Commands.FillStyle> (updates);
				Handler.ApplyIfNeeded<Commands.FillPattern> (updates);
				var bar = Document.Create<Commands.Bar> ();
				bar.Rectangle = rect;
				Handler.AddCommand (bar, updates);
			}
			
			Handler.SelectTool<Paste> ();
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = Size.Empty };
			
			layout.BeginHorizontal ();
			
			var b = new ImageButton{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Copy-Clear.png"),
#if DESKTOP
				ToolTip = "Clear copied region",
#endif
				Toggle = true,
				Pressed = enableClear
			};
			b.Click += delegate {
				enableClear = b.Pressed;
			};
			
			layout.Add (b);
			layout.Add (null);
			layout.EndHorizontal ();
			
			return layout;
		}
		
	}
}

