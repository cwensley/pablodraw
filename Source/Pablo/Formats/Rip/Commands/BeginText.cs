using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class BeginText : RipCommand
	{
		public class Type : RipCommandType<BeginText>
		{
			public override string OpCode { get { return "1T"; } }
		}
		
		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			reader.ReadRipRectangle ();
			reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			//base.Write (writer);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
		}
	}
}

