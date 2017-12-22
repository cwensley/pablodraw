using System;
using System.IO;
using Eto.Drawing;
using System.Text;

namespace Pablo.Formats.Character.Types
{
	public class Tundra : CharacterFormat
	{
		public const string TUNDRA_ID = "TUNDRA24";

		public Tundra(DocumentInfo info) : base(info, "tnd", "Tundra Draw", "tnd")
		{
		}

		public Tundra(DocumentInfo info, string id, string name, params string[] extensions) : base(info, id, name, extensions)
		{
		}

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.TundraDraw;
			FillSauceSize(sauce, document);
			document.ICEColours = true; // tundra always has ice colors on
			FillFlags(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return document.Pages[0].Canvas.Size.Width != DefaultWidth || !document.IsUsingStandard8x16Font;
		}

		public override bool CanSave
		{
			get { return true; }
		}

		public override bool CanAnimate
		{
			get { return false; }
		}

		public override void Save(Stream stream, CharacterDocument document)
		{
			Save(stream, document, true);
		}

		protected void Save(Stream stream, CharacterDocument document, bool asTundra)
		{
			var bw = new BinaryWriter(stream);
			if (asTundra)
			{
				bw.Write((byte)24);
				bw.Write(Encoding.ASCII.GetBytes(TUNDRA_ID));
			}
			
			var page = document.Pages[0];
			var canvas = page.Canvas;
			var pal = page.Palette;
			Save(stream, canvas, pal);
		}

		protected static void Save(Stream stream, Canvas canvas, Palette pal)
		{
			var bw = new BinaryWriter(stream);
			var endy = canvas.FindEndY(CanvasElement.Default);
			var attr = new Attribute(0, 0);
			var p = Point.Empty;
			bool blank = true;
			for (p.Y = 0; p.Y <= endy; p.Y++)
			{
				for (p.X = 0; p.X < canvas.Width; p.X++)
				{
					var ce = canvas[p];
					
					if (ce.Character > 6)
					{
						
						if (blank)
						{
							// write location
							bw.Write((byte)1);
							bw.WriteBigEndian(p.Y);
							bw.WriteBigEndian(p.X);
							
							blank = false;
						}
						
						byte color_notice = 0;
						// don't write foreground colour for space
						if (ce.Character != 32 && attr.Foreground != ce.Foreground)
						{
							color_notice |= 2;
							attr.Foreground = ce.Foreground;
						}

						if (attr.Background != ce.Background)
						{
							color_notice |= 4;
							attr.Background = ce.Background;
						}
						if (color_notice != 0)
						{
							bw.Write(color_notice);
						}
						
						// write the character
						bw.Write((byte)ce.Character);
						
						if (color_notice != 0)
						{
							if ((color_notice & 2) != 0)
							{
								var col = pal[attr.Foreground];
								bw.Write((byte)col.Ab);
								bw.Write((byte)col.Rb);
								bw.Write((byte)col.Gb);
								bw.Write((byte)col.Bb);
							}
							
							if ((color_notice & 4) != 0)
							{
								var col = pal[attr.Background];
								bw.Write((byte)(col.Ab));
								bw.Write((byte)(col.Rb));
								bw.Write((byte)(col.Gb));
								bw.Write((byte)(col.Bb));
							}
						}
						
					}
					else
						blank = true;
				}
			}
			bw.Flush();
		}

		static void InsertCharacter(Rectangle rClip, Canvas canvas, ref Point p, byte character, Attribute attr)
		{
			if (p.X > rClip.InnerRight)
			{
				p.X = rClip.Left;
				p.Y++;
			}

			if (rClip.Contains(p))
			{
				canvas[p] = new CanvasElement(character, attr);
			}
			p.X++;
		}

		public override void Load(Stream fs, CharacterDocument document, CharacterHandler handler)
		{
			var page = document.Pages[0];

			document.ICEColours = true;

			var br = new BinaryReader(fs);
			Canvas canvas = page.Canvas;
			
			Point p;
			ResizeCanvasWidth(fs, document, canvas);
			br.ReadByte(); // read version byte (unused)
			var id = Encoding.ASCII.GetString(br.ReadBytes(8)); // read signature ID
			if (id != TUNDRA_ID)
				throw new Exception("Not a valid tundra24 file");
				
			var pal = new Palette();
			pal.FindAddColour(Colors.Black);
				
			page.Palette = pal;
				
			p = Load(fs, canvas, pal);
			
			ResizeCanvasHeight(document, canvas, p.Y + 1);
		}

		protected static Point Load(Stream stream, Canvas canvas, Palette pal)
		{
			var rClip = new Rectangle(0, 0, canvas.Width, canvas.Height);
			Point p = rClip.Location;
			try
			{
				var br = new BinaryReader(stream);
				var attr = new Attribute(0, 0);
				while (true)
				{
					var command = br.ReadByte();
					
					if (command == 1)
					{
						// get position
						p.Y = br.ReadBigEndianInt32();
						p.X = br.ReadBigEndianInt32();
					}
					else if (command > 1 && command <= 6)
					{
						// character with specified foreground and/or background
						var ch = br.ReadByte();
						
						if ((command & 2) != 0)
						{
							// read in foreground
							br.ReadByte(); // alpha not used
							var r = br.ReadByte();
							var g = br.ReadByte();
							var b = br.ReadByte();
							attr.Foreground = pal.FindAddColour(Color.FromArgb(r, g, b));
						}
						if ((command & 4) != 0)
						{
							// read in background
							br.ReadByte(); // alpha not used
							var r = br.ReadByte();
							var g = br.ReadByte();
							var b = br.ReadByte();
							attr.Background = pal.FindAddColour(Color.FromArgb(r, g, b));
						}
						InsertCharacter(rClip, canvas, ref p, ch, attr);
					}
					else
					{
						// character with same colour as last
						InsertCharacter(rClip, canvas, ref p, command, attr);
					}
					
					if (p.Y > rClip.InnerBottom)
						break;
				}
			}
			catch (EndOfStreamException)
			{
			}
			return p;
		}
	}
}
