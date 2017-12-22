using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class OvalArc : RipCommand
	{
		public class Type : RipCommandType<OvalArc>
		{
			public override string OpCode { get { return "V"; } }
		}

		public Point Point { get; set; }
		public int StartAngle { get; set; }
		public int EndAngle { get; set; }
		public Size Radius { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			Point = reader.ReadRipPoint ();
			StartAngle = reader.ReadRipWord ();
			EndAngle = reader.ReadRipWord ();
			Radius = reader.ReadRipSize ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.WriteWord (StartAngle);
			writer.WriteWord (EndAngle);
			writer.Write (Radius);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.Ellipse(Point.X, Point.Y, StartAngle, EndAngle, Radius.Width, Radius.Height, updates);
		}
	}
}

