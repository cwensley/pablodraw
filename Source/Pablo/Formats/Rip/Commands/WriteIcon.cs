using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class WriteIcon : RipCommand
	{
		public class Type : RipCommandType<WriteIcon>
		{
			public override string OpCode { get { return "1W"; } }
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
		}
	}
}

