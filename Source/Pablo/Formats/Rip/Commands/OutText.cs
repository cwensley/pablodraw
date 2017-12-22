using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class OutText : RipCommand
	{
		public class Type : RipCommandType<OutText>
		{
			public override string OpCode { get { return "T"; } }
		}

		public string Text { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			
			Text = reader.ReadRipString();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Text);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.OutText (Text, updates);
		}
	}
}

