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
		ScanLines lines = new ScanLines();

		public override CharacterDocument DocumentImage => ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Rectangle.ans");

		public override string Description => "Rectangle - Draw a box";

		public override Keys Accelerator => Keys.R | (Handler.Generator.IsMac ? Keys.Control : Keys.Alt);

		public override Cursor MouseCursor => new Cursor(CursorType.Crosshair);

		public Character CurrentCharacter { get; set; }

		bool shouldApplyCharacter;

		public bool ApplyCharacter { get; set; }

		bool shouldApplyColour;

		public bool ApplyColour { get; set; }

		public bool Filled { get; set; }

		public RectangleTool()
		{
			ApplyColour = true;
			ApplyCharacter = true;
			CurrentCharacter = 219;
			HalfMode = true;
		}

		protected override bool Apply()
		{
			var rect = CurrentRectangle;
			if (rect != null)
			{
				// execute the command!
				var action = new Actions.Drawing.DrawRect(Handler)
				{
					Rectangle = rect.Value,
					ApplyColour = shouldApplyColour,
					ApplyCharacter = shouldApplyCharacter,
					Filled = this.Filled,
					HalfMode = HalfMode,
					Element = new CanvasElement(CurrentCharacter, Handler.DrawAttribute)
				};
				action.Execute();
				return true;
			}
			return false;
		}

		protected override void UpdateWithLocation(Rectangle rect, Keys modifiers, Point end)
		{
			shouldApplyCharacter = ApplyCharacter; //modifiers.HasFlag (Key.Shift | Application.Instance.CommonModifier) ^ ApplyCharacter;
			shouldApplyColour = ApplyColour; //modifiers.HasFlag (Key.Alt | Application.Instance.CommonModifier) ^ ApplyColour;

			var oldRect = CurrentRectangle;

			lines.Clear();
			if (!Filled)
			{
				var templines = new ScanLines();
				templines.AddRect(rect);
				
				void Draw(Rectangle drawrect) => lines.AddRect(drawrect);
				templines.Outline(Draw);
			}
			else
				lines.AddRect(rect);

			if (oldRect != null)
				rect = Rectangle.Union(rect, oldRect.Value);
			UpdatingCursor = true;
			if (HalfMode)
				end.Y /= 2;
			Handler.CursorPosition = end;
			UpdatingCursor = false;
			Handler.InvalidateCharacterRegion(rect, false, false, HalfMode);
		}

		public override void Cancel()
		{
			var rect = CurrentRectangle;
			base.Cancel();
			if (rect != null)
				Handler.InvalidateCharacterRegion(rect.Value, false, false, HalfMode);
		}

		public override CanvasElement? GetElement(Point point, Canvas canvas)
		{
			var rect = CurrentRectangle;
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
				else if (lines.PointIsDrawn(point))
				{
					var ce = canvas[point];
					if (shouldApplyCharacter)
						ce.Character = CurrentCharacter;

					if (shouldApplyColour)
						ce.Attribute = Handler.DrawAttribute;
					return ce;
				}

			}
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

		Control FilledButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.RectangleFill.ans"),
				Toggle = true,
				Pressed = Filled,
				ToolTip = "Filled rectangle"
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

			layout.Add(new CharacterSelectPad(this));
			return layout;
		}
	}
}

