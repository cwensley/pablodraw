using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Pixel : RipCommand, ICloneable
	{
		bool undoPoint = true;
		
		public class Type : RipCommandType<Pixel>
		{
			public override string OpCode { get { return "X"; } }
		}

		public Point Point { get; set; }

		public override bool UndoPoint {
			get { return undoPoint; }
		}
		
		public void SetUndoPoint(bool undoPoint)
		{
			this.undoPoint = undoPoint;
		}

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
			BGI.PutPixel (Point.X, Point.Y, BGI.GetColor (), updates);
		}

		#region ICloneable implementation
		public object Clone ()
		{
			return new Pixel{
				Document = Document,
				CommandType = CommandType,
				Point = Point
			};
		}
		#endregion
	}
}

