using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Controls;
using Pablo.Formats.Character.Controls;
using Pablo.Drawing;

namespace Pablo.Formats.Character.Tools
{
	public class EllipseTool : CenterAspectTool, ICharacterSelectSource
	{
		ScanLines lines = new ScanLines ();
		Rectangle? currentRect;
		
		public override Eto.Drawing.Image Image {
			get { return Eto.Drawing.Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Oval.png"); }
		}

		public override string Description {
			get { return "Circle - Draw an ellipse or circle"; }
		}
		
		public override Key Accelerator {
			get {
				return Key.O | (Handler.Generator.IsMac ? Key.Control : Key.Alt);
			}
		}
		
		public Character CurrentCharacter { get; set; }
		
		bool shouldApplyCharacter;

		public bool ApplyCharacter { get; set; }
		
		bool shouldApplyColour;

		public bool ApplyColour { get; set; }
		
		public bool Filled { get; set; }
		
		public override Cursor MouseCursor {
			get { return new Cursor (CursorType.Crosshair); }
		}
		
		public EllipseTool ()
		{
			ApplyColour = true;
			ApplyCharacter = true;
			CurrentCharacter = 219;
		}
		
		protected override bool Apply ()
		{
			var rect = currentRect;
			if (rect != null) {
				// execute the command!
				var action = new Actions.Drawing.DrawEllipse (Handler) {
					Rectangle = rect.Value,
					Element = new CanvasElement (CurrentCharacter, Handler.DrawAttribute),
					ApplyColour = shouldApplyColour,
					ApplyCharacter = shouldApplyCharacter,
					Filled = Filled
				};
				action.Activate ();
				return true;
			}
			return false;
		}
		
		public override void Cancel ()
		{
			var rect = currentRect;
			base.Cancel ();
			currentRect = null;
			if (rect != null)
				Handler.InvalidateCharacterRegion (rect.Value, false, false);
		}
		
		protected override void Finish ()
		{
			var rect = currentRect;
			base.Finish ();
			currentRect = null;
			if (rect != null)
				Handler.InvalidateCharacterRegion (rect.Value, false, false);
		}
		
		protected override void UpdateWithLocation (Rectangle rect, Key modifiers, Point end)
		{
			shouldApplyCharacter = ApplyCharacter; //modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ ApplyCharacter;
			shouldApplyColour = ApplyColour; //modifiers.HasFlag (Key.Alt | Application.Instance.CommonModifier) ^ ApplyColour;
			
			var oldRect = currentRect;
			
			rect.Normalize ();
			lines.Clear ();
			if (!Filled) {
				var templines = new ScanLines ();
				currentRect = rect = templines.AddEllipse (rect);
				ScanLinesDrawDelegate draw = (drawrect) => {
						lines.AddRect (drawrect);
					};
				templines.Outline (draw);
			} else
				currentRect = rect = lines.AddEllipse (rect);

			if (oldRect != null)
				rect = Rectangle.Union (rect, oldRect.Value);
			UpdatingCursor = true;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion (rect, false, false);
		}
		
		public override CanvasElement? GetElement (Point point, Canvas canvas)
		{
			var rect = currentRect;
			if (rect != null) {
				if ((Filled && lines.PointIsInside (point)) || (!Filled && lines.PointIsDrawn (point))) {
					var ce = canvas [point];
					if (shouldApplyColour)
						ce.Attribute = Handler.DrawAttribute;
					if (shouldApplyCharacter)
						ce.Character = CurrentCharacter;
					return ce;
				}
			}
			return canvas [point];
		}
		
		Control ApplyColourButton ()
		{
			var control = new ImageButton{
				Image = Bitmap.FromResource ("Pablo.Formats.Character.Icons.ApplyColour.png"),
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
				Image = Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Text-Horizontal.png"),
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
				Image = Bitmap.FromResource ("Pablo.Formats.Rip.Icons.FilledOval.png"),
				Toggle = true,
				Pressed = Filled,
#if DESKTOP
				ToolTip = "Filled ellipse"
#endif
			};
			
			control.Click += delegate {
				Filled = control.Pressed;
			};
			return control;
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			
			layout.Add (Separator ());
			layout.Add (base.GeneratePad ());
			layout.BeginVertical (Padding.Empty, Eto.Drawing.Size.Empty);
			layout.AddRow (ApplyCharacterButton (), ApplyColourButton ());
			layout.AddRow (FilledButton (), null);
			layout.AddRow (null);
			layout.EndVertical ();
			
			//layout.Add (new Controls.SizePad (this));
			layout.Add (new CharacterSelectPad (this));
			return layout;
		}
		
	}
}

