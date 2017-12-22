using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class EraseWindow : RipCommand
	{
		public class Type : RipCommandType<EraseWindow>
		{
			public override string OpCode { get { return "e"; } }
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
			BGI.ClearDevice (updates);
		}
	}
}

