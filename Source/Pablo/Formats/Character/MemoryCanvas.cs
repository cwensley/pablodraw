using System;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Character
{
	public class MemoryCanvas : Canvas
	{
		const int spaceCharacter = 32;
		const int topChar = 0xdf;
		const int bottomChar = 0xdc;
		const int fullCharacter = 0xdb;

		CanvasElement ceDefault;
		CanvasElement[][] canvas;

		public MemoryCanvas(Size canvasSize) : base(canvasSize)
		{
			ceDefault = CanvasElement.Default;
			canvas = new CanvasElement[canvasSize.Height][];
		}

		public override void Fill(CanvasElement ce)
		{
			ceDefault = ce;
			for (int i=0; i<Size.Height; i++)
			{
				canvas[i] = null;
			}
			base.Fill(ce);
		}

		protected override CanvasElement Get(int x, int y)
		{
			var line = canvas[y];
			return (line == null) ? ceDefault : line[x];
		}
		
		public override void DeleteCharacter(int x, int y, CanvasElement ce)
		{
			var line = GetLine(y);
			for (int i=x; i<Width-1; i++)
			{
				line[i] = line[i+1];
			}
			line[Width-1] = ce;
			this.OnUpdate(new Rectangle(x, y, Width-x, 1));
		}

		public override void InsertCharacter(int x, int y, CanvasElement ce)
		{
			var line = GetLine(y);
			for (int i=Width-1; i>x; i--)
			{
				line[i] = line[i-1];
			}
			line[x] = ce;
			this.OnUpdate(new Rectangle(x, y, Width-x, 1));
		}


		protected override void Set(int x, int y, CanvasElement ce)
		{
			var line = GetLine(y);
			line[x] = ce;
		}

		protected override void SetLine(int x, int y, CanvasElement ce, int width)
		{
			var line = GetLine(y);
			for (int i=x; i<x+width; i++)
			{
				line[i] = ce;
			}
		}

		protected override void SetLine(int x, int y, Attribute attrib, int width)
		{
			var line = GetLine(y);
			for (int i=x; i<x+width; i++)
			{
				line[i].Attribute = attrib;
			}
		}

		protected override void SetHalfLine(int x, int y, int color, int width)
		{
			var line = GetLine(y / 2);
			var top = (y % 2) == 0;

			for (int i = x; i < x + width; i++)
			{
				var ch = line[i];
				ch = UpdateHalfChar(color, top, ch);
				line[i] = ch;
			}
		}

		protected override void ClearHalfLine(int x, int y, int backgroundColor, int width)
		{
			var line = GetLine(y / 2);
			var top = (y % 2) == 0;

			for (int i = x; i < x + width; i++)
			{
				var ce = line[i];
				var color = ce.Foreground;
				var ch = ce.Character;
				ce.Background = backgroundColor;
				if (top)
				{
					if (ch == fullCharacter)
						ce.Character = bottomChar;
					else if (ch != bottomChar)
						ce.Character = spaceCharacter;
				}
				else
				{
					if (ch == fullCharacter)
						ce.Character = topChar;
					else if (ch != topChar)
						ce.Character = spaceCharacter;
				}

				line[i] = ce;
			}
		}

		public static CanvasElement UpdateHalfChar(int color, bool top, CanvasElement ch)
		{
			var background = ch.Background;
			var foreground = ch.Foreground;
			int character = ch.Character;

			switch ((int)ch.Character)
			{
				case fullCharacter:
					if (foreground != color)
					{
						background = foreground;
						foreground = color;
						character = top ? topChar : bottomChar;
					}
					break;
				case topChar:
					if (top)
					{
						foreground = color;
						character = topChar;
					}
					else
					{
						if (foreground == color)
							character = fullCharacter;
						else
							background = color;
					}
					break;
				case bottomChar:
					if (top)
					{
						if (foreground == color)
							character = fullCharacter;
						else
							background = color;
					}
					else
					{
						foreground = color;
						character = bottomChar;
					}
					break;
				default:
					foreground = color;
					character = top ? topChar : bottomChar;
					break;
			}

			// prefer non-blinking background color if possible..
			if (background >= 8 && foreground < 8)
			{
				bool swap = false;
				switch (character)
				{
					case topChar:
						character = bottomChar;
						swap = true;
						break;
					case bottomChar:
						character = topChar;
						swap = true;
						break;
					case spaceCharacter:
						character = fullCharacter;
						swap = true;
						break;
				}
				if (swap)
				{
					var tmp = background;
					background = foreground;
					foreground = tmp;
				}
			}
			ch.Background = background;
			ch.Foreground = foreground;
			ch.Character = new Character(character);
			return ch;
		}

		protected override void SetForeLine(int x, int y, int foreground, int width)
		{
			var line = GetLine(y);
			for (int i=x; i<x+width; i++)
			{
				line[i].Attribute = new Attribute(foreground, line[i].Attribute.Background);
			}
		}
		
		protected override void SetBackLine(int x, int y, int background, int width)
		{
			var line = GetLine(y);
			for (int i=x; i<x+width; i++)
			{
				line[i].Attribute = new Attribute(line[i].Attribute.Foreground, background);
			}
		}
		
		protected override void SetLine(int x, int y, Character character, int width)
		{
			var line = GetLine(y);
			for (int i=x; i<x+width; i++)
			{
				line[i].Character = character;
			}
		}
		
		protected override void SetLine(int x, int y, int sourcex, int width, CanvasElement[] line)
		{
			var cline = GetLine (y);
			for (int i=0; i<width; i++)
			{
				cline[x+i] = line[sourcex+i];
			}
		}
		
		public override CanvasElement[] GetLine(int y)
		{
			var line = canvas[y];
			if (line == null) { 
				line = new CanvasElement[Size.Width]; 
				for (int i=0; i<Size.Width; i++)
				{
					line[i] = ceDefault;
				}
				canvas[y] = line; 
			}
			return line;
		}
		
		public override void ShiftUp()
		{
			for (int y=0; y<Height-1; y++)
			{
				canvas[y] = canvas[y+1];
			}
			canvas[Height-1] = null; // clear last line
			this.OnUpdate(new Rectangle(0, 0, Width, Height));
		}

		public override void ShiftDown()
		{
			for (int y=Height-2; y>0; y--)
			{
				canvas[y+1] = canvas[y];
			}
			canvas[0] = null; // clear last line
			this.OnUpdate(new Rectangle(0, 0, Width, Height));
		}
		
		public override void InsertColumn(int x)
		{
			for (int y = 0; y < Height; y++)
			{
				CanvasElement[] cline = canvas[y];
				if (cline != null)
				{
					for (int count = Width - 1; count > x; count--)
					{
						cline[count] = cline[count - 1];
					}
					cline[x] = CanvasElement.Default;
				}
			}
			this.OnUpdate(new Rectangle(x, 0, Width-x, Height));
		}

		public override void DeleteColumn(int x)
		{
			for (int y = 0; y < Height; y++)
			{
				CanvasElement[] cline = canvas[y];
				if (cline != null)
				{
					for (int count = x; count < Width - 1; count++)
					{
						cline[count] = cline[count + 1];
					}
					cline[Width-1] = CanvasElement.Default;
				}
			}
			this.OnUpdate(new Rectangle(x, 0, Width - x, Height));
		}

		public override void InsertRow(int y)
		{
			for (int count = Height - 1; count > y; count--)
			{
				canvas[count] = canvas[count - 1];
			}
			canvas[y] = null; // clear last line
			this.OnUpdate(new Rectangle(0, y, Width, Height - y));
		}

		public override void DeleteRow(int y)
		{
			for (int count = y; count < Height - 1; count++)
			{
				canvas[count] = canvas[count + 1];
			}
			canvas[Height - 1] = null; // clear last line
			this.OnUpdate(new Rectangle(0, y, Width, Height - y));
		}

		protected override void ResizeCanvas(Size oldCanvasSize, bool keepContent)
		{
			if (oldCanvasSize == Size) return;
			var newCanvasSize = Size;
			var oldCanvas = canvas;
			canvas = new CanvasElement[newCanvasSize.Height][];

			var maxWidth = Math.Min(newCanvasSize.Width, oldCanvasSize.Width);
			for (int i=0; i<newCanvasSize.Height; i++)
			{
				if (oldCanvasSize.Width == newCanvasSize.Width)
				{
					canvas[i] = (i < oldCanvas.Length) ? oldCanvas[i] : null;
				}
				else if (keepContent)
				{
					var oldline = oldCanvas[i];
					if (oldline != null)
					{
						var line = GetLine(i);
						for (int x = 0; x < maxWidth; x++)
						{
							line[x] = oldline[x];
						}
					}
				}
				else
				{
					canvas[i] = null;
				}
			}
			// need to copy over old info later.. maybe using a parameter to specify?
		}
		
		
		public override void UpdatePalette (Palette palette)
		{
			for (int y = 0; y < this.Height; y++) {
				var line = canvas[y];
				if (line != null) {
					for (int x = 0; x < this.Width; x++)
					{
						var ce = line[x];
						var attr = ce.Attribute;
						attr.Foreground = Math.Min (attr.Foreground, palette.Count-1);
						attr.Background = Math.Min (attr.Background, palette.Count-1);
						ce.Attribute = attr;
						line[x] = ce;
					}
					canvas[y] = line;
				}
			}
		}
	}
}

