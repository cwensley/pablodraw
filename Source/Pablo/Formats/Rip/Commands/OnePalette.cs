using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class OnePalette : RipCommand
	{
		public class Type : RipCommandType<OnePalette>
		{
			public override string OpCode { get { return "a"; } }
		}

		public int Color { get; set; }

		public int Palette { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Color = reader.ReadRipWord ();
			Palette = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord (Color);
			writer.WriteWord (Palette);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetPalette (Color, Palette, updates);
		}
	}
}

