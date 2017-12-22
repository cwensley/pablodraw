using System;
using System.Text;
using Eto.Drawing;
using Eto.Forms;

namespace Pablo.Formats.Character.Controls
{
	public class FontTextBox : Drawable
	{
		Canvas canvas;
		readonly BitFont font;
		readonly Palette palette;
		readonly CharacterHandler handler;
		Point cursor;
		bool bordered = true;
		bool readOnly;

		public event EventHandler<EventArgs> CursorElementChanged;

		protected virtual void OnCursorElementChanged(EventArgs e)
		{
			if (CursorElementChanged != null)
				CursorElementChanged(this, e);
		}

		public Canvas Canvas
		{
			get { return canvas; }
			private set
			{
				if (canvas != value)
				{
					if (canvas != null)
						canvas.Update -= HandleCanvasUpdate;
					canvas = value;
					canvas.Update += HandleCanvasUpdate;

					UpdateCanvasSize();
					Invalidate();
				}
			}
		}

		public Point TextCursor
		{
			get { return cursor; }
			set
			{
				if (cursor != value)
				{
					cursor = value;
					Invalidate();
				}
			}
		}

		public Size TextSize
		{
			get { return canvas.Size; }
			set
			{
				Canvas = new MemoryCanvas(value);
			}
		}

