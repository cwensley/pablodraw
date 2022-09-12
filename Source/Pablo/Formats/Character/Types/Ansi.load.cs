using System;
using System.IO;
using System.Text;
using Eto.Drawing;

namespace Pablo.Formats.Character.Types
{
	public partial class Ansi
	{
		class AnsiLoader
		{
			Point ptSave;
			Point ptCur;
			Attribute attribute;
			Attribute rgbAttribute;
			Canvas canvas;
			Palette palette;
			Rectangle rClip;
			bool? iceColours;
			bool? ansiLineWrap;
			bool lastLineHasData;

			public bool AnimateView { get; set; }

			public bool LineWrap { get; set; }

			public AnsiLoader()
			{
				LineWrap = true;
			}

			void ReadChar(int curChar)
			{
				if (ptCur.X >= rClip.Left && ptCur.X <= rClip.InnerRight)
					canvas[ptCur] = new CanvasElement((byte)curChar, rgbAttribute);
				ptCur.X++;
				lastLineHasData = true;

				if (ptCur.X > rClip.InnerRight)
				{
					if (ansiLineWrap ?? LineWrap)
					{
						ptCur.Y++;
						lastLineHasData = false;
						if (ptCur.Y > rClip.InnerBottom)
						{
							if (AnimateView)
							{
								canvas.ShiftUp();
								ptCur.Y = rClip.InnerBottom;
							}
							else
								throw new CanvasOverflowException();
						}
						ptCur.X = rClip.Left;
					}
					else
						ptCur.X = rClip.InnerRight;
				}
			}

			static bool ConvertToInt(string val, out int i)
			{
				return Int32.TryParse(val, out i);
			}

			static int? ConvertToInt(string val)
			{
				int result;
				return Int32.TryParse(val, out result) ? (int?)result : null;
			}

			static readonly byte[] ColourMap = { 0, 4, 2, 6, 1, 5, 3, 7 };

			void ReadEscapeSequence(ref BinaryReader br)
			{
				var sb = new StringBuilder();

				char curChar = (char)br.ReadByte();
				while (!char.IsLetter(curChar))
				{
					sb.Append(curChar);
					curChar = (char)br.ReadByte();
				}

				string[] args = (sb.Length > 0) ? sb.ToString().Split(';') : new string[0];
				int i;

				switch (curChar)
				{
					case 'A':  // move cursor up
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.Y -= i;
						if (ptCur.Y < rClip.Top)
							ptCur.Y = rClip.Top;
						break;

					case 'B':  // move cursor down
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.Y += i;
						if (ptCur.Y > rClip.InnerBottom)
							ptCur.Y = rClip.InnerBottom;
						break;

					case 'C':  // move cursor right
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.X += i;
						if (ptCur.X > rClip.InnerRight)
							ptCur.X = rClip.InnerRight;
						break;
                
					case 'D':  // move cursor left
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.X -= i;
						if (ptCur.X < rClip.Left)
							ptCur.X = rClip.Left;
						break;
					case 'E':
						ptCur.X = rClip.Left;
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.Y += i;
						while (ptCur.Y > rClip.InnerBottom)
						{
							if (AnimateView)
							{
								canvas.ShiftUp();
								ptCur.Y--;
							}
							else
								throw new CanvasOverflowException();
						}
						break;
					case 'F':
						ptCur.X = rClip.Left;
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.Y -= i;
						if (ptCur.Y < rClip.Top)
							ptCur.Y = rClip.Top;
						break;
					case 'G':
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
						ptCur.X = rClip.Left + i - 1;
						if (ptCur.X < rClip.Left)
							ptCur.X = rClip.Left;
						if (ptCur.X > rClip.InnerRight)
							ptCur.X = rClip.InnerRight;
						break;

					case 'H':
					case 'f':
						if (args.Length > 0 && args[0] != string.Empty)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i > 0)
									ptCur.Y = i - 1 + rClip.Top;
								else
									ptCur.Y = rClip.Top;
							}
						}
						else
							ptCur.Y = rClip.Top;

						if (args.Length > 1 && args[1] != string.Empty)
						{
							if (ConvertToInt(args[1], out i))
							{
								if (i > 0)
									ptCur.X = i - 1 + rClip.Left;
								else
									ptCur.X = rClip.Left;
							}
						}
						else
							ptCur.X = rClip.Left;
				
