using System;
using System.IO;
using Eto.Drawing;
using Pablo.BGI;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class PutImage : RipCommand
	{
		public class Type : RipCommandType<PutImage>
		{
			public override string OpCode { get { return "1P"; } }
		}

		public Point Point { get; set; }

		public BGICanvas.WriteMode WriteMode { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Point = reader.ReadRipPoint ();
			WriteMode = (BGICanvas.WriteMode)reader.ReadRipWord ();
			reader.ReadRipNumber ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Point);
			writer.WriteWord ((int)WriteMode);
			writer.WriteNumber (0);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.PutImage (Point.X, Point.Y, Document.RipImage, WriteMode, updates);
		}
	}
}

