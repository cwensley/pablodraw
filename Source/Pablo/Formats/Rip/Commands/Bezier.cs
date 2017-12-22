using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Bezier : RipCommand
	{
		public class Type : RipCommandType<Bezier>
		{
			public override string OpCode { get { return "Z"; } }
		}

		public Point[] Points { get; set; }
		
		public int Segments { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Points = new Point[4];

			for (int i=0; i<4; i++) {
				Points [i] = reader.ReadRipPoint ();
			}

			Segments = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			if (Points == null)
				Points = new Point[4];
			for (int i=0; i<4; i++) {
				writer.Write (Points [i]);
			}
			
			writer.WriteWord (Segments);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.DrawBezier (Points.Length, Points, Segments, updates);
		}
	}
}

