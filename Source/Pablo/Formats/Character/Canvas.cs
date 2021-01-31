using System;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Character
{
	public class CanvasOverflowException : Exception
	{
	}

	public delegate int TranslateCharacter(int ch);

	public abstract class Canvas
	{
		Size canvasSize;

		#region Events

		public event EventHandler<EventArgs> SizeChanged;
		public event UpdateEventHandler Update;

		public void OnUpdate(Rectangle rect)
		{
			if (Update != null)
				Update(this, rect);
		}

		#endregion

		#region Properties

		public Size Size
		{
			get { return canvasSize; }
			set
			{
				canvasSize = value;
				ResizeCanvas(value, false);
			}
		}

		public int Width
		{
			get { return canvasSize.Width; }
		}

		public int Height
		{
			get { return canvasSize.Height; }
		}

		public CanvasElement this[Point p]
		{
			get { return Get(p.X, p.Y); }
			set
			{
				var ce = this.Get(p.X, p.Y);
				if (ce != value)
				{
					Set(p.X, p.Y, value);
					OnUpdate(new Rectangle(p.X, p.Y, 1, 1));
				}
			}
		}

		public CanvasElement this[int x, int y]
		{
			get { return Get(x, y); }
			set
			{
				var ce = this.Get(x, y);
				if (ce != value)
				{
					Set(x, y, value);
					OnUpdate(new Rectangle(x, y, 1, 1));
				}
			}
		}

		#endregion

		public Canvas(Size canvasSize)
		{
			this.canvasSize = canvasSize;
		}

		public IEnumerable<CanvasElement> GetElements(Point point, int length)
		{
			return GetElements(new Rectangle(point, new Size(length, 1)));
		}

		public IEnumerable<CanvasElement> GetElements(Rectangle rect)
		{
			if (rect.IsEmpty)
				yield break;
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
				for (int x = rect.Left; x <= rect.InnerRight; x++)
				{
					yield return this[x, y];
				}
		}

		protected abstract CanvasElement Get(int x, int y);

		protected abstract void Set(int x, int y, CanvasElement ce);

		protected abstract void SetLine(int x, int y, Character ce, int width);

		protected abstract void SetLine(int x, int y, CanvasElement ce, int width);

		protected abstract void SetLine(int x, int y, Attribute attrib, int width);

		protected abstract void SetForeLine(int x, int y, int foreground, int width);

		protected abstract void SetBackLine(int x, int y, int background, int width);

		protected abstract void SetLine(int x, int y, int sourcex, int width, CanvasElement[] line);

		public abstract CanvasElement[] GetLine(int y);

		public abstract void DeleteCharacter(int x, int y, CanvasElement ce);

		public abstract void InsertCharacter(int x, int y, CanvasElement ce);

		public abstract void ShiftUp();

		public abstract void ShiftDown();

		public abstract void InsertColumn(int x);

		public abstract void DeleteColumn(int x);

		public abstract void InsertRow(int y);

		public abstract void DeleteRow(int y);

		public virtual void Clear()
		{
			Fill(new CanvasElement(32, 7));
		}

		public virtual void Set(Point dest, Rectangle source, Canvas canvas, PasteMode pasteMode = PasteMode.Normal)
		{
			Rectangle destRect = new Rectangle(dest, source.Size);
			destRect.Restrict(new Rectangle(Size));
			int offsetx = destRect.Left - dest.X;
			int offsety = destRect.Top - dest.Y;
			switch (pasteMode)
			{
				case PasteMode.Normal:
					for (int y = 0; y < destRect.Height; y++)
					{
						SetLine(destRect.Left, destRect.Top + y, source.Left + offsetx, destRect.Width, canvas.GetLine(source.Top + y + offsety));
					}
					break;
				case PasteMode.Under:
					for (int y = 0; y < destRect.Height; y++)
						for (int x = 0; x < destRect.Width; x++)
						{
							var destPt = destRect.TopLeft + new Size(x, y);
							var ce = this[destPt];
							if (ce.IsTransparent)
								this[destPt] = canvas[source.TopLeft + new Size(x, y)];
						}
					break;
				case PasteMode.Transparent:
					for (int y = 0; y < destRect.Height; y++)
						for (int x = 0; x < destRect.Width; x++)
						{
							var destPt = destRect.TopLeft + new Size(x, y);
							var ce = canvas[source.TopLeft + new Size(x, y)];
							if (!ce.IsTransparent)
								this[destPt] = ce;
						}
					break;
			}
			OnUpdate(destRect);
		}

		public virtual void UpdatePalette(Palette palette)
		{
			for (int y = 0; y < this.Height; y++)
			{
				var line = GetLine(y);
				for (int x = 0; x < this.Width; x++)
				{
					var ce = line[x];
					var attr = ce.Attribute;
					attr.Foreground = Math.Min(attr.Foreground, palette.Count - 1);
					attr.Background = Math.Min(attr.Background, palette.Count - 1);
					ce.Attribute = attr;
					line[x] = ce;
				}
				SetLine(0, y, 0, this.Width, line);
			}
		}

		public virtual void Set(Point dest, Canvas canvas)
		{
			Set(dest, new Rectangle(Point.Empty, canvas.Size), canvas);
		}

		public virtual Canvas Rotate(TranslateCharacter translate = null)
		{
			var mc = new MemoryCanvas(new Size(Height, Width));
			for (int y = 0; y < mc.Height; y++)
				for (int x = 0; x < mc.Width; x++)
				{
					var val = this[y, x];
					if (translate != null)
						val.Character = translate(val.Character);
					mc[mc.Width - x - 1, y] = val;
				}
			return mc;
		}

		public virtual void FlipX(TranslateCharacter translate = null)
		{
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < (Width + 1) / 2; x++)
				{
					int right = Width - x - 1;
					var vall = this[x, y];
					var valr = this[right, y];
					if (translate != null)
					{
						vall.Character = translate(vall.Character);
						valr.Character = translate(valr.Character);
					}
					this[x, y] = valr;
					this[right, y] = vall;
				}
		}

		public virtual void FlipY(TranslateCharacter translate = null)
		{
			for (int y = 0; y < (Height + 1) / 2; y++)
			{
				int bottom = Height - y - 1;
				for (int x = 0; x < Width; x++)
				{
					var valt = this[x, y];
					var valb = this[x, bottom];
					if (translate != null)
					{
						valt.Character = translate(valt.Character);
						valb.Character = translate(valb.Character);
					}
					this[x, y] = valb;
					this[x, bottom] = valt;
				}
			}
		}

		public virtual Canvas Copy()
		{
			Canvas canvas = new MemoryCanvas(this.Size);
			canvas.Set(Point.Empty, this);
			return canvas;
		}

		public virtual Canvas Copy(Rectangle rect)
		{
			Canvas canvas = new MemoryCanvas(rect.Size);
			canvas.Set(new Point(0, 0), rect, this);
			return canvas;
		}

		public virtual void Fill(Rectangle rect, Character character)
		{
			rect.Restrict(new Rectangle(0, 0, Width, Height));
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
			{
				SetLine(rect.Left, y, character, rect.Width);
			}
			OnUpdate(rect);
		}

		public virtual void Fill(CanvasElement ce)
		{
			OnUpdate(new Rectangle(0, 0, Width, Height));
		}

		public void FillForeground(Rectangle rect, int foreground)
		{
			rect.Restrict(new Rectangle(0, 0, Width, Height));
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
			{
				SetForeLine(rect.Left, y, foreground, rect.Width);
			}
			OnUpdate(rect);
		}

		public void FillBackground(Rectangle rect, int background)
		{
			rect.Restrict(new Rectangle(0, 0, Width, Height));
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
			{
				SetBackLine(rect.Left, y, background, rect.Width);
			}
			OnUpdate(rect);
		}

		public void ResizeCanvas(Size canvasSize, bool notify, bool keepContent = false)
		{
			var oldCanvasSize = Size;
			this.canvasSize = canvasSize;
			ResizeCanvas(oldCanvasSize, keepContent);
			if (notify)
				OnSizeChanged(EventArgs.Empty);
		}

		public virtual void Fill(Rectangle rect, CanvasElement ce)
		{
			rect.Restrict(new Rectangle(0, 0, Width, Height));
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
			{
				SetLine(rect.Left, y, ce, rect.Width);
			}
			OnUpdate(rect);
		}

		public virtual void Fill(Rectangle rect, Attribute attrib)
		{
			rect.Restrict(new Rectangle(0, 0, Width, Height));
			for (int y = rect.Top; y <= rect.InnerBottom; y++)
			{
				SetLine(rect.Left, y, attrib, rect.Width);
			}
			OnUpdate(rect);
		}

		public int FindEndX(int y, CanvasElement defaultElement)
		{
			return FindEndX(y, 0, Width - 1, defaultElement);
		}

		public virtual int FindEndX(int y, int startx, int endx, CanvasElement defaultElement)
		{
			CanvasElement element;
			if (endx == 0)
				endx = this.Width - 1;
			var defaultBackground = defaultElement.Attribute.BackgroundOnly;
			for (int col = endx; col >= startx; col--)
			{
				element = this[col, y];
				var ch = element.Character.character;
				if ((ch != 32 && ch != 0) || (element.Attribute.BackgroundOnly != defaultBackground))
				{
					return col;
				}
			}
			return -1;
		}

		public int FindEndX(CanvasElement defaultElement)
		{
			int maxEndX = -1;
			for (int row = 0; row < Height; row++)
			{
				var endX = FindEndX(row, defaultElement);
				if (endX != -1)
				{
					maxEndX = Math.Max(maxEndX, endX);
				}
			}
			return maxEndX;
		}

		public virtual int FindEndY(CanvasElement defaultElement)
		{
			for (int row = Height - 1; row >= 0; row--)
			{
				if (FindEndX(row, defaultElement) != -1)
				{
					return row;
				}
			}
			return -1;
		}

		protected abstract void ResizeCanvas(Size oldCanvasSize, bool keepContent);

		protected virtual void OnSizeChanged(EventArgs e)
		{
			if (SizeChanged != null)
				SizeChanged(this, e);
		}
	}
}
