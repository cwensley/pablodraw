using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Circle : RipCommand
	{
		public class Type : RipCommandType<Circle>
		{
			public override string OpCode { get { return "C"; } }
		}

		public Point Point { get; set; }

		public int Radius { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
			Radius = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.WriteWord (Radius);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.Circle (Point.X, Point.Y, Radius, updates);
		}
	}
}

