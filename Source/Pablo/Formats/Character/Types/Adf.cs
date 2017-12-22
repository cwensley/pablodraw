using System;
using System.IO;
using Eto.Drawing;

namespace Pablo.Formats.Character.Types
{
	public class Adf : CharacterFormat
	{
		public Adf (DocumentInfo info) : base(info, "adf", "Artworx", "adf")
		{
		}

		protected override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)Sauce.Types.Character.CharacterFileType.Ansi;
			FillSauceSize(sauce, document);
			FillFlags(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return base.RequiresSauce(document) || document.Pages[0].Canvas.Size.Width != DefaultWidth || document.ICEColours || !document.IsUsingStandard8x16Font;
		}

		public override bool? Use9pxFont {
			get { return false; }
		}

		public override void Load (Stream fs, CharacterDocument doc, CharacterHandler handler)
		{
			Page page = doc.Pages [0];

			BinaryReader br = new BinaryReader (fs);
			Canvas canvas = page.Canvas;
			Rectangle rClip = new Rectangle (0, 0, canvas.Width, canvas.Height);
			ResizeCanvasWidth (fs, doc, canvas);

			Point p = rClip.Location;
			try {
				br.ReadByte (); // read version byte (unused)
				Palette pal = new Palette ();
				pal.Load (br, 64, 0);

				/*
				TextWriter tw = new StreamWriter("c:\\blah.txt");
				for (int i=0; i<pal.Size; i++)
				tw.WriteLine("{0}, {1}, {2}", pal[i].R, pal[i].G, pal[i].B);
				tw.Close();
				*/

				page.Palette = Palette.FromEGA (pal);
				page.Font = new BitFont (256, 8, 16, BitFont.StandardCodePage);
				page.Font.Load (br);

				WaitEventArgs args = new WaitEventArgs ();
				while (true) {
					p.X = rClip.Left;
					for (int x=0; x<80; x++) {
						byte ch = br.ReadByte ();
						byte attr = br.ReadByte ();
						doc.OnWait (args);
						if (args.Exit)
							break;
						if (p.X <= rClip.InnerRight)
							canvas [p.X - rClip.Left, p.Y] = new CanvasElement (ch, attr);
						p.X++;
					}
					p.Y++;
					if (p.Y > rClip.InnerBottom)
						break;
				}

			} catch (EndOfStreamException) {
			}
			ResizeCanvasHeight (doc, canvas, p.Y + 1);
		}
	}
}
