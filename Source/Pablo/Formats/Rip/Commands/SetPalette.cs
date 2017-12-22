using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class SetPalette : RipCommand
	{
		public class Type : RipCommandType<SetPalette>
		{
			public override string OpCode { get { return "Q"; } }
		}

		public int[] Palette { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			Palette = new int[16];
			for (int i=0; i<16; i++) {
				Palette [i] = reader.ReadRipWord ();
			}
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			for (int i=0; i<16; i++) {
				writer.WriteWord (Palette [i]);
			}
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetPalette (Palette, updates);
		}
	}
}