		public bool Bordered
		{
			get { return bordered; }
			set
			{
				if (bordered != value)
				{
					bordered = value;
					UpdateCanvasSize();
					Invalidate();
				}
			}
		}

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				CanFocus = !readOnly;
			}
		}

		public CanvasElement CursorElement
		{
			get { return canvas[cursor]; }
			set { canvas[cursor] = value; }
		}

		public void SetAttribute(Attribute attribute)
		{
			canvas.Fill(new Rectangle(new Point(0, 0), canvas.Size), attribute);
			Invalidate();
		}

		public void SetString(string value)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(value);
			int length = Math.Min(bytes.Length, canvas.Width);
			for (int i = 0; i < length; i++)
			{
				var ce = canvas[i, 0];
				ce.Character = bytes[i];
				canvas[i, 0] = ce;
			}
		}

		public FontTextBox(CharacterHandler handler, Size? size, BitFont font = null, Palette palette = null)
		{
			this.font = font;
			this.palette = palette;
			this.handler = handler;

			this.Canvas = new MemoryCanvas(size ?? Size.Empty);

			cursor = Point.Empty;
			UpdateCanvasSize();
		}

		public void UpdateCanvasSize()
		{
			var thefont = font ?? handler.CurrentPage.Font;
			var size = canvas.Size * thefont.Size;
			if (Bordered)
				size += 4;
			Size = size;
		}

		void HandleCanvasUpdate(object sender, Rectangle rect)
		{
			Invalidate();
		}

		public override void OnMouseDown(MouseEventArgs e)
		{
			var location = (Point)e.Location;
			location.Offset(-3, -3);
			var currentFont = font ?? handler.CurrentPage.Font;
			cursor = location / currentFont.Size;
			cursor.Restrict(new Rectangle(canvas.Size));
			OnCursorElementChanged(EventArgs.Empty);
			Invalidate();
			base.OnMouseDown(e);
			
			if (CanFocus)
				Focus();
		}

		public override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		public override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			Invalidate();
		}

		public override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.ImageInterpolation = ImageInterpolation.None;
			var drawPoint = e.ClipRectangle.Location;
			var rect = e.ClipRectangle;
			e.Graphics.FillRectangle(Colors.Black, new Rectangle(Size));
			var currentFont = font ?? handler.CurrentPage.Font;
			var currentPal = palette ?? handler.CurrentPage.Palette;
			if (Bordered)
			{
				e.Graphics.DrawInsetRectangle(Colors.Gray, Colors.White, new Rectangle(Size));
				//drawPoint.X = Math.Max (2, drawPoint.X);
				//drawPoint.Y = Math.Max (2, drawPoint.Y);
				rect.Offset(-2, -2);
				rect.Width += 2;
				rect.Height += 2;
				//drawPoint.Add(2, 2);
				if (rect.X < 0)
				{
					rect.Width += rect.X;
					drawPoint.X -= rect.X;
					rect.X = 0;
				}
				if (rect.Y < 0)
				{
					rect.Height += rect.Y;
					drawPoint.Y -= rect.Y;
					rect.Y = 0;
				}
			}
			if (rect.Width > 0 && rect.Height > 0)
			{
				var maxSize = canvas.Size * currentFont.Size;
				rect.Width = Math.Min(rect.Width, maxSize.Width);
				rect.Height = Math.Min(rect.Height, maxSize.Height);
				var bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb, Generator);
				Point? p;
				if (HasFocus && (CanFocus || !ReadOnly))
					p = cursor;
				else
					p = null;
				Page.GenerateRegion(canvas, bitmap, rect, currentFont.Size, currentFont, currentPal, handler.CharacterDocument.ICEColours, true, p, null);
				
				e.Graphics.DrawImage(bitmap, drawPoint);
			}
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			if (!ReadOnly || CanFocus)
			{
				switch (e.KeyData)
				{
					case Key.Left:
						if (cursor.X > 0)
						{
							cursor.X--;
							e.Handled = true;
							OnCursorElementChanged(EventArgs.Empty);
						}
						break;
					case Key.Right:
						if (cursor.X < canvas.Width - 1)
						{
							cursor.X++;
							e.Handled = true;
							OnCursorElementChanged(EventArgs.Empty);
						}
						break;
					case Key.Up:
						if (cursor.Y > 0)
						{
							cursor.Y--;
							e.Handled = true;
							OnCursorElementChanged(EventArgs.Empty);
						}
						break;
					case Key.Down:
						if (cursor.Y < canvas.Height - 1)
						{
							cursor.Y++;
							e.Handled = true;
							OnCursorElementChanged(EventArgs.Empty);
						}
						break;
				}
			}
			
			if (!e.Handled && !ReadOnly)
			{
				if (e.KeyData >= Key.F1 && e.KeyData <= Key.F12)
				{
					int character = e.KeyData - Key.F1;
					InsertChar(handler.Info.GetCharacter(handler.CharacterSet, character));
					e.Handled = true;
				}
				else
					switch (e.KeyData)
					{
						case Key.Escape:
						case Key.Enter:
						case Key.Tab:
						case Key.Tab | Key.Shift:
							break;
						case Key.Backspace:
							if (cursor.X > 0)
							{
								cursor.X--;
								canvas.DeleteColumn(cursor.X);
								OnCursorElementChanged(EventArgs.Empty);
								e.Handled = true;
							}
							break;
						case Key.Delete:
							canvas.DeleteColumn(cursor.X);
							e.Handled = true;
							break;
						default:
							if (e.IsChar)
							{
								e.Handled = true;
								byte[] bytes = Encoding.ASCII.GetBytes(new char[] { e.KeyChar });
								InsertChar(bytes[0]);
							}
							break;
					}
				
			}
			if (e.Handled)
				Invalidate();
			
			base.OnKeyDown(e);
		}

		public void Insert(CanvasElement ce)
		{
			var currentFont = font ?? handler.CurrentPage.Font;
			if (cursor.X < canvas.Width && ce.Character < currentFont.NumChars)
			{
				canvas[cursor] = ce;
				if (cursor.X < canvas.Width - 1)
					cursor.X++;
				OnCursorElementChanged(EventArgs.Empty);
			}
		}

		public void InsertChar(int b)
		{
			var currentFont = font ?? handler.CurrentPage.Font;
			if (cursor.X < canvas.Width && b < currentFont.NumChars)
			{
				canvas[cursor] = new CanvasElement(b, canvas[cursor].Attribute);
				if (cursor.X < canvas.Width - 1)
					cursor.X++;
				OnCursorElementChanged(EventArgs.Empty);
			}
		}
	}
}
