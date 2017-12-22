using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Drawing;
using Pablo.Formats.Character.Controls;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public class LineTool : CenterAspectTool, ICharacterSelectSource
	{
		ScanLines lines = new ScanLines();
		
		public Character CurrentCharacter { get; set; }
		
		bool shouldApplyCharacter;
		public bool ApplyCharacter { get; set; }
		
		bool shouldApplyColour;
		public bool ApplyColour { get; set; }
		
		public override Eto.Drawing.Image Image {
			get { return Eto.Drawing.Bitmap.FromResource ("Pablo.Formats.Rip.Icons.Line.png"); }
		}

		public override Cursor MouseCursor {
			get { return new Cursor (CursorType.Crosshair); }
		}
		
		public override string Description {
			get { return "Line - Draw a line"; }
		}
		
		public override Key Accelerator {
			get {
				return Key.L | (Handler.Generator.IsMac ? Key.Control : Key.Alt);
			}
		}
		
		public LineTool()
		{
			this.ApplyColour = true;
			this.ApplyCharacter = true;
			CurrentCharacter = 219;
		}
		
		protected override bool Apply ()
		{
			var rect = CurrentRectangle;
			if (rect != null) {
				// execute the command!
				var action = new Actions.Drawing.DrawLine(Handler);
				action.Start = rect.Value.Location;
				action.End = rect.Value.EndLocation;
				action.Element = new CanvasElement(CurrentCharacter, Handler.DrawAttribute);
				action.ApplyColour = shouldApplyColour;
				action.ApplyCharacter = shouldApplyCharacter;
				action.Activate ();
				return true;
			}
			return false;
		}
		
		public override void Cancel ()
		{
			var rect = CurrentRectangle;
			base.Cancel ();
			if (rect != null)
				Handler.InvalidateCharacterRegion (rect.Value, false, false);
		}
		
		protected override void Finish ()
		{
			var rect = CurrentRectangle;
			base.Finish ();
			if (rect != null)
				Handler.InvalidateCharacterRegion (rect.Value, false, false);
		}
		
		protected override void UpdateWithLocation (Rectangle rect, Key modifiers, Point end)
		{
			shouldApplyCharacter = ApplyCharacter; //modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ ApplyCharacter;
			//applyInverted = modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ Inverted;
			shouldApplyColour = ApplyColour; //modifiers.HasFlag (Key.Alt | Application.Instance.CommonModifier) ^ ApplyColour;

			var oldRect = CurrentRectangle;
			lines.Clear ();
			lines.AddLine (rect.Location, rect.EndLocation);
			
			if (oldRect != null)
				rect = Rectangle.Union (rect, oldRect.Value);
			UpdatingCursor = true;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion (rect, false, false);
		}
		
		public override CanvasElement? GetElement (Point point, Canvas canvas)
		{
			var rect = CurrentRectangle;
			if (rect != null) {
				if (lines.PointIsInside(point)) {
					var ce = canvas[point];
					if (shouldApplyColour)
						ce.Attribute = Handler.DrawAttribute;
					if (shouldApplyCharacter)
						ce.Character = CurrentCharacter;
					return ce;
				}
			}
			/*
			if (rect != null && rect.Value.Contains (point)) {
				var ce = canvas [point];
				ce.Attribute = new Attribute (1, 2);
				return ce;
			}*/
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
	
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			
			layout.Add (Separator ());
			layout.Add (base.GeneratePad ());
			layout.BeginVertical (Padding.Empty, Eto.Drawing.Size.Empty);
			layout.AddRow (ApplyCharacterButton (), ApplyColourButton ());
			layout.EndVertical ();
			
			//layout.Add (new Controls.SizePad (this));
			layout.Add (new CharacterSelectPad (this));
			return layout;
		}
		
	}
}

