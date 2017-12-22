using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public class Brush : SizeTool
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

		public override Eto.Drawing.Image Image
		{
			get { return Bitmap.FromResource("Pablo.Formats.Rip.Icons.Brush.png"); }
		}

		public override string Description
		{
			get { return "Brush - Paint with character gradients"; }
		}

		public override Key Accelerator
		{
			get
			{
				return (Handler.Generator.IsMac ? Key.Control : Key.Alt) | Key.B;
			}
		}

		public Brush()
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
				action.Inverse = e.Modifiers.HasFlag(Key.Shift) ^ Inverted;
				action.ApplyColour = e.Modifiers.HasFlag(Key.Alt) ^ ApplyColour;
				action.Activate();
				
				var middle = (Size - 1) / 2;
				UpdateCursorPosition(new Point(location.X + middle, location.Y + middle), new Rectangle(location, new Size(Size, Size)));
			}
		}

		Control InvertButton()
		{
			var control = new ImageButton
			{
				Image = Bitmap.FromResource("Pablo.Formats.Character.Icons.Erase.png"),
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
				Image = Bitmap.FromResource("Pablo.Formats.Character.Icons.ApplyColour.png"),
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
			var layout = new DynamicLayout(Padding.Empty);
			
			layout.Add(Separator());
			layout.BeginVertical(Padding.Empty, Eto.Drawing.Size.Empty);
			layout.AddRow(InvertButton(), ColourButton());
			layout.EndVertical();
			
			layout.Add(base.GeneratePad());
			layout.Add(new Controls.BrushPad(this));
			return layout;
		}
	}
}

