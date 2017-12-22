using System;
using System.Reflection;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;
using Pablo.Formats.Character.Actions.Block;
using Eto.Forms;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public class ColourBrush : SizeTool
	{
		Actions.Block.Fill action;
		
		public bool PaintForeground { get; set; }
		
		public bool PaintBackground { get; set; }
		
		public override CharacterHandler Handler {
			get { return base.Handler; }
			set {
				base.Handler = value;
				action = new Actions.Block.Fill (value);
			}
		}
		
		public override Cursor MouseCursor {
			get { return new Cursor (CursorType.Crosshair); }
		}
		
		public override Eto.Drawing.Image Image {
			get { return Eto.Drawing.Bitmap.FromResource ("Pablo.Formats.Character.Icons.ColourBrush.png"); }
		}

		public override string Description {
			get { return "Color Brush - paint only foreground and/or background colour"; }
		}
		
		public override Key Accelerator {
			get {
				return Key.B | Key.Shift | (Handler.Generator.IsMac ? Key.Control : Key.Alt);
			}
		}
		
		
		public ColourBrush()
		{
			PaintForeground = true;
			PaintBackground = true;
		}
		
		Controls.FillMode GetFillMode (Key modifiers)
		{
			var mode = Controls.FillMode.None;
			if (modifiers.HasFlag (Key.Shift) ^ PaintForeground)
				mode |= Controls.FillMode.Foreground;
			
			if (modifiers.HasFlag (Key.Alt) ^ PaintBackground)
				mode |= Controls.FillMode.Background;
			return mode;
		}

		protected override void Draw (Point location, Eto.Forms.MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary) {
				action.Attributes = new FillAttributes{
					Rectangle = new Rectangle (location, new Size (this.Size, this.Size)),
					Mode = GetFillMode (e.Modifiers),
					Attribute = Handler.DrawAttribute
				};
				action.Activate ();
				
				var middle = (Size - 1) / 2;
				Handler.CursorPosition = new Point (location.X + middle, location.Y + middle);
			}
		}
		
		Control FGButton ()
		{
			var control = new ImageButton{
				Image = Bitmap.FromResource ("Pablo.Formats.Character.Icons.DrawForeground.png"),
				Toggle = true,
				Pressed = PaintForeground,
#if DESKTOP
				ToolTip = "Paint foreground (shift)"
#endif
			};
			
			control.Click += delegate {
				PaintForeground = control.Pressed;
			};
			return control;
		}

		Control BGButton ()
		{
			var control = new ImageButton{
				Image = Bitmap.FromResource ("Pablo.Formats.Character.Icons.DrawBackground.png"),
				Toggle = true,
				Pressed = PaintBackground,
#if DESKTOP
				ToolTip = "Paint background (alt)"
#endif
			};
			
			control.Click += delegate {
				PaintBackground = control.Pressed;
			};
			return control;
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			
			layout.Add (Separator ());
			layout.BeginVertical (Padding.Empty, Eto.Drawing.Size.Empty);
			layout.AddRow (FGButton (), BGButton ());
			layout.EndVertical ();
			
			layout.Add (base.GeneratePad ());
			return layout;
		}
		
	}
}

