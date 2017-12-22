using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Line : RipCommand, ICloneable
	{
		public class Type : RipCommandType<Line>
		{
			public override string OpCode { get { return "L"; } }
		}

		public Point Start { get; set; }

		public Point End { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Start = reader.ReadRipPoint ();
			End = reader.ReadRipPoint ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Start);
			writer.Write (End);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.Line (Start.X, Start.Y, End.X, End.Y, updates);
		}

		public object Clone ()
		{
			return new Line{
				Document = Document,
				CommandType = CommandType,
				Start = Start,
				End = End
			};
		}
		
		public override string ToString ()
		{
			return string.Format ("[Line: Start={0}, End={1}]", Start, End);
		}
	}
}

