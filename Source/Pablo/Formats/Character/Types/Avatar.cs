using System.IO;
using Eto.Drawing;

namespace Pablo.Formats.Character.Types
{
	/// <summary>
	/// Summary description for Format.
	/// </summary>
	public class Avatar : CharacterFormat
	{
		private Point ptCur = new Point(0, 0);
		private Attribute attribute = new Attribute();
		private Canvas canvas;
		private Rectangle rClip;

		public Avatar(DocumentInfo info) : base(info, "avatar", "Avatar", "avt")
		{
		}

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Avatar;
			FillSauceSize(sauce, document);
			FillFlags(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return document.Pages[0].Canvas.Size.Width != DefaultWidth || document.ICEColours || !document.IsUsingStandard8x16Font;
		}

		public override bool DetectAnimation(Stream stream)
		{
			// cannot detect animation yet..
			return false;
		}

		private void ReadChar(byte curChar)
		{
			if (ptCur.X >= rClip.Left && ptCur.X <= rClip.InnerRight)
				canvas[ptCur] = new CanvasElement(curChar, attribute);
			
			ptCur.X++;

			if (ptCur.X > rClip.InnerRight)
			{
				ptCur.Y++;
				if (ptCur.Y > rClip.InnerBottom)
				{
					canvas.ShiftUp();
					ptCur.Y = rClip.InnerBottom;
				}
				ptCur.X = rClip.Left;
			}
		}

		public override void Load(Stream fs, CharacterDocument doc, CharacterHandler handler)
		{
			BinaryReader br = new BinaryReader(fs);
			Page page = doc.Pages[0];

			canvas = page.Canvas;
			ResizeCanvasWidth(fs, doc, canvas);
			rClip = new Rectangle(0, 0, canvas.Width, canvas.Height);
			attribute.Foreground = 7;
			attribute.Background = 0;
			ptCur = rClip.Location;
			try
			{
				WaitEventArgs args = new WaitEventArgs();
				while (true)
				{
					doc.OnWait(args);
					if (args.Exit)
						break;
					byte curByte = br.ReadByte();
					switch (curByte)
					{
						case 10:
							ptCur.Y++;
							if (ptCur.Y > rClip.InnerBottom)
							{
								canvas.ShiftUp();
								ptCur.Y = rClip.InnerBottom;
								//throw new CanvasOverflowException();
							}
							ptCur.X = rClip.Left;
							break;
						case 13:
							// supposed to move X to 0, but for unix file types, it is omitted
							break;
						case 12:
							canvas.Fill(new CanvasElement(32, 7));
							attribute.Foreground = 7;
							attribute.Background = 0;
							ptCur = rClip.Location;
							break;
						case 25:
							{
								byte ch = br.ReadByte();
								int count = br.ReadByte();
								for (int i = 0; i < count; i++)
									ReadChar(ch);
								break;
							}
						case 22:
							{
								switch (br.ReadByte())
								{
									case 1:
										attribute = (byte)(br.ReadByte() & 0x7f);
										break;
									case 2:
										attribute.Blink = true;
										break;
									case 3:
										ptCur.Y--;
										if (ptCur.Y < rClip.Top)
											ptCur.Y = rClip.Top;
										break;
									case 4:
										ptCur.Y++;
										if (ptCur.Y > rClip.InnerBottom)
											ptCur.Y = rClip.InnerBottom;
										break;
									case 5:
										ptCur.X--;
										if (ptCur.X < rClip.Left)
											ptCur.X = rClip.Left;
										break;
									case 6:
										ptCur.X++;
										if (ptCur.X > rClip.InnerRight)
											ptCur.X = rClip.InnerRight;
										break;
									case 7:
										canvas.Fill(new Rectangle(ptCur, new Size(rClip.Width - ptCur.X, 1)), new CanvasElement(32, attribute));
										break;
									case 8:
										ptCur.X = br.ReadByte() - 1;
										ptCur.Y = br.ReadByte() - 1;
										break;
								}
								break;
							}
						default:
							ReadChar(curByte);
							break;
					}
				}
			}
			catch (EndOfStreamException)
			{
				// reached end of file, so we're okay
			}
			catch (CanvasOverflowException)
			{
				// everything's still okay, just went over the limits of the canvas
			}
			
			ResizeCanvasHeight(doc, canvas, ptCur.Y + 1);
		}
	}
}

