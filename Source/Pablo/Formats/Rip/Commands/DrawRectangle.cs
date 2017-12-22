using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class DrawRectangle : RipCommand
	{
		public class Type : RipCommandType<DrawRectangle>
		{
			public override string OpCode { get { return "R"; } }
		}
		
		public Rectangle Rectangle { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read (reader);
			Rectangle = reader.ReadRipRectangle ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Rectangle);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.Rectangle (Rectangle.Left, Rectangle.Top, Rectangle.InnerRight, Rectangle.InnerBottom, updates);
		}
	}
}

