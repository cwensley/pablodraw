using System;
using System.IO;
using System.Text;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class OutTextXY : RipCommand
	{
		public class Type : RipCommandType<OutTextXY>
		{
			public override string OpCode { get { return "@"; } }
		}

		public Point Point { get; set; }
		
		public string Text { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
			Text = reader.ReadRipString ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.Write (Text);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.OutTextXY (Point.X, Point.Y, Text, updates);
		}
	}
}

