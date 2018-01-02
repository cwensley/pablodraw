using Eto.Drawing;
using System.IO;
using System.Reflection;

namespace Pablo.Formats.Character.Types
{
	public class CG : CharacterFormat
	{
		public CG(DocumentInfo info)
			: base(info, "cg", "CG / Seq", "seq", "cg")
		{
		}

		public override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Ansi;
			FillSauceSize(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return base.RequiresSauce(document) || document.Pages[0].Canvas.Size.Width != DefaultWidth || document.ICEColours;
		}

		public static Palette GetCGPalette()
		{
			var pal = new Palette();
			pal.Add(Color.FromArgb(0, 0, 0));
			pal.Add(Color.FromArgb(255, 255, 255));
			pal.Add(Color.FromArgb(189, 24, 33));
			pal.Add(Color.FromArgb(49, 231, 198));
			pal.Add(Color.FromArgb(181, 24, 231));
			pal.Add(Color.FromArgb(24, 214, 24));
			pal.Add(Color.FromArgb(33, 24, 173));
			pal.Add(Color.FromArgb(222, 247, 8));
			pal.Add(Color.FromArgb(189, 66, 0));
			pal.Add(Color.FromArgb(107, 49, 0));
			pal.Add(Color.FromArgb(255, 74, 82));
			pal.Add(Color.FromArgb(66, 66, 66));
			pal.Add(Color.FromArgb(115, 115, 115));
			pal.Add(Color.FromArgb(90, 255, 90));
			pal.Add(Color.FromArgb(90, 82, 255));
			pal.Add(Color.FromArgb(165, 165, 165));
			return pal;
		}

		public override bool DetectAnimation(Stream stream)
		{
			long pos = stream.Position;
			try
			{

				var br = new BinaryReader(stream);
				while (true)
				{
					byte b = br.ReadByte();
					switch (b)
					{
						case 20:
						case 145:
						case 157:
							return true;

					}
				}
			}
			catch (EndOfStreamException)
			{

			}
			finally
			{
				stream.Position = pos;
			}
			return false;
		}

		public override bool? Use9pxFont
		{
			get { return false; }
		}

