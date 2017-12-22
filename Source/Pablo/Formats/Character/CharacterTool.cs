using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Character
{
	public abstract class CharacterTool
	{
		public abstract Eto.Drawing.Image Image { get; }

		public virtual CharacterHandler Handler { get; set; }

		public virtual Key Accelerator { get { return Key.None; } }

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
			return Handler.ScreenToCharacter(location);
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

		public void UpdateCursorPosition(Point newCursor, Rectangle excludeRectangle)
		{
			var oldCursor = Handler.CursorPosition;
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

		public virtual void GenerateActions(GenerateActionArgs args)
		{
		}

		protected Control Separator()
		{
			var control = new Drawable
			{
				Size = new Size(1, 2)
			};

			control.Paint += (sender, pe) =>
			{
				pe.Graphics.FillRectangle(Colors.White, 0, 0, control.Size.Width, 0);
				pe.Graphics.DrawInsetRectangle(Colors.Gray, Colors.White, new Rectangle(0, 0, control.Size.Width, 2));
			};
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

