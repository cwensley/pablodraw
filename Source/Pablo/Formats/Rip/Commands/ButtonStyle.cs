using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class ButtonStyle : RipCommand
	{
		public class Type : RipCommandType<ButtonStyle>
		{
			public override string OpCode { get { return "1B"; } }
		}

		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipInt ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			reader.ReadRipWord ();
			
			reader.ReadRipWord ();
			reader.ReadRipWord ();
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

