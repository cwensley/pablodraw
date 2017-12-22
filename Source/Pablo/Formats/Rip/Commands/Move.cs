using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Move : RipCommand
	{
		public class Type : RipCommandType<Move>
		{
			public override string OpCode { get { return "m"; } }
		}

		public Point Point { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.MoveTo (Point.X, Point.Y);
		}
	}
}

