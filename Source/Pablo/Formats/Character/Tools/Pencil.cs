using System;
using System.Reflection;
using Eto.Drawing;
using Pablo.Formats.Character.Actions.Block;
using Pablo.Formats.Character.Controls;
using Eto.Forms;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public class Pencil : SizeTool, ICharacterSelectSource
	{
		public bool Inverted { get; set; }

		public bool ApplyColour { get; set; }

		public Character CurrentCharacter { get; set; }

		public override Eto.Drawing.Image Image
		{
			get { return Eto.Drawing.Bitmap.FromResource("Pablo.Formats.Rip.Icons.Pixel.png"); }
		}

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

		public Pencil()
		{
			ApplyColour = true;
			CurrentCharacter = 177; // shaded character
		}

		protected override void Draw(Point location, Eto.Forms.MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary)
			{
				var action = new Actions.Block.Fill(Handler);
				var rect = new Rectangle(location, new Size(this.Size, this.Size));
				var attribs = action.Attributes = new FillAttributes
				{
					Rectangle = rect,
					Mode = Controls.FillMode.Character
				};
				
				var inverted = e.Modifiers.HasFlag(Keys.Shift) ^ Inverted;
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
			
			layout.Add(Separator());
			layout.BeginVertical(Padding.Empty, Eto.Drawing.Size.Empty);
			layout.AddRow(InvertButton(), ColourButton());
			layout.EndVertical();
			
			layout.Add(base.GeneratePad());
			layout.Add(new CharacterSelectPad(this));
			return layout;
		}
	}
}

