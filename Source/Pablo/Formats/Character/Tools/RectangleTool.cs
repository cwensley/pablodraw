using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Formats.Character.Controls;
using Pablo.Formats.Character.Actions.Block;
using Pablo.Controls;
using Pablo.Drawing;

namespace Pablo.Formats.Character.Tools
{
	public class RectangleTool : CenterAspectTool, ICharacterSelectSource
	{
		ScanLines lines = new ScanLines ();
		
		public override Eto.Drawing.Image Image {
			get { return ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Rectangle.png"); }
		}

		public override string Description {
			get { return "Rectangle - Draw a box"; }
		}
		
		public override Keys Accelerator {
			get {
				return Keys.R | (Handler.Generator.IsMac ? Keys.Control : Keys.Alt);
			}
		}
		
		public override Cursor MouseCursor {
			get { return new Cursor (CursorType.Crosshair); }
		}
		
		public Character CurrentCharacter { get; set; }
		
		bool shouldApplyCharacter;

		public bool ApplyCharacter { get; set; }
		
		bool shouldApplyColour;

		public bool ApplyColour { get; set; }
		
		public bool Filled { get; set; }
		
		public RectangleTool ()
		{
			ApplyColour = true;
			ApplyCharacter = true;
			CurrentCharacter = 219;
		}
		
		protected override bool Apply ()
		{
			var rect = CurrentRectangle;
			if (rect != null) {
				// execute the command!
				var action = new Actions.Drawing.DrawRect (Handler) {
					Rectangle = rect.Value,
					ApplyColour = shouldApplyColour,
					ApplyCharacter = shouldApplyCharacter,
					Filled = this.Filled,
					Element = new CanvasElement (CurrentCharacter, Handler.DrawAttribute)
				};
				action.Execute();
				return true;
			}
			return false;
		}
		
		protected override void UpdateWithLocation (Rectangle rect, Keys modifiers, Point end)
		{
			shouldApplyCharacter = ApplyCharacter; //modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ ApplyCharacter;
			shouldApplyColour = ApplyColour; //modifiers.HasFlag (Key.Alt | Application.Instance.CommonModifier) ^ ApplyColour;

			var oldRect = CurrentRectangle;
			
			lines.Clear ();
			if (!Filled) {
				var templines = new ScanLines ();
				templines.AddRect (rect);
				ScanLinesDrawDelegate draw = (drawrect) => {
					lines.AddRect (drawrect);
				};
				templines.Outline (draw);
			} else
				lines.AddRect (rect);
			
			if (oldRect != null)
				rect = Rectangle.Union (rect, oldRect.Value);
			UpdatingCursor = true;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion (rect, false, false);
		}
		
		public override void Cancel ()
		{
			var rect = CurrentRectangle;
			base.Cancel ();
			if (rect != null)
				Handler.InvalidateCharacterRegion (rect.Value, false, false);
		}
		
		public override CanvasElement? GetElement (Point point, Canvas canvas)
		{
			var rect = CurrentRectangle;
			if (rect != null && lines.PointIsDrawn (point)) {
				
				var ce = canvas [point];
				
				if (shouldApplyCharacter)
					ce.Character = CurrentCharacter;
				
				if (shouldApplyColour)
					ce.Attribute = Handler.DrawAttribute;
				
				return ce;
			}
			return canvas [point];
		}
		
		Control ApplyColourButton ()
		{
			var control = new ImageButton{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Character.Icons.ApplyColour.png"),
				Toggle = true,
				Pressed = ApplyColour,
#if DESKTOP
				ToolTip = "Draw with color"
#endif
			};
			
			control.Click += delegate {
				ApplyColour = control.Pressed;
			};
			return control;
		}
		
		Control ApplyCharacterButton ()
		{
			var control = new ImageButton{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Text-Horizontal.png"),
				Toggle = true,
				Pressed = ApplyCharacter,
#if DESKTOP
				ToolTip = "Draw with character"
#endif
			};
			
			control.Click += delegate {
				ApplyCharacter = control.Pressed;
			};
			return control;
		}
	
		Control FilledButton ()
		{
			var control = new ImageButton{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.FilledRectangle.png"),
				Toggle = true,
				Pressed = Filled,
#if DESKTOP
				ToolTip = "Filled rectangle"
#endif
			};
			
			control.Click += delegate {
				Filled = control.Pressed;
			};
			return control;
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			
			layout.Add (Separator ());
			layout.Add (base.GeneratePad ());
			layout.BeginVertical (Padding.Empty, Eto.Drawing.Size.Empty);
			layout.AddRow (ApplyCharacterButton (), ApplyColourButton ());
			layout.AddRow (FilledButton (), null);
			layout.AddRow (null);
			layout.EndVertical ();
			
			layout.Add (new CharacterSelectPad (this));
			return layout;
		}
	}
}

