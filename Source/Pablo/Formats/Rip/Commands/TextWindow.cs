using System;
using System.IO;
using System.Collections;
using Eto.Drawing;
using System.Collections.Generic;

namespace Pablo.Formats.Rip.Commands
{
	public class TextWindow : RipCommand
	{
		public class Type : RipCommandType<TextWindow
			>
		{
			public override string OpCode { get { return "w"; } }
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
			//Window(Mega_Word,Mega_Word,Mega_Word,Mega_Word);
		}
	}
}

