using System;
using System.Reflection;
using Eto.Drawing;
using Pablo.Formats.Character.Controls;
using Eto.Forms;
using Pablo.Controls;
using Pablo.Formats.Character.Actions.Drawing;
using Pablo.Formats.Character.Actions.Block;

namespace Pablo.Formats.Character.Tools
{
	public class PencilTool : SizeTool, ICharacterSelectSource
	{
		public bool Inverted { get; set; }

		public bool ApplyColour { get; set; }

		public Character CurrentCharacter { get; set; }

		public override CharacterDocument DocumentImage => ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Pencil.ans", false);

		public override string Description
		{
			get { return "Pencil - Paint with a single character"; }
		}

		public override Keys Accelerator
		{
			get
			{
				return Keys.P | (Handler.Generator.IsMac ? Keys.Control : Keys.Alt);
			}
		}

		public override Cursor MouseCursor
		{
			get { return new Cursor(CursorType.Crosshair); }
		}

		public PencilTool()
		{
			ApplyColour = true;
			CurrentCharacter = 177; // shaded character
			HalfMode = true;
			Size = 2;
		}

		protected override void Draw(Point location, Eto.Forms.MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary)
			{
				var inverted = e.Modifiers.HasFlag(Keys.Shift) ^ Inverted;

				if (HalfMode)
				{
					// half mode!
					var action = new HalfFill(Handler);
					var rect = new Rectangle(location, new Size(Size, Size));
					// Console.WriteLine($"Location: {location}");
					action.Attributes = new HalfFillAttributes
					{
						HalfRectangle = rect,
						Color = Handler.DrawAttribute,
						Invert = inverted
					};

					action.Execute();
					
					var middle = (Size - 1) / 2;
				
					UpdateCursorPosition(new Point(location.X + middle, location.Y + middle), rect);
				}
				else
				{
					var action = new Fill(Handler);
					var rect = new Rectangle(location, new Size(this.Size, this.Size));
					var attribs = action.Attributes = new FillAttributes
					{
						Rectangle = rect,
						Mode = Controls.FillMode.Character
					};

					attribs.Character = new Character((inverted) ? (Character)32 : CurrentCharacter);

					if (e.Modifiers.HasFlag(Keys.Alt) ^ ApplyColour)
					{
						attribs.Mode |= Controls.FillMode.Attribute;
						attribs.Attribute = Handler.DrawAttribute;
					}
					action.Execute();
					
					var middle = (Size - 1) / 2;
				
					UpdateCursorPosition(new Point(location.X + middle, location.Y + middle), rect);
				}
			}
		}

		Control InvertButton()
		{
			var control = new ImageButton
			{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Character.Icons.Erase.png"),
				Toggle = true,
				Pressed = Inverted,
#if DESKTOP
				ToolTip = "Erase mode (shift)"
#endif
			};
			
			control.Click += delegate
			{
				Inverted = control.Pressed;
			};
			return control;
		}

		Control ColourButton()
		{
			var control = new ImageButton
			{
				Image = ImageCache.BitmapFromResource("Pablo.Formats.Character.Icons.ApplyColour.png"),
				Toggle = true,
				Pressed = ApplyColour,
#if DESKTOP
				ToolTip = "Draw with color (alt)"
#endif
			};
			
			control.Click += delegate
			{
				ApplyColour = control.Pressed;
			};
			return control;
		}

		public override Control GeneratePad()
		{
			var layout = new DynamicLayout { Padding = Padding.Empty };
			
			layout.BeginVertical(Padding.Empty, new Size(1, 1));
			layout.AddRow(InvertButton(), ColourButton());
			layout.AddRow(HalfModeButton());
			layout.EndVertical();
			
			layout.Add(base.GeneratePad());
			layout.Add(new CharacterSelectPad(this));
			return layout;
		}
	}
}

