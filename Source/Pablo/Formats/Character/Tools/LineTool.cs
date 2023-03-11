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
		ScanLines _lines = new ScanLines();
		Character _currentCharacter;

		public Character CurrentCharacter
		{
			get => _currentCharacter;
			set
			{
				_currentCharacter = value;
				
			}
		}

		bool shouldApplyCharacter;
		public bool ApplyCharacter { get; set; }

		bool shouldApplyColour;
		public bool ApplyColour { get; set; }

		public override CharacterDocument DocumentImage => ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Line.ans");

		public override Cursor MouseCursor => new Cursor(CursorType.Crosshair);

		public override string Description => "Line - Draw a line";

		public override Keys Accelerator => Keys.L | (Handler.Generator.IsMac ? Keys.Control : Keys.Alt);

		public LineTool()
		{
			ApplyColour = true;
			ApplyCharacter = true;
			HalfMode = true;
			CurrentCharacter = 219;
		}

		protected override bool Apply()
		{
			var rect = CurrentRectangle;
			if (rect != null)
			{
				// execute the command!
				var action = new Actions.Drawing.DrawLine(Handler);
				action.Start = rect.Value.Location;
				action.End = rect.Value.EndLocation;
				action.Element = new CanvasElement(CurrentCharacter, Handler.DrawAttribute);
				action.ApplyColour = shouldApplyColour;
				action.ApplyCharacter = shouldApplyCharacter;
				action.HalfMode = HalfMode;
				action.Execute();
				return true;
			}
			return false;
		}

		public override void Cancel()
		{
			var rect = CurrentRectangle;
			base.Cancel();
			if (rect != null)
				Handler.InvalidateCharacterRegion(rect.Value, false, false, HalfMode);
		}

		protected override void Finish()
		{
			var rect = CurrentRectangle;
			base.Finish();
			if (rect != null)
				Handler.InvalidateCharacterRegion(rect.Value, false, false, HalfMode);
		}

		protected override void UpdateWithLocation(Rectangle rect, Keys modifiers, Point end)
		{
			shouldApplyCharacter = ApplyCharacter; //modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ ApplyCharacter;
												   //applyInverted = modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ Inverted;
			shouldApplyColour = ApplyColour; //modifiers.HasFlag (Key.Alt | Application.Instance.CommonModifier) ^ ApplyColour;

			var oldRect = CurrentRectangle;
			_lines.Clear();
			_lines.AddLine(rect.Location, rect.EndLocation);

			if (oldRect != null)
				rect = Rectangle.Union(rect, oldRect.Value);
			UpdatingCursor = true;
			if (HalfMode)
				end.Y = end.Y /= 2;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion(rect, false, false, HalfMode);
		}

		public override CanvasElement? GetElement(Point point, Canvas canvas)
		{
			var rect = CurrentRectangle;
			if (rect != null)
			{
				if (HalfMode)
				{
					var halfPoint1 = point;
					halfPoint1.Y *= 2;
					var halfPoint2 = halfPoint1;
					halfPoint2.Y += 1;
					var halfpoint1Inside = _lines.PointIsInside(halfPoint1);
					var halfpoint2Inside = _lines.PointIsInside(halfPoint2);
					if (halfpoint1Inside && halfpoint2Inside)
					{
						var ce = canvas[point];
						ce.Character = 219;
						ce.Foreground = Handler.DrawAttribute.Foreground;
						return ce;
					}
					if (halfpoint1Inside)
					{
						var ce = canvas[point];
						return MemoryCanvas.UpdateHalfChar(Handler.DrawAttribute.Foreground, true, ce);
					}
					if (halfpoint2Inside)
					{
						var ce = canvas[point];
						return MemoryCanvas.UpdateHalfChar(Handler.DrawAttribute.Foreground, false, ce);
					}
				}
				else
				{

					if (_lines.PointIsInside(point))
					{
						var ce = canvas[point];
						if (shouldApplyColour)
							ce.Attribute = Handler.DrawAttribute;
						if (shouldApplyCharacter)
							ce.Character = CurrentCharacter;
						return ce;
					}
				}
			}
			/*
			if (rect != null && rect.Value.Contains (point)) {
				var ce = canvas [point];
				ce.Attribute = new Attribute (1, 2);
				return ce;
			}*/
			return canvas[point];
		}

		Control ApplyColourButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.ApplyColour.ans", false),
				Toggle = true,
				Pressed = ApplyColour,
				ToolTip = "Draw with color"
			};

			control.Click += delegate
			{
				ApplyColour = control.Pressed;
			};
			return control;
		}

		Control ApplyCharacterButton()
		{
			var control = new ImageButton
			{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Rip.Icons.Text-Horizontal.png"),
				Toggle = true,
				Pressed = ApplyCharacter,
#if DESKTOP
				ToolTip = "Draw with character"
#endif
			};

			control.Click += delegate
			{
				ApplyCharacter = control.Pressed;
			};
			return control;
		}

		public override Control GeneratePad()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = new Size(1, 1) };

			layout.Add(base.GeneratePad());
			layout.BeginVertical(Padding.Empty, new Size(1, 1));
			layout.AddRow(ApplyCharacterButton(), ApplyColourButton());
			layout.AddRow(HalfModeButton());
			layout.EndVertical();

			//layout.Add (new Controls.SizePad (this));
			layout.Add(new CharacterSelectPad(this));
			return layout;
		}

	}
}