						ptCur.Restrict(rClip);
						break;

					case 'J':
						if (args.Length > 0)
						{
							ConvertToInt(args[0], out i);
						}
						else
							i = 0;
				
						switch (i)
						{
							case 0:
								canvas.Fill(new Rectangle(new Point(rClip.Left, ptCur.Y), rClip.InnerBottomRight), new CanvasElement(' ', 7));
								break;
							case 1:
								canvas.Fill(new Rectangle(rClip.TopLeft, new Point(rClip.InnerRight, ptCur.Y)), new CanvasElement(' ', 7));
								break;
							case 2:
								attribute = rgbAttribute = new Attribute(7, 0);
								canvas.Fill(rClip, new CanvasElement(' ', attribute));
								ptCur.X = rClip.Left;
								ptCur.Y = rClip.Top;
								break;
						}
						break;
					case 'K':
						if (args.Length > 0)
							ConvertToInt(args[0], out i);
						else
							i = 0;
				
						switch (i)
						{
							case 0:
								canvas.Fill(new Rectangle(ptCur, new Point(rClip.InnerRight, ptCur.Y)), new CanvasElement(' ', 7));
								break;
							case 1:
								canvas.Fill(new Rectangle(ptCur, new Point(0, ptCur.Y)), new CanvasElement(' ', 7));
								break;
							case 2:
								canvas.Fill(new Rectangle(new Point(0, ptCur.Y), new Point(rClip.InnerRight, ptCur.Y)), new CanvasElement(' ', 7));
								break;
						}
						break;
					case 't':
						if (args.Length == 4)
						{
							var r = ConvertToInt(args[1]) ?? 0;
							var g = ConvertToInt(args[2]) ?? 0;
							var b = ConvertToInt(args[3]) ?? 0;
							var col = palette.FindAddColour(Color.FromArgb(r, g, b));
							switch (args[0])
							{
								case "0":
									rgbAttribute.Background = col;
									break;
								case "1":
									rgbAttribute.Foreground = col;
									break;
							}
						}
						break;
					case 'h':
						foreach (string flag in args)
						{
							switch (flag.Trim())
							{
								case "?33": // ice colour on
									iceColours = true;
									break;
								case "?7":
									ansiLineWrap = true;
									break;
							}
						}
						break;
					case 'l':
						foreach (var flag in args)
						{
							switch (flag.Trim())
							{
								case "?33": // ice colour off
									iceColours = false;
									break;
								case "?7":
									ansiLineWrap = false;
									break;
							}
						}
						break;
				
					case 'm':
						if (args.Length == 0)
						{
							attribute = rgbAttribute = new Attribute(7, 0);
						}
						else
						{
							for (int count = 0; count < args.Length; count++)
							{
								int j;
								if (ConvertToInt(args[count], out i))
								{
									switch (i)
									{
										case 0:
											attribute = rgbAttribute = new Attribute(7, 0);
											break;
										case 1:
											attribute.Bold = true;
											rgbAttribute.Foreground = attribute.Foreground;
											break;
										case 2:
										case 22:
											attribute.Bold = false;
											rgbAttribute.Foreground = attribute.Foreground;
											break;
										case 5:
											attribute.Blink = true;
											rgbAttribute.Background = attribute.Background;
											break;
										case 25:
											attribute.Blink = false;
											rgbAttribute.Background = attribute.Background;
											break;
										case 7:
										case 27:
											j = attribute.ForegroundOnly;
											attribute.ForegroundOnly = attribute.BackgroundOnly;
											attribute.BackgroundOnly = j;
											j = rgbAttribute.ForegroundOnly;
											rgbAttribute.ForegroundOnly = rgbAttribute.BackgroundOnly;
											rgbAttribute.BackgroundOnly = j;
											break;
										case 30:
										case 31:
										case 32:
										case 33:
										case 34:
										case 35:
										case 36:
										case 37:
											attribute.ForegroundOnly = ColourMap[i - 30];
											rgbAttribute.Foreground = attribute.Foreground;
											break;
										case 40:
										case 41:
										case 42:
										case 43:
										case 44:
										case 45:
										case 46:
										case 47:
											attribute.BackgroundOnly = ColourMap[i - 40];
											rgbAttribute.Background = attribute.Background;
											break;
									}
								}
							}
						}
						break;
					case 'S':
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
				
