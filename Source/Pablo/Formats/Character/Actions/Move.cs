using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.Network;

namespace Pablo.Formats.Character.Actions
{
	public enum MoveDirection
	{
		Up,
		Down,
		Left,
		Right,
		PageUp,
		PageDown,
		First,
		Last,
		Top,
		Bottom
	}

	public class Move : Command
	{
		MoveDirection direction;

		public Move(Handler handler, string id, string name, string text, MoveDirection direction, params Key[] accelerators) : base(handler)
		{
			this.ID = "character_move" + id;
			this.Text = text;
			this.Name = name;
			this.Accelerators = (Key[])accelerators.Clone();
			this.direction = direction;
		}
		
		public override int CommandID {
			get { return (int)NetCommands.Move; }
		}
		
		public override UserLevel Level {
			get { return UserLevel.Viewer; }
		}

		public override bool Send (SendCommandArgs args)
		{
			Execute(new CommandExecuteArgs());
			base.Send (args);
			return true;
		}
		
		protected override void Execute(CommandExecuteArgs args)
		{
			CharacterHandler handler = Handler as CharacterHandler;
			if (handler != null)
			{
				Canvas canvas = handler.CurrentPage.Canvas;
				Point pos = handler.CursorPosition;
				switch (direction)
				{
					case MoveDirection.Up: MoveTo(pos.X, pos.Y-1); break;
					case MoveDirection.Down: MoveTo(pos.X, pos.Y+1); break;
					case MoveDirection.Left: MoveTo(pos.X-1, pos.Y); break;
					case MoveDirection.Right: MoveTo(pos.X+1, pos.Y); break;
					case MoveDirection.PageUp: MoveVertical(pos, -1); break;
					case MoveDirection.PageDown: MoveVertical(pos, 1); break;
					case MoveDirection.First: MoveTo(0, pos.Y); break;
					case MoveDirection.Last: MoveTo(canvas.Width-1, pos.Y); break;
					case MoveDirection.Top: MoveTo(pos.X, 0); break;
					case MoveDirection.Bottom: MoveTo(pos.X, canvas.Height-1); break;
				}
				
				handler.EnsureVisible(handler.CursorPosition);
			}
		}
		
		private void MoveTo(int x, int y)
		{
			CharacterHandler handler = Handler as CharacterHandler;
			Point pos = handler.CursorPosition;
			var newPos = new Point(x, y);
			newPos.Restrict (new Rectangle(handler.CurrentPage.Canvas.Size));
			
			if (pos != newPos)
			{
				handler.CursorPosition = newPos;
			}
		}
		
		private void MoveVertical(Point pos, int ydir)
		{
			CharacterHandler handler = Handler as CharacterHandler;
			IViewer viewer = handler.Viewer;
			if (viewer != null)
			{
				Canvas canvas = handler.CurrentPage.Canvas;
				BitFont font = handler.CurrentPage.Font;

				Point newCursor = pos;
				float fontHeight = ((float)font.Height) * handler.ZoomRatio.Height;
				int verticalOffset = (int)((viewer.ViewSize.Height + fontHeight -1) / fontHeight);
				newCursor.Y += verticalOffset * ydir;
				if (newCursor.Y > canvas.Height-1) newCursor.Y = canvas.Height-1;
				if (newCursor.Y < 0) newCursor.Y = 0;
				//Rectangle rect = handler.SelectedRegion;
				
				handler.CursorPosition = newCursor;
				/*handler.SetCursorPosition(newCursor, false);
				handler.InvalidateCharacterRegion(new Rectangle(pos, new Size(1,1)));
				rect.Normalize();
				rect.Width++;
				rect.Height++;
				
				handler.InvalidateCharacterRegion(rect);
				 */
				
				Point scrollPos = viewer.ScrollPosition;
				scrollPos.Y += (int)Math.Round(verticalOffset * fontHeight) * ydir;
				if (scrollPos.Y < 0) scrollPos.Y = 0;
				viewer.ScrollPosition = scrollPos;

				/*
				handler.InvalidateCharacterRegion(new Rectangle(newCursor, new Size(1,1)));
				rect = handler.SelectedRegion;
				rect.Normalize();
				rect.Width++;
				rect.Height++;
				
				handler.InvalidateCharacterRegion(rect);
				 */
				
			}
		}
	}
}
