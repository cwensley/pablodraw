using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class Home : RipCommand
	{
		public class Type : RipCommandType<Home>
		{
			public override string OpCode { get { return "H"; } }
		}

		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			//    GotoXY(1,1);
		}
	}
}