		public static BitFont GetCGLowerFont()
		{
			using (var fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Pablo.Formats.Character.Fonts.cg-lower.fnt"))
			{
				BitFont f = null;
				var br = new BinaryReader(fontStream);
				f = new BitFont(256, 8, 8, BitFont.StandardCodePage);
				f.Load(br);
				f.Resize(16, 16, true);
				fontStream.Close();
				return f;
			}
		}

		public static BitFont GetCGUpperFont()
		{
			using (var fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Pablo.Formats.Character.Fonts.cg-upper.fnt"))
			{
				BitFont f = null;
				var br = new BinaryReader(fontStream);
				f = new BitFont(256, 8, 8, BitFont.StandardCodePage);
				f.Load(br);
				f.Resize(16, 16, true);
				fontStream.Close();
				return f;
			}
		}

		static readonly byte[] Pet2Screen =
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 33, 34, 35, 36, 37, 38, 39,
			40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
			60, 61, 62, 63, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
			16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 64, 65, 66, 67,
			68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87,
			88, 89, 90, 91, 92, 93, 94, 95, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			32, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115,
			116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 64, 65, 66, 67, 68, 69, 70, 71,
			72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91,
			92, 93, 94, 95, 32, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
			112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 94
		};

		protected override int DefaultWidth
		{
			get
			{
				return 40;
			}
		}

		public override void Load(Stream fs, CharacterDocument document, CharacterHandler handler)
		{
			Page page = document.Pages[0];

			var br = new BinaryReader(fs);
			var canvas = page.Canvas;
			ResizeCanvasWidth(fs, document, canvas);
			var rClip = new Rectangle(0, 0, canvas.Width, canvas.Height);

			Point p = rClip.Location;
			try
			{
				page.Palette = GetCGPalette();
				page.Font = GetCGLowerFont();
				bool rvson = false;

				var ce = new CanvasElement(32, 7);
				p = rClip.Location;
				var args = new WaitEventArgs();
				while (true)
				{
					document.OnWait(args);
					if (args.Exit)
						break;
					byte b = br.ReadByte();
					switch (b)
					{
						case 0:
							break;
						case 1:
							break;
						case 2:
							break;
						case 3:
							break;
						case 4:
							break;
						case 5:
							ce.Attribute = 1;
							break;
						case 6:
							break;
						case 7:
							break;
						case 8:
							break;
						case 9:
							break;
						case 10:
							break;
						case 11:
							break;
						case 12:
							break;
						case 14: // shift out
							page.Font = GetCGLowerFont();
							break;
						case 15:
							break;
						case 16:
							break;
						case 17:
							p.Y++;
							break;
						case 18:
							rvson = true;
							break; //inv
						case 19:
							p.X = rClip.Left;    //home
							p.Y = rClip.Top;
							break;
						case 20:
							if (p.X > rClip.Left)
							{
								p.X--;
								ce.Character = 32;
								canvas.DeleteCharacter(p.X, p.Y, ce);
							}
							else if (p.Y > rClip.Top)
							{
								p.X = rClip.InnerRight;
								p.Y--;
								ce.Character = 32;
								canvas[p] = ce;
							}
							break;
						case 21:
							break;
						case 22:
							break;
						case 23:
							break;
						case 24:
							break;
						case 25:
							break;
						case 26:
							break;
						case 27:
							break;

						case 28:
							ce.Attribute = 2;
							break;
						case 29:
							p.X++;
							if (p.X > rClip.InnerRight)
							{
								p.X = rClip.Left;
								p.Y++;
							}
							break;
						case 30:
							ce.Attribute = 5;
							break;
						case 31:
							ce.Attribute = 6;
							break;
						case 128:
							break;
						case 130:
							break;
						case 131:
							break;
						case 132:
							break;
						case 133:
							break;
						case 134:
							break;
						case 135:
							break;
						case 136:
							break;
						case 137:
							break;
						case 138:
							break;
						case 139:
							break;
						case 140:
							break;

						case 13:
						case 141:
							p.Y++;
							rvson = false;
							p.X = rClip.Left;
							break;
						case 142: // shift in
							page.Font = GetCGUpperFont();
							break;
						case 143:
							break;
						case 144:  // black
							ce.Attribute = 0;
							break;
						case 145:  // cursor up
							if (p.Y > rClip.Top)
								p.Y--;
							break;
						case 146:     //rvs off
							rvson = false;
							break;
						case 147:     //clear home
							p.X = rClip.Left;
							p.Y = rClip.Top;
							canvas.Fill(rClip, new CanvasElement(32, 7));
							break;
						case 148:     //shift delete
							ce.Character = 32;
							canvas.InsertCharacter(p.X, p.Y, ce);
							break;

						case 158:
							ce.Attribute = 7;
							break;
						case 159:
							ce.Attribute = 3;
							break;
						case 156:
							ce.Attribute = 4;
							break;
						case 129:
							ce.Attribute = 8;
							break;
						case 149:
							ce.Attribute = 9;
							break;
						case 150:
							ce.Attribute = 10;
							break;
						case 151:
							ce.Attribute = 11;
							break;
						case 152:
							ce.Attribute = 12;
							break;
						case 153:
							ce.Attribute = 13;
							break;
						case 154:
							ce.Attribute = 14;
							break;
						case 155:
							ce.Attribute = 15;
							break;
						case 157:
							if (p.X > rClip.Left)
							{
								p.X--;
							}
							else if (p.Y > rClip.Top)
							{
								p.X = rClip.InnerRight;
								p.Y--;
							}
							break;
						default:
							byte pcode = Pet2Screen[b];
							if (rvson)
								pcode += 128;
							ce.Character = pcode;
							if (p.X < canvas.Size.Width && p.Y < canvas.Size.Height)
								canvas[p] = ce;
							p.X++;
							if (p.X > rClip.InnerRight)
							{
								p.X = rClip.Left;
								p.Y++;
							}
							break;
					}

					while (p.Y > rClip.InnerBottom)
					{
						p.Y--;
						canvas.ShiftUp();
					}
				}

			}
			catch (EndOfStreamException)
			{
			}
			ResizeCanvasHeight(document, canvas, p.Y + 1);
		}
	}
}
