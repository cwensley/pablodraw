using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Arc : RipCommand
	{
		public class Type : RipCommandType<Arc>
		{
			public override string OpCode { get { return "A"; } }
		}
		
		public Point Point { get; set; }

		public int StartAngle { get; set; }

		public int EndAngle { get; set; }

		public int Radius { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
			StartAngle = reader.ReadRipWord ();
			EndAngle = reader.ReadRipWord ();
			Radius = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.WriteWord (StartAngle);
			writer.WriteWord (EndAngle);
			writer.WriteWord (Radius);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.Arc (Point.X, Point.Y, StartAngle, EndAngle, Radius, updates);
		}
	}
}

