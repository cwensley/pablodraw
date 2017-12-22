using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Fill : RipCommand
	{
		public class Type : RipCommandType<Fill>
		{
			public override string OpCode { get { return "F"; } }
		}

		public Point Point { get; set; }

		public int Border { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
			Border = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.WriteWord (Border);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.FloodFill (Point.X, Point.Y, Border, updates);
		}
	}
}

