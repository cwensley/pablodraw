using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class FilledOval : RipCommand
	{
		public class Type : RipCommandType<FilledOval>
		{
			public override string OpCode { get { return "o"; } }
		}

		public Point Point { get; set; }

		public Size Radius { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
			Radius = reader.ReadRipSize ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.Write (Radius);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.FillEllipse (Point.X, Point.Y, Radius.Width, Radius.Height, updates);
		}
	}
}

