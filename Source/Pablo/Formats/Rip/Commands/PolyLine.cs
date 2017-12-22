using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class PolyLine : RipCommand
	{
		public class Type : RipCommandType<PolyLine>
		{
			public override string OpCode { get { return "l"; } }
		}

		public Point[] Points { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			int count = reader.ReadRipWord ();
			Points = new Point[count];
			for (int i=0; i<count; i++) {
				Points [i] = reader.ReadRipPoint ();
			}
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord (Points.Length);
			for (int i=0; i<Points.Length; i++) {
				writer.Write (Points [i]);
			}
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			if (Points != null && Points.Length > 1) {
				var drawUpdates = updates ?? new List<Rectangle> ();
				BGI.MoveTo (Points [0]);
				for (int i=1; i<Points.Length; i++) {
					BGI.LineTo (Points [i], drawUpdates);
				}
				if (updates == null)
					BGI.UpdateRegion (drawUpdates);
			}
		}
	}
}

