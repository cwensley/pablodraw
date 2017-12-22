using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Polygon : RipCommand
	{
		public class Type : RipCommandType<Polygon>
		{
			public override string OpCode { get { return "P"; } }
		}

		public Point[] Points { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			int count = reader.ReadRipWord ();
			Points = new Point[count];
			for (int i=0; i<count; i++) {
				Points [i] = reader.ReadRipPoint ();
			}
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord (Points.Length);
			for (int i=0; i<Points.Length; i++) {
				writer.Write (Points [i]);
			}
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.DrawPoly (Points, updates);
		}
	}
}

