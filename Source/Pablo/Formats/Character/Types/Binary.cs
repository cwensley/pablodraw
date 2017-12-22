using System;
using System.IO;
using Eto.Drawing;
using Pablo.Sauce;
using System.Collections.Generic;

namespace Pablo.Formats.Character.Types
{
	public class Binary : CharacterFormat
	{

		public override SauceDataType GetSauceDataType(CharacterDocument document)
		{
			return SauceDataType.BinaryText;
		}

		protected override void FillSauce(SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = (byte)(document.Pages[0].Canvas.Size.Width / 2);
			FillFlags(sauce, document);
		}

		public override bool RequiresSauce(CharacterDocument document)
		{
			return document.Pages[0].Canvas.Size.Width != 160 || document.ICEColours || !document.IsUsingStandard8x16Font;
		}

		public override void Save (Stream stream, CharacterDocument document)
		{
			Page page = document.Pages [0];
			var canvas = page.Canvas;
            
			int maxY = page.Canvas.FindEndY (new CanvasElement (32, 7));
			for (int y = 0; y<maxY; y++)
				for (int x = 0; x<canvas.Size.Width; x++) {
					var elem = canvas [x, y];
					stream.WriteByte (elem.Character);
					stream.WriteByte ((byte)elem.Attribute);
				}
		}
        
		FormatParameter fpBinaryWidth;

		public Binary (DocumentInfo info) : base(info, "binary", "Binary", "bin")
		{
			fpBinaryWidth = new FormatParameter ("binwidth", "Bin width", typeof(int), 160);
		}
		
		public override bool CanSave {
			get { return true; }
		}
        
		public override IEnumerable<FormatParameter> GetParameters (SauceInfo sauce)
		{
			foreach (var value in base.GetParameters(sauce))
			{
				yield return value;
			}
			
			if (sauce != null && sauce.ByteFileType > 1) {
				BinaryWidth = sauce.ByteFileType * 2;
				fpBinaryWidth.Enabled = false; // not needed
			} else
				fpBinaryWidth.Enabled = true;
			yield return fpBinaryWidth;
		}

		public int BinaryWidth {
			get { return Convert.ToInt32 (fpBinaryWidth.Value); }
			set { fpBinaryWidth.Value = value; }
		}
		
		protected override int DefaultWidth {
			get {
				return 160;
			}
		}
		
		protected override int GetWidth (Stream stream, CharacterDocument document, object state)
		{
			if (!fpBinaryWidth.Enabled) return BinaryWidth;
			else if (document.Sauce != null && document.Sauce.ByteFileType > 1) return document.Sauce.ByteFileType * 2;
			else return DefaultWidth;
		}

		public override void Load (Stream fs, CharacterDocument doc, CharacterHandler handler)
		{
			Page page = doc.Pages [0];

			BinaryReader br = new BinaryReader (fs);
			Canvas canvas = page.Canvas;
			ResizeCanvasWidth(fs, doc, canvas);
			Rectangle rClip = new Rectangle (0, 0, canvas.Width, canvas.Height);

			Point p = rClip.Location;
			try {
				//page.Palette = Palette.GetDefaultPalette ();
				//page.Font = BitFont.GetStandard8x16 ();

				CanvasElement ce = new CanvasElement (32, 7);
				p = rClip.Location;
				WaitEventArgs args = new WaitEventArgs ();
				while (true) {
					doc.OnWait (args);
					if (args.Exit)
						break;
                 
					ce.Character = br.ReadByte ();
					ce.Attribute = br.ReadByte ();
					if (p.X < canvas.Size.Width && p.Y < canvas.Size.Height)
						canvas [p] = ce;
					p.X++;
					if (p.X > rClip.InnerRight) {
						p.X = rClip.Left;
						p.Y++;
						if (p.Y > rClip.InnerBottom) {
							canvas.ShiftUp ();
							p.Y = rClip.InnerBottom;
						}
					}
				}

			} catch (EndOfStreamException) {
			}
			ResizeCanvasHeight (doc, canvas, p.Y + 1);
		}
	}
}
