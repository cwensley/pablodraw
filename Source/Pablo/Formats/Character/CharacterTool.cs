using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Controls;

namespace Pablo.Formats.Character
{
	public abstract class CharacterTool
	{
		public virtual Eto.Drawing.Image Image { get; }
		public virtual CharacterDocument DocumentImage { get; }

		public virtual CharacterHandler Handler { get; set; }

		public virtual Keys Accelerator { get { return Keys.None; } }

		public virtual bool AllowToolShortcuts { get { return true; } }

		public virtual bool AllowKeyboard { get { return true; } }

		public virtual Cursor MouseCursor { get { return null; } }

		public abstract string Description { get; }

		public object Tag { get; set; }

		public CharacterDocument Document
		{
			get { return Handler.CharacterDocument; }
		}

		public virtual IEnumerable<Pablo.Network.ICommand> Commands
		{
			get { yield break; }
		}

		public virtual void Unselected()
		{

		}

		public virtual void Selecting()
		{
			if (Handler.HasViewerControl)
			{
				var cursor = this.MouseCursor;
#if DESKTOP
				if (cursor != null)
					Handler.ViewerControl.Cursor = cursor;
#endif
			}
		}

		protected virtual Point GetLocation(Point location)
		{
			return Handler.ScreenToCharacter(location, HalfMode);
		}

		public virtual void OnSetCursorPosition(Point old, Point cursorPosition, bool invalidate)
		{
			if (old.Y == cursorPosition.Y && Math.Abs(old.X - cursorPosition.X) <= 1)
			{
				Handler.InvalidateCharacterRegion(new Rectangle(Math.Min(old.X, cursorPosition.X), old.Y, 2, 1), false);
			}
			else
			{
				Handler.InvalidateCharacterRegion(new Rectangle(old, new Size(1, 1)), false);
				Handler.InvalidateCharacterRegion(new Rectangle(cursorPosition, new Size(1, 1)), false);
			}
		}

		bool _halfMode;

		public bool HalfMode
		{
			get => _halfMode;
			set
			{
				if (_halfMode != value)
				{
					_halfMode = value;
					HalfModeChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler<EventArgs> HalfModeChanged;

		public void UpdateCursorPosition(Point newCursor, Rectangle excludeRectangle)
		{
			var oldCursor = Handler.CursorPosition;
			if (HalfMode)
			{
				newCursor.Y /= 2;
				excludeRectangle = excludeRectangle.FromHalfMode();
			}
			Handler.SetCursorPosition(newCursor, false);
			if (!excludeRectangle.Contains(oldCursor))
				Handler.InvalidateCharacterRegion(new Rectangle(oldCursor, new Size(1, 1)), false, false);

		}

		public virtual void OnMouseDown(MouseEventArgs e)
		{
		}

		public virtual void OnMouseUp(MouseEventArgs e)
		{
		}

		public virtual void OnMouseMove(MouseEventArgs e)
		{
		}

		public virtual void OnKeyDown(KeyEventArgs e)
		{
		}

		public virtual void DeleteLine(int y)
		{
		}

		public virtual void InsertLine(int y)
		{
		}

		public virtual void GenerateCommands(GenerateCommandArgs args)
		{
		}
		
		protected Control HalfModeButton()
		{
			var control = new AnsiButton
			{
				Document = ImageCache.CharacterFromResource("Pablo.Formats.Character.Icons.HalfMode.ans"),
				Toggle = true,
				Pressed = HalfMode,
				ToolTip = "Draw with half blocks"
			};

			control.Bind(c => c.Pressed, this, c => c.HalfMode);

			return control;
		}


		public virtual Control GeneratePad()
		{
			return null;
		}

		public virtual IGenerateRegion GetGenerator()
		{
			return null;
		}
	}
}

