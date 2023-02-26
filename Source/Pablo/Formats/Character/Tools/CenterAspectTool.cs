using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.BGI;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public abstract class CenterAspectTool : TwoPointTool
	{
		Rectangle? currentRect;

		protected bool IsSquare { get; private set; }

		protected bool IsCentered { get; private set; }

		protected abstract void UpdateWithLocation(Rectangle rect, Keys modifiers, Point end);

		protected override Rectangle? CurrentRectangle
		{
			get
			{
				return currentRect ?? base.CurrentRectangle;
			}
		}

		protected Rectangle? ResolvedRectangle
		{
			get { return currentRect; }
			set { currentRect = value; }
		}

		public override void Cancel()
		{
			base.Cancel();
			currentRect = null;
		}

		protected override void Finish()
		{
			base.Finish();
			currentRect = null;
		}

		protected sealed override void Update(Point start, Point end, Keys modifiers, Point location)
		{
			var rect = new Rectangle();
			var aspect = Handler.CurrentPage.Font.Aspect / Handler.Aspect;
			if (HalfMode)
				aspect *= 2;
			var size = new Size(end - start);

			var shouldBeSquare = (!modifiers.HasFlag(PabloCommand.CommonModifier) && modifiers.HasFlag(Keys.Shift)) ^ IsSquare;
			var shouldBeCentered = modifiers.HasFlag(Keys.Alt) ^ IsCentered;


			if (shouldBeSquare)
			{
				int diameter = Math.Max(Math.Abs(size.Width), (int)Math.Round(Math.Abs(size.Height) / aspect));
				if (shouldBeCentered)
				{
					rect.Location = start - new Size(diameter * (size.Width >= 0 ? 1 : -1), (int)Math.Round(diameter * aspect) * (size.Height >= 0 ? 1 : -1));
					diameter *= 2;
				}
				else
				{
					rect.Location = start;
				}
				rect.Size = new Size(
					(diameter + ((size.Width >= 0) ? 1 : 0)) * (size.Width >= 0 ? 1 : -1),
					((int)Math.Round(diameter * aspect) + ((size.Height >= 0) ? 1 : 0)) * (size.Height >= 0 ? 1 : -1)
				);

			}
			else
			{
				if (shouldBeCentered)
				{
					rect.Location = start - size;
					size *= new Size(2, 2);
					if (size.Width >= 0)
						size.Width++;
					if (size.Height >= 0)
						size.Height++;
					rect.Size = size;
				}
				else
				{
					if (size.Width >= 0)
						size.Width++;
					if (size.Height >= 0)
						size.Height++;
					rect.Size = size;
					rect.Location = start;
				}
			}
			if (currentRect == null || currentRect.Value != rect)
			{
				UpdateWithLocation(rect, modifiers, end);
				currentRect = rect;
			}
			base.Update(start, end, modifiers, location);
		}

		Control SquareButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Square.ans"),
				Toggle = true,
				Pressed = IsSquare,
				ToolTip = "Square aspect (shift)"
			};

			control.Click += delegate
			{
				IsSquare = control.Pressed;
			};
			return control;
		}

		Control CenteredButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Centered.ans"),
				Toggle = true,
				Pressed = IsCentered,
				ToolTip = "Centered (alt)"
			};

			control.Click += delegate
			{
				IsCentered = control.Pressed;
			};
			return control;
		}

		public override Control GeneratePad()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };

			layout.BeginVertical(Padding.Empty, new Size(1, 1));
			layout.AddRow(SquareButton(), CenteredButton());
			layout.EndVertical();
			//layout.Add (base.GeneratePad ());

			return layout;
		}
	}
}