						if (AnimateView)
						{
							while (i > 1)
							{
								canvas.ShiftUp();
								i--;
							}
						}
						break;
					case 'T':
						if (args.Length > 0)
						{
							if (ConvertToInt(args[0], out i))
							{
								if (i == 0)
									i = 1;
							}
						}
						else
							i = 1;
				
						if (AnimateView)
						{
							while (i > 1)
							{
								canvas.ShiftDown();
								i--;
							}
						}
						break;
				
					case 's':
						if (args.Length == 0)
							ptSave = ptCur;
						break;

					case 'u':
						if (args.Length == 0)
							ptCur = ptSave;
						break;
				}
			
			}

			void ReadCurByte(byte curByte)
			{
				switch (curByte)
				{
					case 10:
						ptCur.Y++;
						lastLineHasData = false;
						if (ptCur.Y > rClip.InnerBottom)
						{
							if (AnimateView)
							{
								canvas.ShiftUp();
								ptCur.Y = rClip.InnerBottom;
							}
							else
								throw new CanvasOverflowException();
						}
						ptCur.X = rClip.Left;
						break;
					case 13:
					case 26:
						break;
					default:
						ReadChar(curByte);
						break;
				}
			}

			public void Load(Stream fs, Canvas canvas, Palette palette, WaitEventHandler onWait = null)
			{
				ansiLineWrap = null;
				var br = new BinaryReader(fs);

				this.canvas = canvas;
				this.palette = palette;
				rClip = new Rectangle(0, 0, canvas.Width, canvas.Height);
				attribute = rgbAttribute = new Attribute(7, 0);
				ptCur = rClip.Location;
				try
				{
					var args = new WaitEventArgs();
					byte curByte = br.ReadByte();
					while (true && (!fs.CanSeek || fs.Position < fs.Length))
					{
						if (onWait != null)
							onWait(this, args);
						if (args.Exit)
							break;
						if (curByte == 27)
						{
							curByte = br.ReadByte();
							if (curByte == 91)
							{
								// Escape sequence found!  scan for parameter
								ReadEscapeSequence(ref br);
							}
							else
							{
								ReadCurByte(27);
								continue;
							}
						}
						else
						{
							ReadCurByte(curByte);
						}
						curByte = br.ReadByte();
					}
				}
				catch (EndOfStreamException)
				{
					// end of stream, so we're finished
				}
				catch (CanvasOverflowException)
				{
					// everything's still okay, just went over the limits of the canvas
				}
				// Console.WriteLine("Finished Loading!");
				br.Close();
			}

			public void Load(Ansi ansi, Stream fs, CharacterDocument document, CharacterHandler handler)
			{
				AnimateView = document.AnimateView;
				Page page = document.Pages[0];
				ansi.ResizeCanvasWidth(fs, document, page.Canvas);
				Load(fs, page.Canvas, page.Palette, (sender, args) => document.OnWait(args));
				if (iceColours != null)
					document.ICEColours = iceColours.Value;
				while (page.Palette.Count < 16)
					page.Palette.Add(Colors.Black);

				if (!document.EditMode && document.FileName != null && string.Equals(Path.GetExtension(document.FileName), ".diz", StringComparison.OrdinalIgnoreCase))
				{
					var endX = page.Canvas.FindEndX(CanvasElement.Default);
					page.Canvas.ResizeCanvas(new Size(endX + 1, page.Canvas.Height), false, true);
				}

				if (lastLineHasData)
					ptCur.Y += 1;

				if (!AnimateView)
					ansi.ResizeCanvasHeight(document, page.Canvas, ptCur.Y);
			}
		}

		public override void Load(Stream fs, CharacterDocument document, CharacterHandler handler)
		{
			var loader = new AnsiLoader();
			loader.Load(this, fs, document, handler);
		}

		public void Load(Stream fs, Canvas canvas, Palette palette, bool? lineWrap = null)
		{
			var loader = new AnsiLoader();
			if (lineWrap != null)
				loader.LineWrap = lineWrap.Value;
			loader.Load(fs, canvas, palette);
		}

	}
}

