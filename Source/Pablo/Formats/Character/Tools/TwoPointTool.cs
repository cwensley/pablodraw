using System;
using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Formats.Character.Tools
{
	public abstract class TwoPointTool : SimpleTool
	{
		bool hasBegun;

		protected Point? Start { get; set; }

		protected Point? End { get; set; }

		protected virtual bool AllowSameEndPoint
		{
			get { return false; }
		}

		protected abstract bool Apply();

		protected virtual bool ApplyDuplicate()
		{
			return Apply();
		}

		public override bool AllowKeyboard
		{
			get
			{
				return !hasBegun;
			}
		}

		public override void Cancel()
		{
			if (Start != null)
			{
				UpdatingCursor = true;
				Handler.CursorPosition = Start.Value;
				UpdatingCursor = false;
			}
			base.Cancel();
			Start = End = null;
			hasBegun = false;
			Handler.UpdateActionState();
		}

		protected virtual void Begin()
		{
			hasBegun = true;
			Handler.UpdateActionState();
		}

		protected virtual Rectangle? CurrentRectangle
		{
			get
			{
				if (Start != null && End != null)
					return new Rectangle(Start.Value, End.Value);
				else
					return null;
			}
		}

		protected override void Finish()
		{
			Apply();
			base.Finish();
			
			Start = End = null;
			hasBegun = false;
			Handler.UpdateActionState();
		}

		protected virtual void Update(Point start, Point end, Keys modifiers, Point location)
		{
			this.Start = start;
			this.End = end;
		}

		public override void OnMouseDown(MouseEventArgs e)
		{
			if (Start == null)
			{
				switch (e.Buttons)
				{
					case MouseButtons.Primary:
						var point = GetLocation((Point)e.Location);
						Update(point, point, e.Modifiers, (Point)e.Location);
						Begin();
						e.Handled = true;
						break;
				}
			}
			else if (e.Buttons == MouseButtons.Primary)
			{
				var end = GetLocation((Point)e.Location);
				if (End == null || End.Value != end)
					Update(Start.Value, end, e.Modifiers, (Point)e.Location);
				
				if (AllowSameEndPoint || End.Value != Start.Value)
					Finish();
				e.Handled = true;
			}
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			if (Start != null)
			{
				switch (e.Key)
				{
					case Keys.S:
					case Keys.Space:
						if (ApplyDuplicate())
							e.Handled = true;
						break;
					case Keys.Enter:
						Finish();
						e.Handled = true;
						break;
					case Keys.Escape:
						Cancel();
						e.Handled = true;
						break;
				}
			}
			if (!e.Handled)
				base.OnKeyDown(e);
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			if (Start != null)
			{
				switch (e.Buttons)
				{
					case MouseButtons.Primary:
						var end = GetLocation((Point)e.Location);
						if (End == null || End.Value != end)
							Update(Start.Value, end, e.Modifiers, (Point)e.Location);
					
						if (AllowSameEndPoint || End.Value != Start.Value)
							Finish();
						e.Handled = true;
						break;
					case MouseButtons.Alternate:
						Cancel();
						break;
				}
			}
		}

		protected bool UpdatingCursor { get; set; }

		public override void OnSetCursorPosition(Point old, Point cursorPosition, bool invalidate)
		{
			if (!UpdatingCursor && Start != null)
			{
				UpdatingCursor = true;
				if (End == null || cursorPosition != End.Value)
					Update(Start.Value, cursorPosition, Keys.None, cursorPosition);
				UpdatingCursor = false;
			}
			base.OnSetCursorPosition(old, cursorPosition, invalidate);
		}

		public override void OnMouseMove(MouseEventArgs e)
		{
			if (Start != null)
			{
				var end = GetLocation((Point)e.Location);
				if (End == null || end != End.Value)
				{
					Update(Start.Value, end, e.Modifiers, (Point)e.Location);
					e.Handled = true;
				}
			}
		}
	}
}

