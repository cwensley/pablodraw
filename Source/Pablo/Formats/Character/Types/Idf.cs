using System;
using System.IO;
using System.Text;
using Eto.Drawing;

namespace Pablo.Formats.Character.Types
{
	public class Idf : CharacterFormat
	{
		const string IDF_VER_1_3 = "\x00041.3";
		const string IDF_VER_1_4 = "\x00041.4";

		public Idf(DocumentInfo info) : base(info, "idf", "iCE Draw", "idf")
		{
		}

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Ansi;
			FillSauceSize(sauce, document);
			FillFlags(sauce, document);
		}

		public override bool? Use9pxFont
		{
			get { return false; }
		}

		public override bool CanAnimate
		{
			get { return false; }
		}

		class IdfHeader
		{
			byte[] idfVer;
			ushort x1;
			ushort y1;
			ushort x2;
			ushort y2;

			public IdfHeader()
			{
				idfVer = Encoding.ASCII.GetBytes(IDF_VER_1_4);
				x1 = 0;
				y1 = 0;
				x2 = 0;
				y2 = 0;
			}

			public IdfHeader(BinaryReader br)
			{
				idfVer = br.ReadBytes(4);
				x1 = br.ReadUInt16();
				y1 = br.ReadUInt16();
				x2 = br.ReadUInt16();
				y2 = br.ReadUInt16();
				if (Version == IDF_VER_1_3)
				{
					y2 = (ushort)(y1 + 199);
				}
			}

			public string Version
			{
				get { return Encoding.ASCII.GetString(idfVer); }
				set { idfVer = Encoding.ASCII.GetBytes(value); }
			}

			public Rectangle Bounds
			{
				get { return new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1); }
			}
		}

		protected override int? GetWidth(Stream stream, CharacterDocument document, object state = null)
		{
			var header = state as IdfHeader;
			if (header != null)
				return header.Bounds.Width;
			return base.GetWidth(stream, document, state);
		}

		public override void Load(Stream fs, CharacterDocument document, CharacterHandler handler)
		{
			Page page = document.Pages[0];

			var br = new BinaryReader(fs);
			var header = new IdfHeader(br);
			//Rectangle rClip = (resizeCanvas) ? new Rectangle(header.Bounds.Size) : header.Bounds;
			Canvas canvas = page.Canvas;
			ResizeCanvasWidth(fs, document, canvas, header); 
			
			var rClip = new Rectangle(canvas.Size);

			Point p = rClip.Location;
			try
			{
				UInt16 ccount;
				UInt16 cpair;
				
				long endpos = fs.Length - 4096 - 48;
				while (fs.Position < endpos)
				{
					ccount = 1;
					cpair = br.ReadUInt16();
					if (cpair == 1)
					{
						ccount = br.ReadUInt16(); 
						//ccount = (UInt16)(br.ReadUInt16() & 0xFF); // 16c way of doing it
						cpair = br.ReadUInt16();
					}
					if (cpair == 0)
						cpair = 0x0700;
					for (UInt16 i = 0; i < ccount; i++)
					{
						if (p.X >= rClip.Left && p.X < canvas.Width)
						{
							canvas[p] = new CanvasElement(new Character((byte)(cpair & 0xFF)), new Attribute((byte)(cpair >> 8)));
						}
						p.X++;
						if (p.X > rClip.InnerRight)
						{
							p.X = rClip.Left;
							p.Y++;
							if (p.Y > rClip.InnerBottom)
								break;
						}
					}
					if (p.Y > rClip.InnerBottom)
						break;
				}
				
				while (fs.Position < endpos)
					br.ReadByte();
				
				page.Font = new BitFont(256, 8, 16, BitFont.StandardCodePage);
				page.Font.Load(br);

				page.Palette.Load(br, 16, 2);
			}
			catch (EndOfStreamException)
			{
				// we're okay, just reached EOF.
			}

			ResizeCanvasHeight(document, canvas);
		}
	}
}
