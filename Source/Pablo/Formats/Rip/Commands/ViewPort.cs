using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class ViewPort : RipCommand
	{
		public class Type : RipCommandType<ViewPort>
		{
			public override string OpCode { get { return "v"; } }
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
			//SetViewPort(Mega_Word,Mega_Word,Mega_Word,Mega_Word,True);
		}
	}
}

