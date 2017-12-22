using System;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class EraseEOL : RipCommand
	{
		public class Type : RipCommandType<EraseEOL>
		{
			public override string OpCode { get { return ">"; } }
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
			// ClrEOL;
		}
	}
}

