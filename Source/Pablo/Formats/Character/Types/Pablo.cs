using System;
using System.IO;
using Eto.Drawing;
using Pablo.Network;
using System.Linq;

namespace Pablo.Formats.Character.Types
{
	public class Pablo : Tundra
	{
		public Pablo (DocumentInfo info) : base(info, "pd", "PabloDraw format", "pd")
		{
		}

		public override void Load (Stream fs, CharacterDocument document, CharacterHandler handler)
		{
			var page = document.Pages [0];
			
			

			var br = new BinaryReader(fs);
			var fontID = br.ReadString ();
			if (string.IsNullOrEmpty(fontID))
				page.Font = new BitFont(fs, BitFont.StandardCodePage);
			else {
				page.Font = document.Info.GetFonts ().SelectMany(r => r.Fonts).FirstOrDefault (r => r.ID == fontID);
			}
			var palSize = br.ReadInt16 ();
			var canvas = page.Canvas;
			
			var pal = page.Palette;
			pal.Load (br, palSize, 0);
			
			ResizeCanvasWidth (fs, document, canvas);
			
			var p = Load (fs, canvas, pal);
			
			ResizeCanvasHeight (document, canvas, p.Y + 1);
		}
		
		public new static Point Load(Stream stream, Canvas canvas, Palette pal)
		{
			return Tundra.Load(stream, canvas, pal);
		}
		
		public new static void Save(Stream stream, Canvas canvas, Palette pal)
		{
			Tundra.Save(stream, canvas, pal);
		}

		public static void Send (Lidgren.Network.NetOutgoingMessage message, Canvas canvas, Palette pal)
		{
			using (var stream = new MemoryStream()) {
			
				Pablo.Save (stream, canvas, pal);
				stream.Flush ();
				stream.Seek (0, SeekOrigin.Begin);
				
				message.WriteVariableInt32 (canvas.Width);
				message.WriteVariableInt32 (canvas.Height);
				message.WritePadBits ();
				message.WriteStream ((Stream)stream);
			}
		}

		public static Canvas Receive (Lidgren.Network.NetIncomingMessage message, Palette pal)
		{
			var width = message.ReadVariableInt32 ();
			var height = message.ReadVariableInt32 ();
			message.ReadPadBits ();
			var stream = message.ReadStream();
			
			var canvas = new MemoryCanvas(new Size(width, height));
			
			Pablo.Load (stream, canvas, pal);
			return canvas;
		}
		
		public override void Save (Stream stream, CharacterDocument document)
		{
			var page = document.Pages [0];
			var bw = new BinaryWriter(stream);
			
			if (page.Font.IsSystemFont)
				bw.Write (page.Font.ID);
			else {
				bw.Write (string.Empty);
				page.Font.FullSave (bw);	
			}
			bw.Write ((Int16)page.Palette.Count);
			page.Palette.Save (bw, 0);
			
			Save (stream, page.Canvas, page.Palette);
		}
	}
}

