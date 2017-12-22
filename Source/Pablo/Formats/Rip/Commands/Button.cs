using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;

namespace Pablo.Formats.Rip.Commands
{
	public class Button : RipCommand
	{
		public Rectangle Rectangle { get; set; }
		
		public ushort HotKey { get; set; }
		
		public byte Flags { get; set; }
		
		public int TopFlag { get; set; }
		
		public string Text { get; set; }
		
		public class Type : RipCommandType<Button>
		{
			public override string OpCode { get { return "U"; } }
		}

		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			Rectangle = reader.ReadRipRectangle ();
			HotKey = reader.ReadRipWord ();
			Flags = reader.ReadRipNumber ();
			reader.ReadRipNumber (); // reserved
			Text = reader.ReadRipString ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.Write (Rectangle);
			writer.WriteWord (HotKey);
			writer.WriteNumber (Flags);
			writer.WriteNumber (0);
			writer.Write (Text);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			//this.BGI.Bar3d(rect, Depth, TopFlag, updates);
		}
	}
}

