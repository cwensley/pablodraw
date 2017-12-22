using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class LoadIcon : RipCommand
	{
		public class Type : RipCommandType<LoadIcon>
		{
			public override string OpCode { get { return "1I"; } }
		}

		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			reader.ReadRipPoint();
			reader.ReadRipWord();
			reader.ReadRipNumber();
			reader.ReadRipWord();
			reader.ReadRipString();
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

