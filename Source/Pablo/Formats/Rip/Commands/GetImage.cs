using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class GetImage : RipCommand
	{
		public class Type : RipCommandType<GetImage>
		{
			public override string OpCode { get { return "1C"; } }
		}

		public Eto.Drawing.Rectangle Rectangle { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			Rectangle = reader.ReadRipRectangle();
			reader.ReadRipNumber ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Rectangle);
			writer.WriteNumber (0);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			Document.RipImage = BGI.GetImage (Rectangle.Left, Rectangle.Top, Rectangle.InnerRight, Rectangle.InnerBottom);
		}
	}
}

