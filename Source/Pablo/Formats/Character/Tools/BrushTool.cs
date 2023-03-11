using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public class BrushTool : SizeTool
	{
		Actions.Drawing.Brush action;
		int currentBrushIndex;

		public event EventHandler<EventArgs> CurrentBrushChanged;

		protected virtual void OnCurrentBrushChanged(EventArgs e)
		{
			if (CurrentBrushChanged != null)
				CurrentBrushChanged(this, e);
		}

		public BrushInfo CurrentBrush
		{
			get
			{ 
				return Handler.Info.Brushes[CurrentBrushIndex];
			}
		}

		public int CurrentBrushIndex
		{
			get { return Math.Max(0, Math.Min(currentBrushIndex, Handler.Info.Brushes.Length)); }
			set
			{
				if (currentBrushIndex != value)
				{
					currentBrushIndex = Math.Max(0, Math.Min(value, Handler.Info.Brushes.Length));
					OnCurrentBrushChanged(EventArgs.Empty);
				}
			}
		}

		public override Cursor MouseCursor
		{
			get { return new Cursor(CursorType.Crosshair); }
		}

		public bool Inverted { get; set; }

		public bool ApplyColour { get; set; }

		public override CharacterHandler Handler
		{
			get { return base.Handler; }
			set
			{
				base.Handler = value;
				action = new Actions.Drawing.Brush(value);
			}
		}

		public override CharacterDocument DocumentImage => ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Brush.ans");

		public override string Description => "Brush - Paint with character gradients";

		public override Keys Accelerator => (Handler.Generator.IsMac ? Keys.Control : Keys.Alt) | Keys.B;

		public BrushTool()
		{
			ApplyColour = true;
		}

		protected override void Draw(Point location, MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary)
			{
				action.Location = location;
				action.DrawBrush = CurrentBrush;
				action.Size = Size;
				action.Inverse = e.Modifiers.HasFlag(Keys.Shift) ^ Inverted;
				action.ApplyColour = e.Modifiers.HasFlag(Keys.Alt) ^ ApplyColour;
				action.Execute();
				
				var middle = (Size - 1) / 2;
				UpdateCursorPosition(new Point(location.X + middle, location.Y + middle), new Rectangle(location, new Size(Size, Size)));
			}
		}

		Control InvertButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.Erase.ans", false),
				Toggle = true,
				Pressed = Inverted,
				ToolTip = "Erase mode (shift)"
			};
			
			control.Click += delegate
			{
				Inverted = control.Pressed;
			};
			return control;
		}

		Control ColourButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.ApplyColour.ans", false),
				Toggle = true,
				Pressed = ApplyColour,
				ToolTip = "Draw with color (alt)"
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
			layout.EndVertical();
			
			layout.Add(base.GeneratePad());
			layout.Add(new Controls.BrushPad(this));
			return layout;
		}
	}
}

