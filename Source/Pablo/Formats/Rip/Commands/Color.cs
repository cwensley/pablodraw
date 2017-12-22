using System;
using System.IO;
using System.Collections.Generic;
using Eto.Drawing;
using Pablo.BGI;

namespace Pablo.Formats.Rip.Commands
{
	public class Color : RipOptionalCommand
	{
		public class Type : RipCommandType<Color>
		{
			public override string OpCode { get { return "c"; } }
		}
		
		public override bool UndoPoint { get { return false; } }

		public int Value { get; set; }
		
		public override void Read (BinaryReader reader)
		{
			base.Read(reader);
			Value = reader.ReadRipWord ();
		}
		
		public override void Write (RipWriter writer)
		{
			base.Write (writer);
			writer.WriteWord (Value);
		}
		
		public override void Apply (IList<Rectangle> updates = null)
		{
			BGI.SetColor ((byte)Value);
		}

		public override bool InternalShouldApply (BGICanvas bgi)
		{
			return bgi.GetColor () != Value;
		}
		
		public override void Set (RipHandler handler, bool forDrawing)
		{
			if (forDrawing && (handler.BGI.Palette[handler.Foreground].ToArgb () & 0xFFFFFF) == 0)
				Value = 7;
			else
				Value = handler.Foreground;
		}
		
		public override void Set (Pablo.BGI.BGICanvas bgi)
		{
			Value = bgi.GetColor ();
		}
	}
}

