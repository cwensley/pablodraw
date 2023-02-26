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
		ScanLines lines = new ScanLines();
		Rectangle? currentRect;

		public override CharacterDocument DocumentImage => ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Ellipse.ans");

		public override string Description => "Circle - Draw an ellipse or circle";

		public override Keys Accelerator => Keys.O | (Handler.Generator.IsMac ? Keys.Control : Keys.Alt);

		public Character CurrentCharacter { get; set; }

		bool shouldApplyCharacter;

		public bool ApplyCharacter { get; set; }

		bool shouldApplyColour;

		public bool ApplyColour { get; set; }

		public bool Filled { get; set; }

		public override Cursor MouseCursor => new Cursor(CursorType.Crosshair);

		public EllipseTool()
		{
			ApplyColour = true;
			ApplyCharacter = true;
			CurrentCharacter = 219;
			HalfMode = true;
		}

		protected override bool Apply()
		{
			var rect = currentRect;
			if (rect != null)
			{
				// execute the command!
				var action = new Actions.Drawing.DrawEllipse(Handler)
				{
					Rectangle = rect.Value,
					Element = new CanvasElement(CurrentCharacter, Handler.DrawAttribute),
					ApplyColour = shouldApplyColour,
					ApplyCharacter = shouldApplyCharacter,
					HalfMode = HalfMode,
					Filled = Filled
				};
				action.Execute();
				return true;
			}
			return false;
		}

		public override void Cancel()
		{
			var rect = currentRect;
			base.Cancel();
			currentRect = null;
			if (rect != null)
				Handler.InvalidateCharacterRegion(rect.Value, false, false, HalfMode);
		}

		protected override void Finish()
		{
			var rect = currentRect;
			base.Finish();
			currentRect = null;
			if (rect != null)
				Handler.InvalidateCharacterRegion(rect.Value, false, false, HalfMode);
		}

		protected override void UpdateWithLocation(Rectangle rect, Keys modifiers, Point end)
		{
			shouldApplyCharacter = ApplyCharacter; //modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ ApplyCharacter;
			shouldApplyColour = ApplyColour; //modifiers.HasFlag (Key.Alt | Application.Instance.CommonModifier) ^ ApplyColour;

			var oldRect = currentRect;

			rect.Normalize();
			lines.Clear();
			if (!Filled)
			{
				var templines = new ScanLines();
				currentRect = rect = templines.AddEllipse(rect);
				void Draw(Rectangle drawrect) => lines.AddRect(drawrect);
				templines.Outline(Draw);
			}
			else
				currentRect = rect = lines.AddEllipse(rect);

			if (oldRect != null)
				rect = Rectangle.Union(rect, oldRect.Value);
			UpdatingCursor = true;
			if (HalfMode)
				end.Y /= 2;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion(rect, false, false, HalfMode);
		}

		public override CanvasElement? GetElement(Point point, Canvas canvas)
		{
			var rect = currentRect;
			if (rect != null)
			{
				if (HalfMode)
				{
					var halfPoint = point;
					halfPoint.Y *= 2;
					var halfpoint1Inside = lines.PointIsDrawn(halfPoint);
					var halfpoint2Inside = lines.PointIsDrawn(halfPoint + new Size(0, 1));
					if (halfpoint1Inside || halfpoint2Inside)
					{
						var ce = canvas[point];
						
						if (halfpoint1Inside && halfpoint2Inside)
						{
							ce.Character = 219;
							ce.Foreground = Handler.DrawAttribute.Foreground;
							return ce;
						}
						return MemoryCanvas.UpdateHalfChar(Handler.DrawAttribute.Foreground, halfpoint1Inside, ce);
					}
				}
				else if ((Filled && lines.PointIsInside(point)) || (!Filled && lines.PointIsDrawn(point)))
				{
					var ce = canvas[point];
					if (shouldApplyColour)
						ce.Attribute = Handler.DrawAttribute;
					if (shouldApplyCharacter)
						ce.Character = CurrentCharacter;
					return ce;
				}
			}
			return canvas[point];
		}

		Control ApplyColourButton()
		{
			var control = new ImageButton
			{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Character.Icons.ApplyColour.png"),
				Toggle = true,
				Pressed = ApplyColour,
#if DESKTOP
				ToolTip = "Draw with color"
#endif
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

		Control FilledButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.EllipseFill.ans"),
				Toggle = true,
				Pressed = Filled,
				ToolTip = "Filled ellipse"
			};

			control.Click += delegate
			{
				Filled = control.Pressed;
			};
			return control;
		}

		public override Control GeneratePad()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty, Spacing = new Size(1, 1) };

			layout.Add(base.GeneratePad());
			layout.BeginVertical(Padding.Empty, new Size(1, 1));
			layout.AddRow(ApplyCharacterButton(), ApplyColourButton());
			layout.AddRow(FilledButton(), HalfModeButton());
			layout.AddRow(null);
			layout.EndVertical();

			//layout.Add (new Controls.SizePad (this));
			layout.Add(new CharacterSelectPad(this));
			return layout;
		}

	}
}

